using Configgy;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace EasyPZ.Components
{
    public class TrackerManager : MonoBehaviour
    {
        [SerializeField] private Image blocker;
        [SerializeField] private GameObject container;

        private RectTransform content;

        private GameObject tracker;
        private LevelStats levelStats;

        private bool editMode;
        public static bool AutoRestartEnabled;

        private StatGoal currentStatGoal;


        [Configgable("Config", "Tracker Type")]
        private static ConfigDropdown<TrackerType> CFG_trackerType = new ConfigDropdown<TrackerType>((TrackerType[])Enum.GetValues(typeof(TrackerType)), ((TrackerType[])Enum.GetValues(typeof(TrackerType))).Select(x => x.ToString()).ToArray(), 1);

        [Configgable("Config", "Always Show Tracker")]
        private static ConfigToggle CFG_AlwaysShowTracker = new ConfigToggle(true);

        [Configgable("Config", "Enable Auto Restart By Default")]
        private static ConfigToggle CFG_AutoResetDefault = new ConfigToggle(true);


        [Configgable("Options", "Use Personal Best As Goal")]
        private static ConfigToggle CFG_PersonalBestMode = new ConfigToggle(false);


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
            CFG_PersonalBestMode.OnValueChanged += InitializeGoal;

            if (InGameCheck.InLevel())
            {
                InitializeGoal(CFG_PersonalBestMode.Value);
                InstanceTracker(CFG_trackerType.Value);
            }

            if (CFG_AutoResetDefault.Value)
                AutoRestartEnabled = true;
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
                case TrackerType.Modern:
                case TrackerType.Classic:
                    prefab = Prefabs.ClassicTrackerPrefab;
                    break;
            }

            tracker = Instantiate(prefab, content);
            if (tracker.TryGetComponent(out IEZPZTracker iTracker))
                iTracker.SetStatGoal(currentStatGoal);
        }

        private void InitializeGoal(bool pbmode)
        {
            if (pbmode)
                currentStatGoal = RecordsManager.GetStatGoal(SceneHelper.CurrentScene);
            else
                currentStatGoal = GetLevelPRankGoal();

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
                InitializeGoal(CFG_PersonalBestMode.Value);
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
            CFG_PersonalBestMode.OnValueChanged -= InitializeGoal;

        }
    }

    public enum TrackerType
    {
        Modern,
        Classic,
        Compact,
    }
}
