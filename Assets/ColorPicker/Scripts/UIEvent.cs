using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIEvent : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler
{
    public Action<PointerEventData> OnPointerDownCallback;

    public Action<PointerEventData> OnBeginDragCallback;
	public Action<PointerEventData> OnDragCallback;
    public Action<PointerEventData> OnEndDragCallback;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (OnPointerDownCallback != null)
            OnPointerDownCallback(eventData);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (OnBeginDragCallback != null)
            OnBeginDragCallback(eventData);
    }

	public void OnDrag(PointerEventData eventData)
    {
        if (OnDragCallback != null)
            OnDragCallback(eventData);
    }


	public void OnEndDrag(PointerEventData eventData)
	{
        if (OnEndDragCallback != null)
            OnEndDragCallback(eventData);
    }
}
