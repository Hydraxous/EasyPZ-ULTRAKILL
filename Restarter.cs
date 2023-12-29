using Configgy;
using EasyPZ.Components;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Collections;
using System.IO;

namespace EasyPZ
{
    public static class Restarter
    {
        [Configgable("Auto Restart", "Restart Type")]
        private static ConfigDropdown<RestartType> restartType = new ConfigDropdown<RestartType>((RestartType[])Enum.GetValues(typeof(RestartType)), ((RestartType[])Enum.GetValues(typeof(RestartType))).Select(x => x.ToString()).ToArray(), 0);

        [Configgable("Auto Restart", "Sound Effect On Auto Restart")]
        private static ConfigToggle soundEffectOnAutoRestart = new ConfigToggle(true);

        [Configgable("Auto Restart", "Use custom sound")]
        private static ConfigToggle useCustomSoundEffect = new ConfigToggle(false);

        [Configgable("Auto Restart", "Custom sound path")]
        private static ConfigInputField<string> soundFileLocation = new ConfigInputField<string>("", ValidateSoundFileLocation);

        private static AudioClip _customSound;

        private static AudioClip customSound
        {
            get
            {
                if(_customSound == null)
                {
                    //guh
                }
                return _customSound;
            }
        }


        private static bool ValidateSoundFileLocation(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Debug.LogError($"File {filePath} not found!");
                return false;
            }

            for(int i = 0; i < fileFormats.Length; i++)
            {
                if (filePath.EndsWith(fileFormats[i]))
                    return true;
            }

            Debug.LogError($"File {filePath} filetype not supported!");
            return false;
        }

        private static readonly string[] fileFormats = new string[] { ".wav", ".mp3", ".ogg" };

        public static void Restart()
        {
            if (soundEffectOnAutoRestart.Value)
                audioSource.Play();

            if(restartType.Value == RestartType.Explosion)
            {
                //Debug.Log("BOOM!");
                //EasyPZ.Instance.StartCoroutine(RestartAfterTime(0.5f));
                //return;
            }
            
            OptionsManager.Instance.RestartMission();
        }

        private static IEnumerator RestartAfterTime(float time)
        {
            yield return new WaitForSeconds(time);
            OptionsManager.Instance.RestartMission();
        }

        private static AudioSource _audioSource;

        private static AudioSource audioSource
        {
            get
            {
                if (_audioSource == null)
                {
                    _audioSource = new GameObject("EZPZ Beeper").AddComponent<AudioSource>();

                    _audioSource.playOnAwake = false;
                    _audioSource.loop = false;
                    _audioSource.volume = 1f;
                    _audioSource.spatialBlend = 0f;
                    _audioSource.clip = restartClip;

                    GameObject.DontDestroyOnLoad(_audioSource.gameObject);
                }

                return _audioSource;
            }
        }

        private static AudioClip _restartClip;
        private static AudioClip restartClip
        {
            get
            {
                if(_restartClip == null)
                {
                    _restartClip = Addressables.LoadAssetAsync<AudioClip>("Assets/Music/Hits/gong.wav").WaitForCompletion();
                }
                return _restartClip;
            }
        }
    }

    public enum RestartType 
    {
        Standard,
        Explosion,
    }

}
