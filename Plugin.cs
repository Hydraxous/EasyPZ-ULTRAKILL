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

            Logger.LogInfo("EasyPZ Loaded. Good luck on P-ing in all the levels! :D");
        }
    }
}