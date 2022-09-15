using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using EasyPZ.Properties;


namespace EasyPZ
{
    [BepInPlugin("Hydraxous.ULTRAKILL.EasyPZ", "EasyPZ", "1.9.0")]
    public class EasyPZ : BaseUnityPlugin
    {
        private ConfigEntry<KeyCode> PMODE_TOGGLE;
        private ConfigEntry<KeyCode> RESTART_MISSION;
        private ConfigEntry<EasyPZUIPatch.RestartType> RESTART_TYPE;
        private ConfigEntry<EasyPZUIPatch.HUDInfoType> HUD_INFO_TYPE;
        private ConfigEntry<bool> PMODE_DEFAULT_STATE;
        private ConfigEntry<bool> ALWAYS_SHOW_TRACKER;

        public EasyPZUIPatch ezPzPatch;

        private void Awake()
        {
            if (!RegisterAssets())
            {
                Logger.LogInfo("EasyPZ Failed to load assets. Mod is disabled.");
                this.enabled = false;
            }else
            {
                Logger.LogInfo("EasyPZ Loaded. Good luck on P-ing in all the levels! :D");
            }
        }

        private void Update()
        {
            try
            {
                CheckPatch();
            }catch(System.Exception e)
            {

            }   
        }

        private bool InLevel()
        {
            string sceneName = SceneManager.GetActiveScene().name;
            if (sceneName == "Intro" || sceneName == "Main Menu" || sceneName == "Cybergrind" || sceneName == "Sandbox")
            {
                return false;
            }
            return true;
        }


        //Checks if patch has been applied. if not, applies it.
        private void CheckPatch()
        {
            if (ezPzPatch != null || !InLevel()) {return;}

            CanvasController canvas = MonoSingleton<CanvasController>.Instance;

            if(!canvas.gameObject.TryGetComponent<EasyPZUIPatch>(out EasyPZUIPatch _ezPz))
            {
                ezPzPatch = canvas.gameObject.AddComponent<EasyPZUIPatch>();
                ezPzPatch.PMODE_TOGGLE = PMODE_TOGGLE.Value;
                ezPzPatch.RESTART_MISSION = RESTART_MISSION.Value;
                ezPzPatch.RESTART_TYPE = RESTART_TYPE.Value;
                ezPzPatch.HUD_INFO_TYPE = HUD_INFO_TYPE.Value;
                ezPzPatch.ALWAYS_SHOW_TRACKER = ALWAYS_SHOW_TRACKER.Value;
                ezPzPatch.PMODE_DEFAULT_STATE = PMODE_DEFAULT_STATE.Value;
            }

        }

        //Calls on HydraLoader to register and prepare assets for use.
        private bool RegisterAssets()
        {
            new HydraLoader.CustomAsset("PStatusIndicatorMini", new Component[] { new UIAutoPositioner() });
            HydraLoader.uIDataRegistry.Add("PStatusIndicatorMini", new UIPositionData(Vector2.zero, new Vector2(100, 100), Vector2.one, Vector2.one, Vector2.one));

            new HydraLoader.CustomAsset("RankGoalIndicator", new Component[] { new UIAutoPositioner(), new RankGoalIndicator() });
            HydraLoader.uIDataRegistry.Add("RankGoalIndicator", new UIPositionData(new Vector2(-10, 10), new Vector2(165f, 202.5f), new Vector2(1, 0), new Vector2(1, 0), new Vector2(1, 0)));

            return HydraLoader.RegisterAll();
        }

        //binds BepInEx config
        private void BindConfig()
        {
            PMODE_TOGGLE = Config.Bind("Binds", "PMODE_TOGGLE", KeyCode.P, "Key to toggle P-Mode");
            RESTART_MISSION = Config.Bind("Binds", "RESTART_MISSION", KeyCode.RightAlt, "Restarts the mission");
            RESTART_TYPE = Config.Bind("General", "RESTART_TYPE", EasyPZUIPatch.RestartType.Instant, "Explosion causes you to explode when you fail P-Rank, instant just restarts the mission without having to click respawn. NOTE: Explosion is not yet implemented and will just make you instantly die when pressing the key.");
            HUD_INFO_TYPE = Config.Bind("General", "HUD_INFO_TYPE", EasyPZUIPatch.HUDInfoType.Default, "Changes hud type. Simple is a tiny P icon in corner when game is paused. Default is the goal UI on the left hand side when stats are turned on.");
            PMODE_DEFAULT_STATE = Config.Bind("General", "PMODE_DEFAULT_STATE", false, "Sets whether P-Mode should be enabled by default.");
            ALWAYS_SHOW_TRACKER = Config.Bind("General", "ALWAYS_SHOW_TRACKER", false, "Shows tracker when stats are not open in level.");
        }
    }
}