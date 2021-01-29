using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using Framework;

public class ControlPointManager : MonoBehaviour, IPointerDownHandler
{
    public bool LockY = false;

    public int GetSelectedIdx() { return m_SelectedIdx; }
    
    private GameObject Prefab;

    private List<ControlPoint> m_ControlPoints = new List<ControlPoint>();

    private ControlPoint m_Selected;
    private int m_SelectedIdx = -1;

    // delete删除
    private bool WaitUpdate = false;
    private bool ActiveDelete = false;

    void Start()
    {
        Prefab = transform.Find("ControlPoint").gameObject;
        Prefab.SetActive(false);
    }

    private void Update()
    {
        if (ActiveDelete && Input.GetKeyDown(KeyCode.Delete))
        {
            DeleteControlPoint();
        }

        if (ActiveDelete && !WaitUpdate && (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2)))
        {
            ActiveDelete = false;
        }

        WaitUpdate = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        //Debug.Log("OnPointerClick "+name);

        RectTransform rt = transform as RectTransform;

        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, eventData.position, null, out localPos);

        Vector2 normalizePos = localPos / rt.sizeDelta;
        AddControlPoint(normalizePos);
    
    }

    // 查找控制点索引
    private int FindControlPoint(ControlPoint controlPoint)
    {
        for (int i = 0; i < m_ControlPoints.Count; i++)
        {
            if (m_ControlPoints[i] == controlPoint)
            {
                return i;
            }
        }
        return -1;
    }

    // 添加控制点，坐标[0-1]
    public void AddControlPoint(Vector2 normlizedPos, bool bInit=false)
    {
        //Debug.Log("AddControlPoint "+normlizedPos);

        Vector2 pos = (transform as RectTransform).sizeDelta * normlizedPos;

        GameObject go = Instantiate(Prefab, transform) as GameObject;
        go.SetActive(true);
        (go.transform as RectTransform).anchoredPosition = pos;
        ControlPoint cp = go.GetComponent<ControlPoint>();
        cp.Manager = this;
        
        int i = 0;
        for (; i < m_ControlPoints.Count; i ++)
        {
            if ((m_ControlPoints[i].transform as RectTransform).anchoredPosition.x > pos.x)
            {
                break;
            }
        }
        m_ControlPoints.Insert(i,cp);

        if (bInit == false)
        {
            Hashtable ht = new Hashtable();
            ht["mgr"] = this;
            ht["idx"] = i;
            ht["pos"] = normlizedPos;
            NotificationCenter.DefaultCenter().PostNotification(NotificationType.VR_CONTROLPOINT_ADD, ht);

        }

        OnControlPointSelect(cp);
        CalculateControlPointPosition();
    }

    // 删除控制点
    private void DeleteControlPoint()
    {
        Debug.Log("DeleteControlPoint");

        if (m_Selected == null) return;
        if (m_ControlPoints.Count <= 2) return;

        int idx = FindControlPoint(m_Selected);

        if (idx != -1)
        {
            Destroy(m_Selected.gameObject);
            m_ControlPoints.RemoveAt(idx);

            Hashtable ht = new Hashtable();
            ht["mgr"] = this;
            ht["idx"] = idx;
            NotificationCenter.DefaultCenter().PostNotification(NotificationType.VR_CONTROLPOINT_DELETE, ht);

            // 自动选择其他控制点
            OnControlPointSelect(m_ControlPoints[Mathf.Max(0, idx - 1)]);
        }

    }

    // 设置控制点坐标
    public void SetControlPointPosition(int idx, Vector2 normlizedPos)
    {
        //Debug.Log("SetControlPointPosition "+normlizedPos);

        if (idx < 0 || idx > m_ControlPoints.Count-1) return;

        Vector2 pos = (transform as RectTransform).sizeDelta * normlizedPos;

        (m_ControlPoints[idx].transform as RectTransform).anchoredPosition = pos;
        
    }


    // 删除所有控制点
    public void ClearAllControlPoint()
    {
        for (int i = 0; i < m_ControlPoints.Count; i++)
        {
            Destroy(m_ControlPoints[i].gameObject);
        }
        m_ControlPoints.Clear();
    }


    // 选中控制点
    public void OnControlPointSelectByIdx(int idx)
    {
        if (idx < 0 || idx > m_ControlPoints.Count - 1) return;

        OnControlPointSelect(m_ControlPoints[idx]);
    }

    // 选中控制点
    public void OnControlPointSelect(ControlPoint controlPoint)
    {
        //Debug.Log("OnControlPointSelect");
        
        if (m_Selected != null && m_Selected != controlPoint)
        {
            m_Selected.SetSelected(false);
        }

        m_Selected = controlPoint;
        m_Selected.SetSelected(true);
        (m_Selected.transform as RectTransform).SetAsLastSibling();
        m_SelectedIdx = FindControlPoint(m_Selected);
        
        NotificationCenter.DefaultCenter().PostNotification(NotificationType.VR_CONTROLPOINT_SELECT, this);

        ActiveDelete = true;
        WaitUpdate = true;
    }

    // 约束控制点的坐标
    public void CalculateControlPointPosition()
    {
        if (m_Selected == null) return;

        float delta = 0.000001f;
        RectTransform rt = transform as RectTransform;

        float xMin = 0;
        float xMax = (transform as RectTransform).sizeDelta.x;
        if (m_SelectedIdx > 0) xMin = (m_ControlPoints[m_SelectedIdx - 1].transform as RectTransform).anchoredPosition.x+delta;
        if (m_SelectedIdx < m_ControlPoints.Count-1) xMax = (m_ControlPoints[m_SelectedIdx +1].transform as RectTransform).anchoredPosition.x-delta;

        RectTransform rectTransform = m_Selected.transform as RectTransform;
        Vector2 anchorPos = rectTransform.anchoredPosition;
        anchorPos.x = Mathf.Clamp(anchorPos.x, xMin, xMax);
        anchorPos.y = Mathf.Clamp(anchorPos.y, 0, rt.sizeDelta.y);
        if (LockY) anchorPos.y = rt.sizeDelta.y/2;
        rectTransform.anchoredPosition = anchorPos;
        
        Hashtable ht = new Hashtable();
        ht["mgr"] = this;
        ht["idx"] = m_SelectedIdx;
        ht["pos"] = anchorPos/ (transform as RectTransform).sizeDelta;
        NotificationCenter.DefaultCenter().PostNotification(NotificationType.VR_CONTROLPOINT_UPDATE, ht);
    }

}
