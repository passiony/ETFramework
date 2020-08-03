using UnityEngine;

namespace ETModel
{
    /// <summary>
    /// 刘海平适配组件
    /// 使用方式：挂载到需要适配的RectTransform上面即可
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class ScreenFitter : MonoBehaviour
    {
        private RectTransform rectTransform;
        private bool isFit;

        void Awake()
        {
            rectTransform = transform as RectTransform;
        }
        
        public void Fit(int indentation)
        {
            if (isFit) return;
            isFit = true;
            
            Rect rect = rectTransform.rect;
            Vector2 anchored = rectTransform.anchoredPosition;
            float x = anchored.x;
            float y = anchored.y - indentation * 0.5f;
            float width = rect.width;
            float height = rect.height - indentation;

            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            rectTransform.anchoredPosition = new Vector2(x, y);
        }
        
    }
}
