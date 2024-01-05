using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.SceneManagement;
using Configgy;
using HarmonyLib;
using EasyPZ.Components;
using JetBrains.Annotations;


namespace EasyPZ
{
    [BepInPlugin("Hydraxous.ULTRAKILL.EasyPZ", "EasyPZ", "3.0.0")]
    [BepInDependency("Hydraxous.ULTRAKILL.Configgy", BepInDependency.DependencyFlags.HardDependency)]
    public class EasyPZ : BaseUnityPlugin
    {
        public static AssetLoader AssetLoader { get; private set; }
        public static EasyPZ Instance { get; private set; }

        public static ConfigBuilder ConfigBuilder { get; private set; }

        private Harmony harmony;

        public static string LatestVersion { get; private set; } = ConstInfo.VERSION;
        public static bool UsingLatestVersion { get; private set; } = true;


        private void Awake()
        {
            Instance = this;
            AssetLoader = new AssetLoader(Properties.EasyPZResources.EasyPZ);

            harmony = new Harmony(ConstInfo.GUID+".harmony");
            harmony.PatchAll();

            ConfigBuilder = new ConfigBuilder(ConstInfo.GUID, ConstInfo.NAME);
            ConfigBuilder.Build();

            InGameCheck.Init();
            GhostManager.PreloadGhostPrefab();

            VersionCheck.CheckVersion(ConstInfo.GITHUB_VERSION_URL, ConstInfo.VERSION, (r, version) =>
            {
                UsingLatestVersion = r;
                if (!r)
                {
                    LatestVersion = version;
                    Debug.LogWarning($"EasyPZ is out of date. New version available ({version})");
                }

            });

            Logger.LogInfo("EasyPZ Loaded. Good luck on P-ing in all the levels! :D");
        }
    }
}