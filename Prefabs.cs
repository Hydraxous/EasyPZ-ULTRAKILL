using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace EasyPZ
{
    public static class Prefabs
    {
        public static GameObject ClassicTrackerPrefab => EasyPZ.AssetLoader.LoadAsset<GameObject>("ClassicTracker");
        public static GameObject StandardTracker => EasyPZ.AssetLoader.LoadAsset<GameObject>("StandardTracker");
        public static GameObject CompactTracker => EasyPZ.AssetLoader.LoadAsset<GameObject>("CompactTracker");
        public static GameObject TrackerManager => EasyPZ.AssetLoader.LoadAsset<GameObject>("TrackerManager");
    }
}
