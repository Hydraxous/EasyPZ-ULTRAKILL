using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EasyPZ
{
    public class UIAutoPositioner : MonoBehaviour
    {
        
        public UIPositionData uIData;
        private RectTransform rTransform;

        private void Awake()
        {
            GetData();
            FixPosition();
            SetColors();
        }

        private void LateUpdate()
        {
            CheckPosition();
        }

        public void CheckPosition()
        {
            if (rTransform != null && uIData != null)
            {
                if (!ValidatePosition())
                {
                    FixPosition();
                    SetColors();
                }
            }
            else
            {
                GetData();
            }
        }

        private void GetData()
        {
            if (gameObject.name.Contains("Clone"))
            {
                gameObject.name = gameObject.name.Replace("(Clone)", "");
            }
            HydraLoader.dataRegistry.TryGetValue(gameObject.name + "_UIPD", out UnityEngine.Object obj);
            uIData = (UIPositionData)obj;
            rTransform = GetComponent<RectTransform>();
        }

        public void FixPosition()
        {
            rTransform.anchorMin = uIData.anchorMin;
            rTransform.anchorMax = uIData.anchorMax;
            rTransform.anchoredPosition = uIData.aPos;
            rTransform.sizeDelta = uIData.size;
            rTransform.pivot = uIData.pivot;
        }

        private void SetColors()
        {
            gameObject.GetComponent<Image>().color = uIData.bgColor;
            Text[] textComponents = gameObject.GetComponentsInChildren<Text>();
            Image[] imageComponents = gameObject.GetComponentsInChildren<Image>();
            for(int i=0;i<textComponents.Length;i++)
            {
                textComponents[i].color = uIData.fontColor;
            }
            for(int i=0;i<imageComponents.Length;i++)
            {
                if(imageComponents[i].gameObject != gameObject)
                {
                    imageComponents[i].color = uIData.fontColor;
                }
                
            }
        }

        private bool ValidatePosition()
        {
            if (rTransform.anchoredPosition != uIData.aPos ||
                rTransform.anchorMax != uIData.anchorMax ||
                rTransform.anchorMin != uIData.anchorMin ||
                rTransform.pivot != uIData.pivot ||
                rTransform.sizeDelta != uIData.size)
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
        public float borderSize;
        public enum AnchorSpot { topRight, topLeft, bottomRight, bottomLeft }
        public AnchorSpot anchorSpot;
        public Color bgColor;
        public Color fontColor;
        public Color highlightColor;

        public UIPositionData(Vector2 size, AnchorSpot anchorSpot, float borderSize, Color backgroundColor, Color fontColor, Color highlightColor)
        {
            this.bgColor = backgroundColor;
            this.fontColor = fontColor;
            this.highlightColor = highlightColor;
            this.aPos = Vector2.zero;
            this.size = size;
            borderSize *= 0.01f;
            this.anchorSpot = anchorSpot;
            switch(anchorSpot)
            {
                case AnchorSpot.topLeft:
                    this.anchorMin = new Vector2(0,1);
                    this.anchorMax = new Vector2(0,1);
                    this.pivot = new Vector2(0-borderSize, 1+borderSize);
                    break;
                case AnchorSpot.topRight:
                    this.anchorMin = new Vector2(1,1);
                    this.anchorMax = new Vector2(1,1);
                    this.pivot = new Vector2(1+borderSize,1+borderSize);
                    break;
                case AnchorSpot.bottomLeft:
                    this.anchorMin = new Vector2(0,0);
                    this.anchorMax = new Vector2(0,0);
                    this.pivot = new Vector2(0-borderSize,0-borderSize);
                    break;
                case AnchorSpot.bottomRight:
                    this.anchorMin = new Vector2(1,0);
                    this.anchorMax = new Vector2(1,0);
                    this.pivot = new Vector2(1+borderSize,0-borderSize);
                    break;
            }
        }

        public override string ToString()
        {
            return string.Format("|UIDATA pos: {0} |\n|size: {1} |\n| borderSize: {2} |\n| bgColor {3} |\n| fontColor {4} |\n| anchorPos {5} |\n| anchorMin {6} |\n| anchorMax {7}", aPos, size, borderSize, bgColor, fontColor, anchorSpot, anchorMin, anchorMax);

        }
    }

}