using System;
using System.Collections.Generic;
using UnityEngine;
using EasyPZ.Properties;

namespace EasyPZ
{
    public static class HydraLoader
    {
        private static List<CustomAsset> assetsToRegister = new List<CustomAsset>();

        private static List<CustomAssetData> dataToRegister = new List<CustomAssetData>();

        public static Dictionary<string, UnityEngine.Object> dataRegistry = new Dictionary<string, UnityEngine.Object>();

        public static Dictionary<string, GameObject> prefabRegistry = new Dictionary<string, GameObject>();

        public static bool dataRegistered = false;
        public static bool assetsRegistered = false;

        public static bool RegisterAll()
        {
            try
            {
                RegisterDataFiles();
                RegisterCustomAssets();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public static void RegisterDataFiles()
        {
            if (!dataRegistered)
            {
                foreach(CustomAssetData assetData in dataToRegister)
                {
                    dataRegistry.Add(assetData.name, assetData.dataFile);
                }

                dataRegistered = true;
                Debug.Log("data registered!");
            }
        }

        public static void RegisterCustomAssets()
        {
            if (!assetsRegistered)
            {
                AssetBundle assetBundle = AssetBundle.LoadFromMemory(EasyPZResources.EasyPZ);

                foreach(CustomAsset asset in assetsToRegister)
                {
                    try
                    {
                        GameObject newPrefab = assetBundle.LoadAsset<GameObject>(asset.name);
                        for (int i = 0; i < asset.modules.Length; i++)
                        {  
                            newPrefab.AddComponent(asset.modules[i].GetType());      
                        }
                        prefabRegistry.Add(asset.name, newPrefab);
                    }catch(Exception e)
                    {
                        Debug.Log("Failed to load asset: " + asset.name);
                    }
                }

                assetsRegistered = true;
                Debug.Log("assets registered!");
            }
        }

        public class CustomAsset
        {
            public Component[] modules;
            public string name;

            public CustomAsset(string assetName, Component[] componentsToAdd)
            {
                this.name = assetName;
                this.modules = componentsToAdd;
                assetsToRegister.Add(this);
            }
        }

        public class CustomAssetData
        {
            public string name;
            public DataFile dataFile;

            public CustomAssetData(string dataName, DataFile dataFile)
            {
                this.name = dataName;
                this.dataFile = dataFile;
                dataToRegister.Add(this);
            }
              
        }
    }

    public class DataFile : UnityEngine.Object { }
}
