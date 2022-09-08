using BepInEx;
using BepInEx.Configuration;
using UnityEngine;

namespace EasyPZ
{

    [BepInPlugin("Hydraxous.ULTRAKILL.EasyPZ", "EasyPZ", "1.0.0")]
    public class EasyPZ : BaseUnityPlugin
    {
        private OptionsManager om;
        private NewMovement player;
        private StatsManager sman;

        private ConfigEntry<KeyCode> PMODE_TOGGLE;
        private ConfigEntry<KeyCode> RESTART_MISSION;

        bool enablePMode = true;
        bool pActive = false;

        private void Awake()
        {
            Logger.LogInfo("EasyPZ Loaded. Good luck on P-ing in all the levels! :D");

            PMODE_TOGGLE = Config.Bind("General", "PMODE_TOGGLE", KeyCode.P, "Key to toggle P-Mode");
            RESTART_MISSION = Config.Bind("General", "RESTART_MISSION", KeyCode.RightAlt, "Instantly restarts the mission");
            FindThings();
            
        }
        
        private void Update()
        {
            if(Input.GetKeyDown(PMODE_TOGGLE.Value))
            {
                enablePMode = !enablePMode;
            }
            if(Input.GetKeyDown(RESTART_MISSION.Value))
            {
                QuickRestart();
            }

            if(player.dead && enablePMode)
            {
                QuickRestart();
            }
        }

        private void LateUpdate()
        {
            if (enablePMode)
            {
                FindThings();
                CheckTimeGoalFailed();
            }
        }

        private void OnGUI()
        {
            
            if (player != null)
            {
                pActive = player.activated;
            }
            if (MonoSingleton<OptionsManager>.Instance.paused && pActive)
            {
                GUI.Label(new Rect(new Vector2(Screen.width/2.1f,Screen.height/3.14f), new Vector2(Screen.width/10, Screen.height/10)), "P-Mode: " + enablePMode.ToString());
            }
        }

        //Restarts level entirely
        private void QuickRestart()
        {
            if (om != null)
            {
                om.RestartMission();
            }
        }

        //Restarts from last checkpoint
        private void QuickCheckpoint()
        {
            if (om != null)
            {
                om.RestartCheckpoint();
            }
        }

        //Kills the player via explosion... Quite funny.
        private void KillPlayer()
        {
            if (player != null)
            {
                player.GetHurt(200, false, 1, true, false);
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
        }
        
        //Checks if time goal has been failed: A rank.
        private void CheckTimeGoalFailed()
        {
            if (sman != null && enablePMode)
            {
                if (sman.GetRanks(sman.timeRanks, sman.seconds, true, false) == "<color=#FF6A00>A</color>")
                {
                    KillPlayer();
                }
            }
        }
    }
}