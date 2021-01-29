using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Framework
{
    public class DropMe : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public Image containerImage;

        public Action<GameObject, GameObject> callback;

        private Color normalColor;
        public Color highlightColor = Color.yellow;

        public void OnEnable()
        {
            if (containerImage != null)
                normalColor = containerImage.color;
        }

        public void OnDrop(PointerEventData data)
        {
            containerImage.color = normalColor;

            Sprite dropSprite = GetDropSprite(data);
            if (dropSprite != null)
            {
                if (callback != null) callback(gameObject, data.pointerDrag);
            }
        }

        public void OnPointerEnter(PointerEventData data)
        {
            if (containerImage == null)
                return;

            Sprite dropSprite = GetDropSprite(data);
            if (dropSprite != null)
                containerImage.color = highlightColor;
        }

        public void OnPointerExit(PointerEventData data)
        {
            if (containerImage == null)
                return;

            containerImage.color = normalColor;
        }

        private Sprite GetDropSprite(PointerEventData data)
        {
            var originalObj = data.pointerDrag;
            if (originalObj == null)
                return null;

            var dragMe = originalObj.GetComponent<DragMe>();
            if (dragMe == null)
                return null;

            var srcImage = originalObj.GetComponent<Image>();
            if (srcImage == null)
                return null;

            return srcImage.sprite;
        }
    }

}