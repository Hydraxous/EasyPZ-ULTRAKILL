using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.SceneManagement;
using Configgy;
using HydraDynamics.Keybinds;
using HydraDynamics;
using HarmonyLib;


namespace EasyPZ
{
    [BepInPlugin("Hydraxous.ULTRAKILL.EasyPZ", "EasyPZ", "3.0.0")]
    [BepInDependency("Hydraxous.ULTRAKILL.Configgy", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("Hydraxous.HydraDynamics", BepInDependency.DependencyFlags.HardDependency)]
    public class EasyPZ : BaseUnityPlugin
    {
        [Configgy.Configgable("Options", "Enable P-Mode By Default")]
        private static ConfigToggle CFG_PModeDefault = new ConfigToggle(true);
        public static bool PModeDefault => CFG_PModeDefault.Value;

        [Configgy.Configgable("Options", "Always Show Tracker")]
        private static ConfigToggle CFG_AlwaysShowTracker = new ConfigToggle(true);
        public static bool AlwaysShowTracker => CFG_AlwaysShowTracker.Value;


        public static Keybinding Key_PModeToggle { get; private set; } = Hydynamics.GetKeybinding("Toggle P-Mode", KeyCode.P);
        public static Keybinding Key_RestartMission { get; private set; } = Hydynamics.GetKeybinding("Restart Mission", KeyCode.RightAlt);

        public static AssetLoader AssetLoader { get; private set; }
        public static EasyPZ Instance { get; private set; }

        public static bool PModeEnabled;
        private ConfigBuilder configBuilder;
        private Harmony harmony;

        private void Awake()
        {
            Instance = this;
            AssetLoader = new AssetLoader(Properties.EasyPZResources.EasyPZ);

            harmony = new Harmony(ConstInfo.GUID+".harmony");
            harmony.PatchAll();

            configBuilder = new ConfigBuilder(ConstInfo.GUID, ConstInfo.NAME);
            configBuilder.Build();

            Logger.LogInfo("EasyPZ Loaded. Good luck on P-ing in all the levels! :D");
        }

        private bool InLevel()
        {
            string sceneName = SceneManager.GetActiveScene().name;
            if (sceneName == "Intro" || sceneName == "Main Menu" || sceneName == "Endless" || sceneName == "uk_construct")
            {
                return false;
            }
            return true;
        }


        public static bool CheckPRankFailed()
        {
            if (AssistController.Instance.cheatsEnabled || AssistController.Instance.majorEnabled) 
            {
                return true;
            }

            bool fail = false;

            //Checks if player died.
            if (NewMovement.Instance.dead || StatsManager.Instance.seconds > StatsManager.Instance.timeRanks[3]) 
            { 
                fail = true; 
            }
            //checks if you failed p req at the end of the level.
            else if (StatsManager.Instance.infoSent && (StatsManager.Instance.kills < StatsManager.Instance.killRanks[3] || StatsManager.Instance.stylePoints < StatsManager.Instance.styleRanks[3])) 
            { 
                fail = true;
            } 

            return fail;
        }
    }
}