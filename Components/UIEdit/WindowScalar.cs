using UnityEngine;
using UnityEngine.EventSystems;

namespace EasyPZ.UIEdit
{
    public class WindowScalar : MonoBehaviour, IBeginDragHandler, IDragHandler
    {
        [SerializeField] private RectTransform target;
        [SerializeField] private Vector2 maxSize;
        [SerializeField] private Vector2 minSize;

        [SerializeField] private TextAnchor scaleDirection;

        private Canvas canvas;

        public void OnBeginDrag(PointerEventData eventData)
        {
            canvas = target.GetComponentInParent<Canvas>();
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector2 scaleDelta = Vector2.Scale(GetMultiplier(scaleDirection), (eventData.delta / canvas.scaleFactor));

            Vector2 finalDelta = target.sizeDelta + scaleDelta;

            finalDelta.x = Mathf.Clamp(finalDelta.x, minSize.x, maxSize.x);
            finalDelta.y = Mathf.Clamp(finalDelta.y, minSize.y, maxSize.y);

            target.sizeDelta = finalDelta;
        }

        private Vector2 GetMultiplier(TextAnchor alignment)
        {
            switch (alignment)
            {
                case TextAnchor.UpperLeft:
                    return new Vector2(-1f, 1f);
                case TextAnchor.UpperRight:
                    return Vector2.one;
                case TextAnchor.LowerLeft:
                    return -Vector2.one;
                case TextAnchor.LowerRight:
                    return new Vector2(1f, -1f);
                case TextAnchor.LowerCenter:
                    return new Vector2(0f, -1f);
                case TextAnchor.UpperCenter:
                    return new Vector2(0f, 1f);
                case TextAnchor.MiddleCenter:
                    return Vector2.zero;
                case TextAnchor.MiddleLeft:
                    return new Vector2(-1, 0f);
                case TextAnchor.MiddleRight:
                    return new Vector2(1, 0f);
            }

            return Vector2.zero;
        }
    }
}