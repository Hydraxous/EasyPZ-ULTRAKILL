using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace EasyPZ
{
    internal static class Data
    {
        private static EasyPZCache cache;
        internal static EasyPZCache Cache
        {
            get
            {
                if(cache == null)
                {
                    LoadCache();
                }
                
                return cache;
            }
        }

        private const string cacheFileName = "cache.json";
        private static string cacheFilePath => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), cacheFileName);

        private static void LoadCache()
        {
            if(!File.Exists(cacheFilePath))
            {
                cache = new EasyPZCache();
                return;
            }

            try
            {
                string json = File.ReadAllText(cacheFilePath);
                cache = JsonConvert.DeserializeObject<EasyPZCache>(json);
                if(cache == null)
                    throw new Exception("Cache data is null or corrupted.");

            } catch(Exception e)
            {
                Debug.LogError($"Failed to load {ConstInfo.NAME} cache file");
                Debug.LogException(e);
                cache = new EasyPZCache();
            }

        }

        public static void SaveCache()
        {
            if (cache == null)
                return;

            string json = JsonConvert.SerializeObject(cache, Formatting.Indented);
            File.WriteAllText(cacheFilePath, json);
        }
    }

}

[Serializable]
public class EasyPZCache
{
    public bool AskedAboutRecording;
}
