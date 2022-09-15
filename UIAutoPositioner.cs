using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EasyPZ
{
    public class UIAutoPositioner : MonoBehaviour
    {
        private UIPositionData uIPositionData;
        private RectTransform rTransform;

        private void Start()
        {
            GetPositionData();
            rTransform = GetComponent<RectTransform>();
            CheckPosition();
        }

        private void GetPositionData()
        {
            HydraLoader.dataRegistry.TryGetValue(gameObject.name+"_UIPD", out Object dataGet);
            uIPositionData = (UIPositionData) dataGet;
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

            if (
                rTransform.anchoredPosition != uIPositionData.aPos ||
                rTransform.anchorMax != uIPositionData.anchorMax ||
                rTransform.anchorMin != uIPositionData.anchorMin ||
                rTransform.pivot != uIPositionData.pivot ||
                rTransform.sizeDelta != uIPositionData.size
                )
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

    public class UIPositionData : DataFile
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