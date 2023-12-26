using Configgy;
using Steamworks;
using Steamworks.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace EasyPZ.Components
{
    public class TrackerManager : MonoBehaviour
    {
        [Configgable("Config", "Tracker Type")]
        private static ConfigDropdown<TrackerType> CFG_trackerType = new ConfigDropdown<TrackerType>((TrackerType[])Enum.GetValues(typeof(TrackerType)), ((TrackerType[])Enum.GetValues(typeof(TrackerType))).Select(x => x.ToString()).ToArray(), 1);

        [Configgable("Config", "Always Show Tracker")]
        private static ConfigToggle CFG_AlwaysShowTracker = new ConfigToggle(true);

        [Configgable("Config", "Enable Auto Restart By Default")]
        private static ConfigToggle CFG_AutoResetDefault = new ConfigToggle(true);

        [Configgable("Config/Goal", "Use P-Rank Time as PB Goal")]
        private static ConfigToggle CFG_UsePRankForLeaderboardGoals = new ConfigToggle(false);

        [Configgable("Config/Goal", "Goal Mode")]
        private static ConfigDropdown<GoalMode> CFG_GoalMode = new ConfigDropdown<GoalMode>((GoalMode[])Enum.GetValues(typeof(GoalMode)), GoalModeDescriptions, 0);
        
        private static readonly string[] GoalModeDescriptions = new string[]
        {
            "P-Rank",
            "Custom",
            "Personal Best",
            "Top Friend",
            "Next Highest Friend",
        };

        #region CFG_CustomGoal

        [Configgable("Config/Goal/Custom Goal", "Kills")]
        private static ConfigInputField<int> CFG_CustomGoalKills = new ConfigInputField<int>(0);

        [Configgable("Config/Goal/Custom Goal", "Seconds")]
        private static ConfigInputField<float> CFG_CustomGoalSeconds = new ConfigInputField<float>(120f);

        [Configgable("Config/Goal/Custom Goal", "Deaths")]
        private static ConfigInputField<int> CFG_CustomGoalDeaths = new ConfigInputField<int>(0);

        [Configgable("Config/Goal/Custom Goal", "Style")]
        private static ConfigInputField<int> CFG_CustomGoalStyle = new ConfigInputField<int>(2000);
        #endregion

        [SerializeField] private UnityEngine.UI.Image blocker;
        [SerializeField] private GameObject container;

        private RectTransform content;

        private GameObject tracker;
        private LevelStats levelStats;

        private bool editMode;
        public static bool AutoRestartEnabled;

        private StatGoal currentStatGoal;

        private void Awake()
        {
            instance = this;
            levelStats = FindObjectOfType<LevelStatsEnabler>()?.GetComponentInChildren<LevelStats>();
            blocker.gameObject.SetActive(false);
            content = container.GetComponent<RectTransform>();
            
        }

        private void Start()
        {
            CFG_trackerType.OnValueChanged += InstanceTracker;
            CFG_UsePRankForLeaderboardGoals.OnValueChanged += (v) => InitializeGoal();
            CFG_GoalMode.OnValueChanged += (v) => InitializeGoal();

            CFG_CustomGoalDeaths.OnValueChanged += (v) => InitializeGoal();
            CFG_CustomGoalKills.OnValueChanged += (v) => InitializeGoal();
            CFG_CustomGoalStyle.OnValueChanged += (v) => InitializeGoal();
            CFG_CustomGoalSeconds.OnValueChanged += (v) => InitializeGoal();


            if (InGameCheck.InLevel())
            {
                InitializeGoal();
                InstanceTracker(CFG_trackerType.Value);
                gameObject.AddComponent<SessionRecorder>();
                gameObject.AddComponent<GhostManager>();
            }

            if (CFG_AutoResetDefault.Value)
                AutoRestartEnabled = true;
        }

        private void InitializeGoal()
        {
            SetGoal(GetCurrentGoal());
        }

        private void InstanceTracker(TrackerType trackerType)
        {
            if (tracker != null)
            {
                GameObject destroy = tracker;
                Destroy(destroy);
                tracker = null;
            }

            GameObject prefab = null;

            switch (trackerType)
            {
                case TrackerType.Compact:
                    prefab = Prefabs.CompactTracker;
                    break;
                case TrackerType.Modern:
                case TrackerType.Classic:
                    prefab = Prefabs.ClassicTrackerPrefab;
                    break;
            }

            tracker = Instantiate(prefab, content);
            if (tracker.TryGetComponent(out IEZPZTracker iTracker))
                iTracker.SetStatGoal(currentStatGoal);
        }


        private void SetGoal(StatGoal goal)
        {
            currentStatGoal = goal;
            if (tracker != null)
                if (tracker.TryGetComponent<IEZPZTracker>(out IEZPZTracker iTracker))
                    iTracker.SetStatGoal(currentStatGoal);
        }

        

        private void Update()
        {
            if (!InGameCheck.InLevel())
                return;

            if (tracker == null)
            {
                InitializeGoal();
                InstanceTracker(CFG_trackerType.Value);
            }

            if (editMode)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                    EndEditMode();

                return;
            }

            //Copy level stats active state to tracker.
            if (!CFG_AlwaysShowTracker.Value)
            {
                if (levelStats != null)
                {
                    if (container.activeSelf != levelStats.gameObject.activeInHierarchy)
                        container.SetActive(levelStats.gameObject.activeInHierarchy);
                }
            }
            else
            {
                if (!container.activeSelf)
                    container.SetActive(true);
            }

            if (EasyPZ.Key_PModeToggle.WasPerformedThisFrame)
            {
                AutoRestartEnabled = !AutoRestartEnabled;
            }

            if (EasyPZ.Key_RestartMission.WasPerformedThisFrame)
            {
                OptionsManager.Instance.RestartMission();
            }

            if (AutoRestartEnabled)
            {
                if (currentStatGoal.NotEmpty())
                    if (currentStatGoal.IsFailed())
                        OptionsManager.Instance.RestartMission();
            }
        }

        //[Configgable("Options", "Open Tracker Editor")]
        private static void EnterEditMode()
        {
            if (instance == null)
                return;

            instance.EnterEditModeInternal();
        }


        private void EnterEditModeInternal()
        {
            editMode = true;
            blocker.gameObject.SetActive(true);
            container.SetActive(true);

            if (tracker.TryGetComponent(out IUIEditable editable))
            {
                editable.StartEditMode();
            }
        }

        private void EndEditMode()
        {
            if (tracker.TryGetComponent(out IUIEditable editable))
            {
                editable.EndEditMode();
            }

            blocker.gameObject.SetActive(false);
            editMode = false;
        }

        private StatGoal GetCurrentGoal()
        {
            switch (CFG_GoalMode.Value)
            {
                case GoalMode.LevelPRank:
                    return GetLevelPRankGoal();
                case GoalMode.PersonalBestTime:
                case GoalMode.HighestFriendTime:
                case GoalMode.NextHighestFriendTime:
                    if(!CanUseOnlineRecords())
                        return GetLevelPRankGoal();
                    SetGoalToPBGoal();
                    return currentStatGoal;
                default:
                    return GetCustomGoal();
            }
        }


        private void SetGoalToPBGoal()
        {
            if (retrievingScores)
                return;

            try
            {
                StartCoroutine(RetrieveLevelScores());
            }catch(System.Exception ex)
            {
                Debug.LogError("Error occured while trying to fetch scores.");
                Debug.LogException(ex);

                //Retry in 1 second.
                if (CanUseOnlineRecords())
                    Invoke(nameof(SetGoalToPBGoal), 1f);
            }
        }

        private bool CanUseOnlineRecords()
        {
            return InGameCheck.CurrentLevelType == InGameCheck.UKLevelType.Level || InGameCheck.CurrentLevelType == InGameCheck.UKLevelType.PrimeSanctum && SteamClient.IsValid;
        }

        private bool retrievingScores;

        private IEnumerator RetrieveLevelScores()
        {
            retrievingScores = true;

            string levelName = SceneHelper.CurrentScene;
            if(LeaderboardController.Instance == null)
            {
                retrievingScores = false;
                yield break;
            }

            Task<LeaderboardEntry[]> task = LeaderboardController.Instance.GetLevelScores(levelName, CFG_UsePRankForLeaderboardGoals.Value);
            
            if(task == null)
            {
                retrievingScores = false;
                yield break;
            }

            while (!task.IsCompleted)
                yield return null;

            if (task.Result == null)
            {
                retrievingScores = false;
                Debug.LogError("Failed to retrieve leaderboard scores.");
                yield break;
            }

            OnLevelScoresReceived(task.Result);
            retrievingScores = false;
        }

        private void OnLevelScoresReceived(LeaderboardEntry[] scores)
        {
            StatGoal goal;

            switch (CFG_GoalMode.Value)
            {
                case GoalMode.PersonalBestTime:
                    goal = GetPersonalGoal(scores);
                    break;
                case GoalMode.HighestFriendTime:
                    goal = GetTopFriendGoal(scores);
                    break;
                case GoalMode.NextHighestFriendTime:
                    goal = GetNextHighestFriendGoal(scores);
                    break;
                default:
                    goal = GetLevelPRankGoal();
                    break;
            }

            if (CFG_UsePRankForLeaderboardGoals.Value)
            {
                StatGoal prank = GetLevelPRankGoal();
                goal.Deaths = 0;
                goal.Kills = prank.Kills;
                goal.Style = prank.Style;
            }

            SetGoal(goal);
        }

        private StatGoal GetNextHighestFriendGoal(LeaderboardEntry[] entries)
        {
            entries = entries.OrderBy(x => x.Score).ToArray();

            for (int i = 0; i < entries.Length; i++)
            {
                if (!entries[i].User.IsMe)
                    continue;

                if (i == 0)
                    return GetGoalFromLeaderboard(entries[i]);
                else
                    return GetGoalFromLeaderboard(entries[i - 1]);
            }

            //Return last user in list.
            return GetGoalFromLeaderboard(entries[entries.Length-1]);
        }

        private StatGoal GetPersonalGoal(LeaderboardEntry[] entries)
        {
            for (int i = 0; i < entries.Length; i++)
            {
                if (!entries[i].User.IsMe)
                    continue;

                return GetGoalFromLeaderboard(entries[i]);
            }

            Debug.Log($"EZPZ: No leaderboard score could be found. Using P-Rank instead.");
            return GetLevelPRankGoal();
        }

        private StatGoal GetTopFriendGoal(LeaderboardEntry[] entries)
        {
            return GetGoalFromLeaderboard(entries.OrderBy(x=>x.Score).FirstOrDefault());
        }

        private StatGoal GetGoalFromLeaderboard(LeaderboardEntry entry)
        {
            int score = entry.Score;
            float seconds = (float)score / 1000f;

            return new StatGoal()
            {
                Seconds = seconds,
                Kills = 0,
                Deaths = 10000,
                Style = 0
            };
        }

        private StatGoal GetCustomGoal()
        {
            return new StatGoal()
            {
                Kills = CFG_CustomGoalKills.Value,
                Seconds = CFG_CustomGoalSeconds.Value,
                Deaths = CFG_CustomGoalDeaths.Value,
                Style = CFG_CustomGoalStyle.Value
            };
        }

        private StatGoal GetLevelPRankGoal()
        {
            return new StatGoal()
            {
                Kills = StatsManager.Instance.killRanks[3],
                Seconds = StatsManager.Instance.timeRanks[3],
                Deaths = 0,
                Style = StatsManager.Instance.styleRanks[3]
            };
        }

        private static TrackerManager instance;

        private void OnDestroy()
        {
            CFG_trackerType.OnValueChanged -= InstanceTracker;
            CFG_UsePRankForLeaderboardGoals.OnValueChanged -= (v) => InitializeGoal();
            CFG_GoalMode.OnValueChanged -= (v) => InitializeGoal();

            CFG_CustomGoalDeaths.OnValueChanged -= (v) => InitializeGoal();
            CFG_CustomGoalKills.OnValueChanged -= (v) => InitializeGoal();
            CFG_CustomGoalStyle.OnValueChanged -= (v) => InitializeGoal();
            CFG_CustomGoalSeconds.OnValueChanged -= (v) => InitializeGoal();
        }
    }

    public enum GoalMode
    {
        LevelPRank,
        Custom,
        PersonalBestTime,
        HighestFriendTime,
        NextHighestFriendTime,
    }

    public enum TrackerType
    {
        Modern,
        Classic,
        Compact,
    }
}
