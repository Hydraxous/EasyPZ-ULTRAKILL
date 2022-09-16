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
        public bool lastPMode;

        private ConfigEntry<KeyCode> PMODE_TOGGLE;
        private ConfigEntry<KeyCode> RESTART_MISSION;
        private ConfigEntry<EasyPZUIPatch.RestartType> RESTART_TYPE;
        private ConfigEntry<EasyPZUIPatch.HUDInfoType> HUD_INFO_TYPE;
        private ConfigEntry<bool> PMODE_DEFAULT_STATE;
        private ConfigEntry<bool> ALWAYS_SHOW_TRACKER;


        //Ui optionn
        private ConfigEntry<UIPositionData.AnchorSpot> TRACKER_ANCHOR_POSITION;
        private ConfigEntry<float> TRACKER_BORDER_SIZE;
        private ConfigEntry<Color> TRACKER_BACKGROUND_COLOR;
        private ConfigEntry<Color> TRACKER_FONT_COLOR;
        private ConfigEntry<Color> TRACKER_HIGHLIGHT_COLOR;


        public EasyPZUIPatch ezPzPatch;

        private void Awake()
        {
            BindConfig();
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
            if (ezPzPatch != null || !InLevel())
            {
                return;
            }

            CanvasController canvas = MonoSingleton<CanvasController>.Instance;

            if(!canvas.gameObject.TryGetComponent<EasyPZUIPatch>(out EasyPZUIPatch _ezPz))
            {
                ezPzPatch = canvas.gameObject.AddComponent<EasyPZUIPatch>();
                ezPzPatch.PMODE_TOGGLE = PMODE_TOGGLE.Value;
                ezPzPatch.RESTART_MISSION = RESTART_MISSION.Value;
                ezPzPatch.RESTART_TYPE = RESTART_TYPE.Value;
                ezPzPatch.HUD_INFO_TYPE = HUD_INFO_TYPE.Value;
                ezPzPatch.ALWAYS_SHOW_TRACKER = ALWAYS_SHOW_TRACKER.Value;
                ezPzPatch.PMode = lastPMode;
            }

        }

        //Calls on HydraLoader to register and prepare assets for use.
        private bool RegisterAssets()
        {
            new HydraLoader.CustomAsset("PStatusIndicatorMini", new Component[] { new UIAutoPositioner() });
            new HydraLoader.CustomAssetData("PStatusIndicatorMini_UIPD", new UIPositionData(new Vector2(85f, 85f),TRACKER_ANCHOR_POSITION.Value, TRACKER_BORDER_SIZE.Value, TRACKER_BACKGROUND_COLOR.Value, TRACKER_FONT_COLOR.Value, TRACKER_HIGHLIGHT_COLOR.Value));

            new HydraLoader.CustomAsset("RankGoalIndicator", new Component[] { new UIAutoPositioner(), new RankGoalIndicator() });
            new HydraLoader.CustomAssetData("RankGoalIndicator_UIPD", new UIPositionData(new Vector2(170f, 202.5f), TRACKER_ANCHOR_POSITION.Value, TRACKER_BORDER_SIZE.Value, TRACKER_BACKGROUND_COLOR.Value, TRACKER_FONT_COLOR.Value, TRACKER_HIGHLIGHT_COLOR.Value));

            return HydraLoader.RegisterAll();
        }

        //binds BepInEx config
        private void BindConfig()
        {
            PMODE_TOGGLE = Config.Bind("Binds", "PMODE_TOGGLE", KeyCode.P, "Key to toggle P-Mode");
            RESTART_MISSION = Config.Bind("Binds", "RESTART_MISSION", KeyCode.RightAlt, "Restarts the mission");
            RESTART_TYPE = Config.Bind("General", "RESTART_TYPE", EasyPZUIPatch.RestartType.Instant, "Explosion causes you to explode when you fail P-Rank, instant just restarts the mission without having to click respawn. NOTE: Explosion is not yet implemented and will just make you instantly die when pressing the key.");
            HUD_INFO_TYPE = Config.Bind("General", "HUD_INFO_TYPE", EasyPZUIPatch.HUDInfoType.Default, "Changes hud type. Simple is a tiny P icon in corner when game is paused. Default is the goal UI on the left hand side when stats are turned on.");
            PMODE_DEFAULT_STATE = Config.Bind("General", "PMODE_DEFAULT_STATE", true, "Sets whether P-Mode should be enabled by default.");
            ALWAYS_SHOW_TRACKER = Config.Bind("General", "ALWAYS_SHOW_TRACKER", true, "Shows tracker when stats are not open in level.");
            lastPMode = PMODE_DEFAULT_STATE.Value;

            //UI OPTIONS
            TRACKER_ANCHOR_POSITION = Config.Bind("Tracker Settings", "TRACKER_ANCHOR_POSITION", UIPositionData.AnchorSpot.bottomRight, "Changes anchored position of the tracker.");
            TRACKER_BORDER_SIZE = Config.Bind("Tracker Settings", "TRACKER_BORDER_SIZE", 10.0f, "Changes how far from the edge of the screen the tracker is.");
            TRACKER_BACKGROUND_COLOR = Config.Bind("Tracker Settings", "TRACKER_BACKGROUND_COLOR", new Color(0,0,0,69.0f), "Changes background color of the tracker, the color format is RGBA to Hex here is a helpful tool https://rgbacolorpicker.com/rgba-to-hex the last hex value is transparency. 00 is transparent, FF is opaque if you want a code similar to most UK Ui use this: 00000059");
            TRACKER_FONT_COLOR = Config.Bind("Tracker Settings", "TRACKER_FONT_COLOR", new Color(255,255,255,210.0f), "Changes font color");
            TRACKER_HIGHLIGHT_COLOR = Config.Bind("Tracker Settings", "TRACKER_HIGHLIGHT_COLOR", new Color(255,0,0,255f), "Changes the color of the highlighted text.");
        }
    }
}