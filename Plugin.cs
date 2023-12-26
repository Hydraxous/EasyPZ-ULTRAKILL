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
        public static Keybinding Key_PModeToggle { get; private set; } = Hydynamics.GetKeybinding("Toggle Auto Reset", KeyCode.P);
        public static Keybinding Key_RestartMission { get; private set; } = Hydynamics.GetKeybinding("Restart Mission", KeyCode.RightAlt);

        public static AssetLoader AssetLoader { get; private set; }
        public static EasyPZ Instance { get; private set; }

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

            InGameCheck.Init();
            GhostManager.PreloadGhostPrefab();

            Logger.LogInfo("EasyPZ Loaded. Good luck on P-ing in all the levels! :D");
        }

        //[Configgable("Options", "Open Keybindings")]
        private static void OpenKeybinds()
        {
            //yikes.
        }
    }
}