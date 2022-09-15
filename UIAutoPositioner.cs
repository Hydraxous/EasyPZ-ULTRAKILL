using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EasyPZ
{
    public class UIAutoPositioner : MonoBehaviour
    {
        public UIPositionData uIPositionData = new UIPositionData(new Vector2(-10, 10), new Vector2(165f, 202.5f), new Vector2(1, 0), new Vector2(1, 0), new Vector2(1, 0));
        private RectTransform rTransform;

        private void Start()
        {
            GetPositionData();
            rTransform = GetComponent<RectTransform>();
            CheckPosition();
        }

        private void GetPositionData()
        {
            try
            {
                HydraLoader.uIDataRegistry.TryGetValue(gameObject.name, out UIPositionData uIPositionData);
            }
            catch(System.Exception e)
            {
                Debug.Log("Error getting UIposition data for " + gameObject.name);
            }
            
        }

        public void CheckPosition()
        {
            if (rTransform != null || uIPositionData != null)
            {
                if (!ValidatePosition())
                {
                    FixPosition();
                }
            }
        }

        public void FixPosition()
        {
            rTransform.anchorMin = uIPositionData.anchorMin;
            rTransform.anchorMax = uIPositionData.anchorMax;
            rTransform.anchoredPosition = uIPositionData.aPos;
            rTransform.sizeDelta = uIPositionData.size;
            rTransform.pivot = uIPositionData.pivot;
        }

        private bool ValidatePosition()
        {

            if (rTransform.anchoredPosition != uIPositionData.aPos ||
                rTransform.anchorMax != uIPositionData.anchorMax ||
                rTransform.anchorMin != uIPositionData.anchorMin ||
                rTransform.pivot != uIPositionData.pivot ||
                rTransform.sizeDelta != uIPositionData.size)
            {
                return false;
            }
            return true;
        }

        private void OnEnable()
        {
            CheckPosition();
        }

        private void OnDisable()
        {
            CheckPosition();
        }
    }

    public class UIPositionData
    {
        public Vector2 aPos, size, anchorMin, anchorMax, pivot;

        public UIPositionData(Vector2 aPos, Vector2 size, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot)
        {
            this.aPos = aPos;
            this.size = size;
            this.anchorMin = anchorMin;
            this.anchorMax = anchorMax;
            this.pivot = pivot;
        }
    }

}