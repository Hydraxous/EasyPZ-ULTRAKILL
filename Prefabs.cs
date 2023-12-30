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
        public static GameObject RunManagerMenu => EasyPZ.AssetLoader.LoadAsset<GameObject>("RunManagerMenu");
        public static GameObject PlayerNameplate => EasyPZ.AssetLoader.LoadAsset<GameObject>("PlayerNameplate");
        public static GameObject GhostSavedNotifier => EasyPZ.AssetLoader.LoadAsset<GameObject>("GhostSavedNotifier");
        public static Material GhostMaterial => EasyPZ.AssetLoader.LoadAsset<Material>("GhostMaterial");
        public static AudioClip ShittyBoom => EasyPZ.AssetLoader.LoadAsset<AudioClip>("ShittyBoom");
        public static Font VCR_Font => EasyPZ.AssetLoader.LoadAsset<Font>("VCR_OSD_MONO");
    }
}
