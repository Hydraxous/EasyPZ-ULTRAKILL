using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EasyPZ
{

    [BepInPlugin("Hydraxous.ULTRAKILL.EasyPZ", "EasyPZ", "1.0.1")]
    public class EasyPZ : BaseUnityPlugin
    {
        private enum RestartType {Explosion, Instant}

        private OptionsManager om;
        private NewMovement player;
        private StatsManager sman;

        private ConfigEntry<KeyCode> PMODE_TOGGLE;
        private ConfigEntry<KeyCode> RESTART_MISSION;
        private ConfigEntry<RestartType> RESTART_TYPE;

        private GameObject Boom; //Unused

        public bool PMode = true;

        private void Awake()
        {
            Logger.LogInfo("EasyPZ Loaded. Good luck on P-ing in all the levels! :D");

            PMODE_TOGGLE = Config.Bind("Binds", "PMODE_TOGGLE", KeyCode.P, "Key to toggle P-Mode");
            RESTART_MISSION = Config.Bind("Binds", "RESTART_MISSION", KeyCode.RightAlt, "Restarts the mission");
            RESTART_TYPE = Config.Bind("General", "RESTART_TYPE", RestartType.Instant, "Explosion causes you to explode when you fail P-Rank, instant just restarts the mission without having to click respawn. NOTE: Explosion is not yet implemented and will just make you instantly die when pressing the key.");
          
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
            if (PMode)
            {
                FindThings();
                if (player.dead) { FailPRank(RestartType.Instant); } //Checks if player died.
                CheckTimeGoalFailed();
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

        private void OnGUI()
        {
            
            if (player != null)
            {
                if (MonoSingleton<OptionsManager>.Instance.paused && player.activated)
                {
                    GUI.Label(new Rect(new Vector2(Screen.width / 2.1f, Screen.height / 3.14f), new Vector2(Screen.width / 10, Screen.height / 10)), "P-Mode: " + PMode.ToString());
                }
            }
        }

        //Kills the player via explosion, sorta... Quite funny. Unused for the moment.
        private void ExplodePlayer()
        {
            if (player != null && Boom != null)
            {

                GameObject newBoom = Instantiate<GameObject>(Boom, player.transform.position, Quaternion.identity);
                foreach(Explosion explosion in newBoom.GetComponentsInChildren<Explosion>())
                {
                    explosion.sourceWeapon = MonoSingleton<GunControl>.Instance.currentWeapon;
                    explosion.enemyDamageMultiplier = 99999f;
                    explosion.maxSize *= 99f;
                    explosion.damage = 99999;
                }
            }   
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


        private void FailPRank(RestartType manType)
        {
            if (om != null && SceneManager.GetActiveScene().name != "Main Menu" && player != null)
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
    }
}