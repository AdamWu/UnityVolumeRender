using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ControlPoint : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    public ControlPointManager Manager { get; set; }

    private RectTransform rectTransform;
    private Image image;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();
    }
    
    public void SetSelected(bool value)
    {
        image = GetComponent<Image>();
        if (value)
        {
            image.color = new Color(0.9f, 0.6f, 0.1f);
        } else
        {
            image.color = Color.gray;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("OnPointerClick "+name);

        Manager.OnControlPointSelect(this);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector3 pos;
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(rectTransform, eventData.position, null, out pos))
        {
            rectTransform.position = pos;
            Manager.CalculateControlPointPosition();
        }
    }
}
