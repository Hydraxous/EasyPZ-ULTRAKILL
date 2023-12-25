using UnityEngine;
using UnityEngine.EventSystems;

namespace EasyPZ.UIEdit
{
    public class MovableWindow : MonoBehaviour, IBeginDragHandler, IDragHandler
    {
        [SerializeField] private RectTransform target;

        private Canvas canvas;

        public void OnBeginDrag(PointerEventData eventData)
        {
            canvas = target.GetComponentInParent<Canvas>();
        }

        public void OnDrag(PointerEventData eventData)
        {
            target.anchoredPosition += eventData.delta / canvas.scaleFactor;
        }

    }
}

