using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace EasyPZ.Patches
{
    [HarmonyPatch(typeof(CanvasController))]
    public static class InstanceUI
    {

        [HarmonyPatch("Awake"), HarmonyPostfix]
        public static void OnAwake(CanvasController __instance)
        {
            RectTransform rt = __instance.GetComponent<RectTransform>();
            if(rt == null)
            {
                Debug.LogError("EZPZ: Canvas controller patch issue!, RectTransform could not be found!");
                return;
            }
            InstanceElements(rt);
        }

        private static void InstanceElements(RectTransform root)
        {
            GameObject.Instantiate(Prefabs.TrackerManager, root);
        }
    }
}
