using Configgy;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EasyPZ
{
    public static class InGameCheck
    {
        private static bool initialized;
        public static void Init()
        {
            if (!initialized)
            {
                initialized = true;
                SceneManager.sceneLoaded += OnSceneLoad;
            }
        }

        /// <summary>
        /// Enumerated version of the Ultrakill scene types
        /// </summary>
        public enum UKLevelType { Intro, MainMenu, Level, Endless, Sandbox, Credits, Custom, Intermission, Secret, PrimeSanctum, Unknown }

        /// <summary>
        /// Returns the current level type
        /// </summary>
        public static UKLevelType CurrentLevelType = UKLevelType.Intro;

        /// <summary>
        /// Returns the currently active ultrakill scene name.
        /// </summary>
        public static string CurrentSceneName = "";

        public delegate void OnLevelChangedHandler(UKLevelType uKLevelType);

        /// <summary>
        /// Invoked whenever the current level type is changed.
        /// </summary>
        public static OnLevelChangedHandler OnLevelTypeChanged;

        /// <summary>
        /// Invoked whenever the scene is changed.
        /// </summary>
        public static OnLevelChangedHandler OnLevelChanged;

        //Perhaps there is a better way to do this.
        private static void OnSceneLoad(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (scene != SceneManager.GetActiveScene())
                return;

            UKLevelType newScene = GetUKLevelType(SceneHelper.CurrentScene);

            if (newScene != CurrentLevelType)
            {
                CurrentLevelType = newScene;
                CurrentSceneName = SceneHelper.CurrentScene; //grr
                OnLevelTypeChanged?.Invoke(newScene);
            }



            OnLevelChanged?.Invoke(CurrentLevelType);
        }

        //Perhaps there is a better way to do this. Also this will most definitely cause problems in the future if PITR or Hakita rename any scenes.

        /// <summary>
        /// Gets enumerated level type from the name of a scene.
        /// </summary>
        /// <param name="sceneName">Name of the scene</param>
        /// <returns></returns>
        public static UKLevelType GetUKLevelType(string sceneName)
        {
            sceneName = (sceneName.Contains("P-")) ? "Sanctum" : sceneName;
            sceneName = (sceneName.Contains("-S")) ? "Secret" : sceneName;
            sceneName = (sceneName.Contains("Level")) ? "Level" : sceneName;
            sceneName = (sceneName.Contains("Intermission")) ? "Intermission" : sceneName;

            switch (sceneName)
            {
                case "Main Menu":
                    return UKLevelType.MainMenu;
                case "Custom Content":
                    return UKLevelType.Custom;
                case "Intro":
                    return UKLevelType.Intro;
                case "Endless":
                    return UKLevelType.Endless;
                case "uk_construct":
                    return UKLevelType.Sandbox;
                case "Intermission":
                    return UKLevelType.Intermission;
                case "Level":
                    return UKLevelType.Level;
                case "Secret":
                    return UKLevelType.Secret;
                case "Sanctum":
                    return UKLevelType.PrimeSanctum;
                case "CreditsMuseum2":
                    return UKLevelType.Credits;
                default:
                    return UKLevelType.Unknown;
            }
        }

        [Configgable("Extras/Advanced", "Force Tracker In All Scenes")]
        private static ConfigToggle CFG_ForceInLevelCheckTrue = new ConfigToggle(false);

        /// <summary>
        /// Returns true if the current scene is playable.
        /// This will return false for all secret levels.
        /// </summary>
        /// <returns></returns>
        public static bool InLevel()
        {
            if (CFG_ForceInLevelCheckTrue.Value)
                return true;

            switch (CurrentLevelType)
            {
                case UKLevelType.MainMenu:
                case UKLevelType.Intro:
                case UKLevelType.Intermission:
                case UKLevelType.Secret:
                    return false;
                default:
                    return true;
            }
        }
    }
}