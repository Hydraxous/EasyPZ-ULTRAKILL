using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using EasyPZ.Properties;


namespace EasyPZ
{
    [BepInPlugin("Hydraxous.ULTRAKILL.EasyPZ", "EasyPZ", "1.0.1")]
    public class EasyPZ : BaseUnityPlugin
    {
        private enum RestartType {Explosion, Instant}
        private enum HUDInfoType {Simple, Default}

        private OptionsManager om;
        private NewMovement player;
        private StatsManager sman;
        private LevelStats lStats;

        private ConfigEntry<KeyCode> PMODE_TOGGLE;
        private ConfigEntry<KeyCode> RESTART_MISSION;
        private ConfigEntry<RestartType> RESTART_TYPE;
        private ConfigEntry<HUDInfoType> HUD_INFO_TYPE;
        private ConfigEntry<bool> PMODE_DEFAULT_STATE;
        private ConfigEntry<bool> ALWAYS_SHOW_TRACKER;

        private GameObject PStatusIndicatorPrefab;
        private static UIPositionData PSI_UIPData = new UIPositionData(Vector2.zero, new Vector2(100,100),Vector2.one,Vector2.one,Vector2.one);
        private GameObject RankGoalIndicatorPrefab;
        private static UIPositionData RGI_UIPData = new UIPositionData(new Vector2(-10, 10), new Vector2(150f, 202.5f), new Vector2(1,0), new Vector2(1, 0), new Vector2(1, 0));


        public GameObject rankTrackerUIElement;

        public static AssetBundle easyPZAssets;
        public bool PMode = false;


        private void Awake()
        {
            Logger.LogInfo("EasyPZ Loaded. Good luck on P-ing in all the levels! :D");

            PMODE_TOGGLE = Config.Bind("Binds", "PMODE_TOGGLE", KeyCode.P, "Key to toggle P-Mode");
            RESTART_MISSION = Config.Bind("Binds", "RESTART_MISSION", KeyCode.RightAlt, "Restarts the mission");
            RESTART_TYPE = Config.Bind("General", "RESTART_TYPE", RestartType.Instant, "Explosion causes you to explode when you fail P-Rank, instant just restarts the mission without having to click respawn. NOTE: Explosion is not yet implemented and will just make you instantly die when pressing the key.");
            HUD_INFO_TYPE = Config.Bind("General", "HUD_INFO_TYPE", HUDInfoType.Default, "Changes hud type. Simple is a tiny P icon in corner when game is paused. Default is the goal UI on the left hand side when stats are turned on.");
            PMODE_DEFAULT_STATE = Config.Bind("General", "PMODE_DEFAULT_STATE", false, "Sets whether P-Mode should be enabled by default.");
            ALWAYS_SHOW_TRACKER = Config.Bind("General", "ALWAYS_SHOW_TRACKER", false, "Shows tracker when stats are not open in level.");
            LoadAssets();
        }
        

        private void Update()
        {
            if(Input.GetKeyDown(PMODE_TOGGLE.Value))
            {
                PMode = !PMode;
            }

            if(Input.GetKeyDown(RESTART_MISSION.Value))
            {
                FailPRank(RESTART_TYPE.Value);
            }

        }


        private void LateUpdate()
        {
            if (PMode || InLevel())
            {
                FindThings();
                if (player.dead) { FailPRank(RestartType.Instant); } //Checks if player died.
                CheckTimeGoalFailed();
            }

            if (rankTrackerUIElement == null && player != null)
            {
                CreateTracker();
            }else
            {
                UpdateTracker();
            }
        }

        //Finds instances for operation
        private void FindThings()
        {
            if (om == null)
            {
                om = MonoSingleton<OptionsManager>.Instance;
            }

            if (sman == null)
            {
                sman = MonoSingleton<StatsManager>.Instance;
            }

            if (player == null)
            {
                player = MonoSingleton<NewMovement>.Instance;
            }

            if (lStats == null)
            {
                lStats = GameObject.FindObjectOfType<LevelStatsEnabler>().transform.GetChild(0).GetComponent<LevelStats>();
            }

            if (rankTrackerUIElement == null)
            {
                CreateTracker();
            }
        }

