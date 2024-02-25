using Configgy;
using Configgy.UI;
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
        [Configgable("Binds", "Auto Restart Toggle")]
        private static ConfigKeybind Key_PModeToggle = new ConfigKeybind(KeyCode.P);

        [Configgable("Binds", "Restart Mission")]
        private static ConfigKeybind Key_RestartMission = new ConfigKeybind(KeyCode.RightAlt);

        [Configgable("Tracker", "Tracker Type")]
        private static ConfigDropdown<TrackerType> CFG_trackerType = new ConfigDropdown<TrackerType>((TrackerType[])Enum.GetValues(typeof(TrackerType)), ((TrackerType[])Enum.GetValues(typeof(TrackerType))).Select(x => x.ToString()).ToArray(), 1);

        [Configgable("Tracker", "Always Show Tracker", description: "When disabled the tracker will be tied to the tab stats overlay and will only show when it is on screen.")]
        private static ConfigToggle CFG_AlwaysShowTracker = new ConfigToggle(true);

        [Configgable("Auto Restart", "Only AutoRestart At Level End", description: "This will only auto-restart if the goal is failed when you finish the level.")]
        private static ConfigToggle CFG_AutoRestartAtLevelEnd = new ConfigToggle(false);

        [Configgable("Auto Restart", "Enable Auto Restart")]
        public static ConfigToggle AutoRestartEnabled = new ConfigToggle(false);

        [Configgable("Tracker/Goal", "Use P-Rank stats for Goal", description: "This will use the P-Rank time instead of Any% time for leaderboard based goals.")]
        private static ConfigToggle CFG_UsePRankForLeaderboardGoals = new ConfigToggle(false);


        [Configgable("Tracker/Goal", "Goal Mode")]
        private static ConfigDropdown<GoalMode> CFG_GoalMode = new ConfigDropdown<GoalMode>((GoalMode[])Enum.GetValues(typeof(GoalMode)), new string[]
        {
            "P-Rank",
            "Custom",
            "Personal Best",
            "Top Friend",
            "Next Highest Friend",

        }, 0);
        

        #region CFG_CustomGoal

        [Configgable("Tracker/Goal/Custom Goal", "Kills")]
        private static ConfigInputField<int> CFG_CustomGoalKills = new ConfigInputField<int>(0);

        [Configgable("Tracker/Goal/Custom Goal", "Seconds")]
        private static ConfigInputField<float> CFG_CustomGoalSeconds = new ConfigInputField<float>(120f);

        [Configgable("Tracker/Goal/Custom Goal", "Deaths")]
        private static ConfigInputField<int> CFG_CustomGoalDeaths = new ConfigInputField<int>(0);

        [Configgable("Tracker/Goal/Custom Goal", "Style")]
        private static ConfigInputField<int> CFG_CustomGoalStyle = new ConfigInputField<int>(2000);
        #endregion

        [SerializeField] private UnityEngine.UI.Image blocker;
        [SerializeField] private GameObject container;

        private RectTransform content;

        private GameObject tracker;
        private LevelStats levelStats;

        private bool editMode;

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
            CFG_UsePRankForLeaderboardGoals.OnValueChanged += ReinitGoal<bool>;
            CFG_GoalMode.OnValueChanged += ReinitGoal<GoalMode>;

            CFG_CustomGoalDeaths.OnValueChanged += ReinitGoal<int>;
            CFG_CustomGoalKills.OnValueChanged += ReinitGoal<int>;
            CFG_CustomGoalStyle.OnValueChanged += ReinitGoal<int>;
            CFG_CustomGoalSeconds.OnValueChanged += ReinitGoal<float>;

            if (InGameCheck.InLevel())
            {
                InitializeGoal();
                InstanceTracker(CFG_trackerType.Value);
                gameObject.AddComponent<SessionRecorder>();
                gameObject.AddComponent<GhostManager>();
                gameObject.AddComponent<SpeedLimiter>();
            }
        }


        private void ReinitGoal<T>(T v)
        {
            InitializeGoal();
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
                default:
                case TrackerType.Compact:
                    prefab = Prefabs.CompactTracker;
                    break;
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

        public static void SetCustomGoal(StatGoal goal)
        {
            CFG_GoalMode.SetIndex((int)GoalMode.Custom);
            CFG_CustomGoalKills.SetValue(goal.Kills);
            CFG_CustomGoalDeaths.SetValue(goal.Deaths);
            CFG_CustomGoalStyle.SetValue(goal.Style);
            CFG_CustomGoalSeconds.SetValue(goal.Seconds);
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

            if (Key_PModeToggle.WasPeformed())
            {
                AutoRestartEnabled.SetValue(!AutoRestartEnabled.Value);
            }

            if (Key_RestartMission.WasPeformed())
            {
                Restarter.Restart();
            }

            if (AutoRestartEnabled.Value)
            {
                if (currentStatGoal.NotEmpty())
                    if (StatsManager.Instance.infoSent || !CFG_AutoRestartAtLevelEnd.Value)
                        if (currentStatGoal.IsFailed())
                            Restarter.Restart();
            }
        }


        [Configgable("Tracker", "Reposition Tracker")]
        private static void EnterEditMode()
        {
            if (instance == null)
                return;

            instance.EnterEditModeInternal();
        }


        private void EnterEditModeInternal()
        {
            ConfigurationMenu.Close();

            GameState configState = new GameState("edit_ezpz_tracker", blocker.gameObject);

            configState.cursorLock = LockMode.Unlock;
            configState.playerInputLock = LockMode.Lock;
            configState.cameraInputLock = LockMode.Lock;
            configState.priority = 20;

            OptionsManager.Instance.paused = true;
            Time.timeScale = 0f;
            GameStateManager.Instance.RegisterState(configState);

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
            GameStateManager.Instance.PopState("edit_ezpz_tracker");
            OptionsManager.Instance.paused = false;
            Time.timeScale = 1f;

            if (tracker.TryGetComponent(out IUIEditable editable))
            {
                editable.EndEditMode();
            }

            blocker.gameObject.SetActive(false);
            editMode = false;

            ConfigurationMenu.Open();
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
            try
            {
                StartCoroutine(RetrieveLevelScores((r, l) =>
                {
                    if(r)
                        OnLevelScoresReceived(l);
                    else
                        Debug.LogError("Failed to retrieve leaderboard scores.");
                }));

            }catch(System.Exception ex)
            {
                Debug.LogError("Error occured while trying to fetch scores.");
                Debug.LogException(ex);
            }
        }

        private bool CanUseOnlineRecords()
        {
            return InGameCheck.CurrentLevelType == InGameCheck.UKLevelType.Level || InGameCheck.CurrentLevelType == InGameCheck.UKLevelType.PrimeSanctum && SteamClient.IsValid;
        }


        private static IEnumerator RetrieveLevelScores(Action<bool, LeaderboardEntry[]> onComplete)
        {
            string levelName = SceneHelper.CurrentScene;
            if(LeaderboardController.Instance == null)
            {
                onComplete.Invoke(false, null);
                yield break;
            }

            Task<LeaderboardEntry[]> task = LeaderboardController.Instance.GetLevelScores(levelName, CFG_UsePRankForLeaderboardGoals.Value);
            
            if(task == null)
            {
                onComplete.Invoke(false, null);
                yield break;
            }

            while (!task.IsCompleted)
                yield return null;

            if (task.Result == null)
            {
                Debug.LogError("Failed to retrieve leaderboard scores.");
                onComplete.Invoke(false, null);
                yield break;
            }

            onComplete.Invoke(true, task.Result);
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
            //Score is milliseconds
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
            CFG_UsePRankForLeaderboardGoals.OnValueChanged -= ReinitGoal<bool>;
            CFG_GoalMode.OnValueChanged -= ReinitGoal<GoalMode>;

            CFG_CustomGoalDeaths.OnValueChanged -= ReinitGoal<int>;
            CFG_CustomGoalKills.OnValueChanged -= ReinitGoal<int>;
            CFG_CustomGoalStyle.OnValueChanged -= ReinitGoal<int>;
            CFG_CustomGoalSeconds.OnValueChanged -= ReinitGoal<float>;

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
        Classic,
        Compact,
    }
}
