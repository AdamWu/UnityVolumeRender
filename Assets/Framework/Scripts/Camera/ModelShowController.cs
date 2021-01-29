/*
 * Model Mouse Handle with Rotating, Zooming, Moving
*/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ModelShowController : MonoBehaviour {

	public enum MouseButton {
		LEFT=0,
		RIGHT,
		MIDDLE,
	}

    public enum TouchScreen
    {
        ONE,
        TWO,
    }

	[Tooltip("Which Camera to Render? Default is Camera.main")]
	public Camera m_TargetCamera;
		
	// rotate
	[Header("Rotate")]
	public bool m_RotateEnable = true;
	public Vector2 m_RotateSpeed = new Vector2(0.2f, 0.1f);
	public MouseButton m_RotateMouseButton = MouseButton.LEFT;
    private TouchScreen m_RotateTouchScreen = TouchScreen.ONE;
	public Space m_RotateYSpace = Space.Self;
	[Tooltip("Anchor Rotate, if set successfully, it can rotate with diffierent anchor")]
	public Transform m_RotateAnchor;

	// zoom
	[Header("Zoom")]
	public bool m_ZoomEnable = true;
	public float m_ZoomSpeed = 2f;
    public float m_ZoomTouchSpeed = 0.3f;
    [Tooltip("Zoom With Mouse?")]
	public bool m_ZoomFree = false;
    [Tooltip("Zoom count")]
    private float offsetTouchStart;
    private float CameraMax = 100;
    private float CameraMin = -100;

    // move
    [Header("Move")]
	public bool m_MoveEnable = true;
	public MouseButton m_MoveMouseButton = MouseButton.MIDDLE;

    [Header("control开关")]
    bool bRotate = false;
    [Header("control开关")]
    bool bZoom = false;
    [Header("control开关")]
    bool bMove = false;
    Vector3 MousePosPre;
    Vector2 TouchPosPre;
    Vector2 TouchCenter;
    public bool bControl = false;

	bool bMoving = false;
	Vector3 screenPos, moveOffset;

	Vector3 m_PositionInit;
	Quaternion m_RotationInit;
	Vector3 m_CameraPositonInit;
	Vector3 m_AnchorPositionInit;
    private float FarthestCameraDistance = 80;

	public void SetAnchor(Vector3 pos) {
		if (m_RotateAnchor) {
			Vector3 posPre = m_RotateAnchor.position;
			Vector3 anchor = m_RotateAnchor.InverseTransformPoint (pos);
			anchor.x *= m_RotateAnchor.transform.localScale.x;
			anchor.y *= m_RotateAnchor.transform.localScale.y;
			anchor.z *= m_RotateAnchor.transform.localScale.z;
			m_RotateAnchor.localPosition = -anchor;
			transform.position -= (m_RotateAnchor.position - posPre);
		}
	}
	public void ResetAnchor() {
		SetAnchor (m_AnchorPositionInit);
	}

	public void Reset() {
		transform.position = m_PositionInit;
		transform.rotation = m_RotationInit;
        if (m_TargetCamera != null) m_TargetCamera.transform.position = m_CameraPositonInit;
		if (m_RotateAnchor) {
			m_RotateAnchor.position = m_AnchorPositionInit;
		}
	}

	// Use this for initialization
	void Start () {
		if (m_TargetCamera == null)
			m_TargetCamera = Camera.main;
		
		if (m_RotateAnchor) {
			m_AnchorPositionInit = m_RotateAnchor.position;
		}

		m_PositionInit = transform.position;
		m_RotationInit = transform.rotation;
		m_CameraPositonInit = m_TargetCamera.transform.position;
	}
	
	// Update is called once per frame
	void Update () {

        if (!bControl &&  !bRotate && !bMoving&& IsPointerOverUIObject())
            return;
        if (Input.GetMouseButtonDown (0)) {
			Ray ray = m_TargetCamera.ScreenPointToRay (Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast (ray, out hit)) {
				//SetAnchor (hit.transform.position);
			}
		}

        Rotate ();
        Zoom();
        Move();

	    RotateTouch();
        MoveTouch();
        ZoomTouch();

	    MousePosPre = Input.mousePosition;
	}

    bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }

	void Zoom() {

		if (!m_ZoomEnable) return;

	    if (Input.touchCount > 0)
	        return;
	    Vector3 direction = transform.position - m_TargetCamera.transform.position;
        float offset = Input.GetAxis ("Mouse ScrollWheel");

		if (m_ZoomFree) {
			Vector3 dir = m_TargetCamera.ScreenPointToRay(Input.mousePosition).direction;
			Vector3 offsetVec = offset * dir * m_ZoomSpeed;
			Vector3 offsetVec_local = m_TargetCamera.transform.InverseTransformVector (offsetVec);
			m_TargetCamera.transform.Translate (offsetVec_local);

		} else
		{
            //if((m_TargetCamera.transform.position.z>=CameraMax && offset>0)|| m_TargetCamera.transform.position.z <= CameraMin && offset < 0)
            //    return;
		    m_TargetCamera.transform.Translate(direction * offset);
        }
	}

    private Vector3 direction;
    void ZoomTouch()
    {
        if (!m_ZoomEnable) return;

        if(Input.touchCount != 2)
            return;

        if (Input.GetTouch(0).phase == TouchPhase.Ended || Input.GetTouch(1).phase == TouchPhase.Ended)
        {
            bZoom = false;
        }
       
        if (bMove || Vector2.Dot(Input.GetTouch(0).deltaPosition, Input.GetTouch(1).deltaPosition) > 0 || (Input.GetTouch(0).deltaPosition == Vector2.zero && Input.GetTouch(1).deltaPosition == Vector2.zero))
        {
            bZoom = false;
            return;
        }

        if (Input.GetTouch(0).phase == TouchPhase.Began || Input.GetTouch(1).phase == TouchPhase.Began)
        {
            offsetTouchStart = Vector2.Distance(Input.GetTouch(0).position, Input.GetTouch(1).position);
            return;
        }

        if (bZoom)
        {
            float offset = Vector2.Distance(Input.touches[0].position, Input.touches[1].position) / offsetTouchStart;
            Vector3 value = transform.position - direction / offset;      //已模型中心缩放
            //Vector3 value = m_TargetCamera.transform.position / offset; //已屏幕中心点缩放
            if (offset>1 || Vector3.Distance(value, transform.position) <= FarthestCameraDistance)
            {
                //if (value.z >= CameraMax) return;
                m_TargetCamera.transform.position = value;
                bZoom = false;
            }    
            
        }

        if (Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(1).phase == TouchPhase.Moved)
        {
            direction = transform.position - m_TargetCamera.transform.position;
            offsetTouchStart = Vector2.Distance(Input.GetTouch(0).position, Input.GetTouch(1).position);
            bZoom = true;
        }
    }

    void Rotate() {

		if (!m_RotateEnable) return;

	    if (Input.touchCount > 0) return;

		if (bRotate) {
			Vector3 offset = Input.mousePosition - MousePosPre;
            transform.Rotate (new Vector3(offset.y * m_RotateSpeed.y, 0, 0), Space.World);
			transform.Rotate (new Vector3(0, -offset.x * m_RotateSpeed.x, 0), m_RotateYSpace);
		}

		if (Input.GetMouseButtonDown ((int)m_RotateMouseButton)) {
			bRotate = true;
		}

		if (Input.GetMouseButtonUp ((int)m_RotateMouseButton)) {
			bRotate = false;
		}
	}

    void RotateTouch()
    {
        if (!m_RotateEnable) return;

        if (Input.touchCount == (int)m_RotateTouchScreen + 1)
        {
            Vector3 offset = Input.GetTouch(0).deltaPosition;
            transform.Rotate(new Vector3(offset.y * m_RotateSpeed.y, 0, 0), Space.World);
            transform.Rotate(new Vector3(0, -offset.x * m_RotateSpeed.x, 0), m_RotateYSpace);
        }
    }

    void Move() {

		if (!m_MoveEnable) return;

        if(Input.touchCount >0)
            return;

		if (Input.GetMouseButtonDown ((int)m_MoveMouseButton)) {
			bMoving = true;
			Vector3 mousePos = Input.mousePosition;
			screenPos = m_TargetCamera.WorldToScreenPoint (transform.position);
			mousePos.z = screenPos.z;
			moveOffset = transform.position - m_TargetCamera.ScreenToWorldPoint (mousePos);
		}

		if (bMoving) {
			Vector3 curScreenPos = new Vector3 (Input.mousePosition.x, Input.mousePosition.y, screenPos.z);
			Vector3 curPos = m_TargetCamera.ScreenToWorldPoint (curScreenPos) + moveOffset;
			transform.position = curPos;
		}

		if (Input.GetMouseButtonUp ((int)m_MoveMouseButton)) {
			bMoving = false;
		}
	}

    void MoveTouch()
    {
        if (!m_MoveEnable) return;

        if (Input.touchCount != 2)
            return;

        if (bZoom || Vector2.Dot(Input.GetTouch(0).deltaPosition, Input.GetTouch(1).deltaPosition) < 0 ||
            Input.GetTouch(0).deltaPosition == Vector2.zero || Input.GetTouch(1).deltaPosition == Vector2.zero)
        {
            bMove = false;
            return;
        }

        if (bMove==false)
        {
            TouchCenter = (Input.GetTouch(0).position + Input.GetTouch(1).position) / 2;
            Vector3 mousePos = TouchCenter;
            screenPos = m_TargetCamera.WorldToScreenPoint(transform.position);
            mousePos.z = screenPos.z;
            moveOffset = transform.position - m_TargetCamera.ScreenToWorldPoint(mousePos);
        }

        if ((Input.touches[0].phase == TouchPhase.Moved && Input.touches[1].phase == TouchPhase.Moved))
        {
            bMove = true;
            Vector3 curScreenPos = new Vector3(TouchCenter.x, TouchCenter.y, screenPos.z);
            Vector3 curPos = m_TargetCamera.ScreenToWorldPoint(curScreenPos) + moveOffset;
            transform.position = curPos;
            TouchCenter = (Input.GetTouch(0).position + Input.GetTouch(1).position) / 2;
        }
        else
        {
            bMove = false;
        }
    }
}
	