        //Checks if in level in which the display should not be shown
        private bool InLevel()
        {
            string sceneName = SceneManager.GetActiveScene().name;
            if (sceneName == "Intro" || sceneName == "Main Menu" || sceneName == "Cybergrind" || sceneName == "Sandbox")
            {
                return false;
            }
            return true;
        }
        
        //Checks if time goal has been failed: A rank.
        private void CheckTimeGoalFailed()
        {
            if (sman != null)
            {
                if (sman.GetRanks(sman.timeRanks, sman.seconds, true, false) == "<color=#FF6A00>A</color>")
                {
                    FailPRank(RESTART_TYPE.Value);
                }
            }
        }

        //Restarts the mission.
        private void FailPRank(RestartType manType)
        {
            if (om != null && InLevel() && player != null)
            {
                switch (manType)
                {
                    case RestartType.Explosion:
                        //ExplodePlayer();
                        player.GetHurt(200, false, 1, false, true);
                        break;
                    default:
                        om.RestartMission();
                        break;
                }
                
            }
            
        }
        
        //Tries to instantiate the tracker object.
        public void CreateTracker()
        {
            if (!InLevel()) { return; }

            switch (HUD_INFO_TYPE.Value)
            {
                case HUDInfoType.Default:
                    InstantiateTracker(RankGoalIndicatorPrefab, RGI_UIPData);
                    break;
                case HUDInfoType.Simple:
                    InstantiateTracker(PStatusIndicatorPrefab, PSI_UIPData);
                    break;
            }
        }


        private void InstantiateTracker(GameObject prefab, UIPositionData uiData)
        {
            RectTransform canvas = MonoSingleton<CanvasController>.Instance.gameObject.GetComponent<RectTransform>();
            rankTrackerUIElement = GameObject.Instantiate<GameObject>(prefab, canvas);
            UIAutoPositioner positioner = rankTrackerUIElement.AddComponent<UIAutoPositioner>();
            if (rankTrackerUIElement.name.Contains("RankGoal"))
            {
                rankTrackerUIElement.AddComponent<RankGoalTracker>();
            }

            positioner.uIPositionData = uiData;
            positioner.CheckPosition();
            UpdateTracker();
        }
        
        //Updates the data in the tracker.
        private void UpdateTracker()
        {
            switch(HUD_INFO_TYPE.Value)
            {
                case HUDInfoType.Default:
                    if ((lStats.gameObject.activeInHierarchy || ALWAYS_SHOW_TRACKER.Value) && InLevel())
                    {
                        rankTrackerUIElement.SetActive(true);   
                    }
                    else
                    {
                        rankTrackerUIElement.SetActive(false);
                    }
                    break;
                case HUDInfoType.Simple:
                    if (om.paused)
                    {
                        rankTrackerUIElement.SetActive(true);
                        if(PMode) //Sets color of indicator
                        {
                            rankTrackerUIElement.transform.GetChild(0).GetComponent<Text>().color = new Color(255,119,0,255);
                        }else
                        {
                            rankTrackerUIElement.transform.GetChild(0).GetComponent<Text>().color = new Color(191, 191, 191, 150);
                        }
                    }
                    else
                    {
                        rankTrackerUIElement.SetActive(false);
                    }
                    break;
            }
            
        }

        private void GetReverseCounter()
        {

        }

        private void LoadAssets()
        {
            try
            {
                easyPZAssets = AssetBundle.LoadFromMemory(EasyPZResources.EasyPZ);
                PStatusIndicatorPrefab = easyPZAssets.LoadAsset<GameObject>("PStatusIndicatorMini");
                RankGoalIndicatorPrefab = easyPZAssets.LoadAsset<GameObject>("RankGoalIndicator");
            }
            catch (System.Exception e)
            {
                Logger.LogError(e.Message);
                Logger.LogError("Could not load assets.");
            }

        }
    }
}