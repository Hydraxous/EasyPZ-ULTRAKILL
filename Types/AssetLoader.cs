using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace EasyPZ
{
    public class AssetLoader
    {
        public AssetBundle Bundle { get; }
        private Dictionary<string, UnityEngine.Object> loadedAssets = new Dictionary<string, UnityEngine.Object>();

        public AssetLoader(AssetBundle bundle)
        {
            this.Bundle = bundle;
        }

        public AssetLoader(byte[] bundleBytes)
        {
            this.Bundle = AssetBundle.LoadFromMemory(bundleBytes);
        }

        public AssetLoader(string filePath)
        {
            this.Bundle = AssetBundle.LoadFromFile(filePath);
        }


        public T LoadAsset<T>(string assetName) where T : UnityEngine.Object
        {
            if (loadedAssets.ContainsKey(assetName))
                return (T)loadedAssets[assetName];

            T asset = Bundle.LoadAsset<T>(assetName);

            if (asset == null)
                return null;

            loadedAssets.Add(assetName, asset);
            return asset;
        }

        public T[] LoadAllAssets<T>() where T : UnityEngine.Object
        {
            T[] assets = Bundle.LoadAllAssets<T>();

            foreach (T asset in assets)
            {
                if (asset == null)
                    continue;

                if (!loadedAssets.ContainsKey(asset.name))
                    loadedAssets.Add(asset.name, asset);
            }

            return assets;
        }

        public void Unload(bool unloadAllLoadedObjects = true)
        {
            Bundle.Unload(unloadAllLoadedObjects);
        }
    }
}