using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace EasyPZ
{
    public class EasyPZUIPatch : MonoBehaviour
    {
        public bool PMode = false;

        public enum RestartType { Explosion, Instant }
        public enum HUDInfoType { Simple, Default }

        public KeyCode PMODE_TOGGLE = KeyCode.P;
        public KeyCode RESTART_MISSION = KeyCode.RightAlt;
        public RestartType RESTART_TYPE = RestartType.Instant;
        public HUDInfoType HUD_INFO_TYPE = HUDInfoType.Default;
        public bool ALWAYS_SHOW_TRACKER = false;
        public bool PMODE_DEFAULT_STATE = false;

        public GameObject rankTrackerUIElement;

        private OptionsManager om;
        private NewMovement player;
        private StatsManager sman;
        private LevelStats lStats;
        private AssistController assCon;

        private void Start()
        {
            FindThings();
            InstantiateTracker();
        }

        private void Update()
        {
            if (Input.GetKeyDown(PMODE_TOGGLE))
            {
                PMode = !PMode;
            }

            if (Input.GetKeyDown(RESTART_MISSION) && InLevel())
            {
                FailPRank();
            }

            if (rankTrackerUIElement != null)
            {
                UpdateTracker();
            }
            else
            {
                InstantiateTracker();
            }
            
        }

        private void LateUpdate()
        {
            CheckPRankStatus();   
        }

        //returns true if in level, false if out.
        private bool InLevel()
        {
            string sceneName = SceneManager.GetActiveScene().name;
            if (sceneName == "Intro" || sceneName == "Main Menu" || sceneName == "Cybergrind" || sceneName == "Sandbox")
            {
                return false;
            }
            return true;
        }

        private void CheckPRankStatus()
        {
            if(!InLevel()) { return; }
            if (assCon.cheatsEnabled || assCon.majorEnabled) { PMode = false; } 
            if (!PMode) { return; }

            bool fail = false;

            if (player.dead || sman.seconds > sman.timeRanks[3]) { fail=true; } //Checks if player died.
            else if (sman.infoSent && (sman.kills > sman.killRanks[3] || sman.stylePoints > sman.styleRanks[3])) { fail = true; } //checks if you failed p req at the end of the level.

            if(fail) { FailPRank(); }
        }

        //Restarts the mission.
        private void FailPRank()
        {
            switch (RESTART_TYPE)
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

        private void InstantiateTracker()
        {
            RectTransform canvas = gameObject.GetComponent<RectTransform>();
            GameObject trackerPrefab;
            switch (HUD_INFO_TYPE)
            {
                default:
                    HydraLoader.prefabRegistry.TryGetValue("RankGoalIndicator", out trackerPrefab);
                    trackerPrefab.SetActive(false);
                    break;
                case HUDInfoType.Simple:
                    HydraLoader.prefabRegistry.TryGetValue("PStatusIndicator", out trackerPrefab);
                    trackerPrefab.SetActive(false);
                    break;
            }
            rankTrackerUIElement = GameObject.Instantiate<GameObject>(trackerPrefab, canvas);
        }

        //Finds instances for operation
        private void FindThings()
        {
            om = MonoSingleton<OptionsManager>.Instance;
            sman = MonoSingleton<StatsManager>.Instance;
            player = MonoSingleton<NewMovement>.Instance;
            assCon = MonoSingleton<AssistController>.Instance;
            lStats = GameObject.FindObjectOfType<LevelStatsEnabler>().transform.GetChild(0).GetComponent<LevelStats>();
        }

        //Updates the visibility of the tracker.
        private void UpdateTracker()
        {
            bool trackerVisible = false;
            if (InLevel())
            {
                switch (HUD_INFO_TYPE)
                {
                    case HUDInfoType.Default:
                        if (lStats.gameObject.activeInHierarchy || ALWAYS_SHOW_TRACKER)
                        {
                            trackerVisible = true;
                        }
                        break;
                    case HUDInfoType.Simple:
                        if (om.paused)
                        {
                            trackerVisible = true;
                            if (PMode) //Sets color of indicator
                            {
                                rankTrackerUIElement.transform.GetChild(0).GetComponent<Text>().color = new Color(255, 119, 0, 255);
                            }
                            else
                            {
                                rankTrackerUIElement.transform.GetChild(0).GetComponent<Text>().color = new Color(191, 191, 191, 150);
                            }
                        }
                        break;
                }
                
            }else
            {
                trackerVisible = false;
            }
            rankTrackerUIElement.SetActive(trackerVisible);
        }

    }
}
