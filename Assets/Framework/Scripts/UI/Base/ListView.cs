using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Framework
{
    [RequireComponent(typeof(ScrollRect))]
    public class ListView : MonoBehaviour
    {
        public delegate GameObject CreateListItemDelegate(int idx);
        public delegate void UpdateListItemDelegate(int idx, GameObject item);
        public CreateListItemDelegate createListItem = null;
        public UpdateListItemDelegate updateListItem = null;

        private ScrollRect scrollRect;
        private RectTransform RT_SVContent;

        private Vector2 m_SVSize;

        private Vector2 m_ItemSize;
        public Vector2 itemSize { get { return m_ItemSize; } set { m_ItemSize = value; } }

        private int m_ItemDim = 1;
        public int itemDim { get { return m_ItemDim; } set { m_ItemDim = value; } }

        private int m_ItemCount;
        public int itemCount { get { return m_ItemCount; } set { m_ItemCount = value; } }

        Dictionary<int, GameObject> m_SVItems = new Dictionary<int, GameObject>();
        public Dictionary<int, GameObject> Items { get { return m_SVItems; } }
        Stack<GameObject> cachedItems = new Stack<GameObject>();

        int preStartIdx = -1;
        int preEndIdx = -1;

        bool bInit = false;

        void Init()
        {
            if (bInit) return;
            bInit = true;

            scrollRect = GetComponent<ScrollRect>();

            m_SVSize = (scrollRect.transform as RectTransform).sizeDelta;
            scrollRect.onValueChanged.AddListener(OnListViewChange);

            RT_SVContent = scrollRect.transform.Find("Viewport/Content") as RectTransform;
        }

        void OnDestroy()
        {
            Clear(true);
        }

        public void Clear(bool bRemoveCached = false)
        {
            if (bInit == false) Init();

            scrollRect.StopMovement();

            // remove old
            foreach (Transform tf in RT_SVContent.transform)
            {
                if (tf != RT_SVContent) Destroy(tf.gameObject);
            }
            m_SVItems.Clear();

            preStartIdx = -1;
            preEndIdx = -1;

            if (bRemoveCached)
            {
                while (cachedItems.Count > 0)
                {
                    Destroy(cachedItems.Pop());
                }
                cachedItems.Clear();
            }
        }

        public void InitView()
        {
            if (bInit == false) Init();

            RT_SVContent.sizeDelta = new Vector2(0, itemSize.y * Mathf.CeilToInt((float)m_ItemCount / m_ItemDim));
            RT_SVContent.anchoredPosition = new Vector3(0, 0, 0);

            OnListViewChange(new Vector2(0, scrollRect.verticalNormalizedPosition));
        }

        public void RefreshView()
        {
            if (bInit == false) Init();

            preStartIdx = -1;
            preEndIdx = -1;

            foreach (int idx in m_SVItems.Keys)
            {
                AddObjectToCached(m_SVItems[idx]);
            }
            m_SVItems.Clear();

            OnListViewChange(new Vector2(0, scrollRect.verticalNormalizedPosition));
        }

        public GameObject GetItem(int idx)
        {
            if (m_SVItems.ContainsKey(idx))
            {
                return m_SVItems[idx];
            }
            else
            {
                return null;
            }
        }

        // listview change
        void OnListViewChange(Vector2 offset)
        {
            offset.x = Mathf.Clamp(offset.x, 0, 1);
            offset.y = Mathf.Clamp(offset.y, 0, 1);
            //offset.x = (RT_SVContent.sizeDelta.x - sv_size.x) * (1 - offset.x);
            offset.y = (RT_SVContent.sizeDelta.y - m_SVSize.y) * (1 - offset.y);
            int startidx = (int)(offset.y / m_ItemSize.y);
            int endidx = (int)((offset.y + m_SVSize.y) / m_ItemSize.y);
            startidx = Mathf.Max(startidx, 0);
            endidx = Mathf.Min(endidx, Mathf.CeilToInt((float)m_ItemCount / m_ItemDim) - 1);
            if (preStartIdx == startidx && preEndIdx == endidx)
            {
                return;
            }
            preStartIdx = startidx;
            preEndIdx = endidx;
            //Debug.LogFormat("OnListViewChange idx {0} {1}", startidx, endidx);

            int _startidx = startidx * m_ItemDim;
            int _endidx = Mathf.Min(m_ItemCount - 1, (endidx + 1) * m_ItemDim - 1);
            List<int> todeleted = new List<int>();
            foreach (int idx in m_SVItems.Keys)
            {
                if (idx < _startidx || idx > _endidx)
                {
                    AddObjectToCached(m_SVItems[idx]);
                    todeleted.Add(idx);
                }
            }
            for (int i = 0; i < todeleted.Count; i++)
            {
                m_SVItems.Remove(todeleted[i]);
            }

            for (int i = _startidx; i <= _endidx; i++)
            {
                if (m_SVItems.ContainsKey(i) == false)
                {
                    GameObject item = PopObjectFromCached();
                    if (item == null && createListItem != null)
                    {
                        item = createListItem(i);
                    }
                    else if (updateListItem != null)
                    {
                        updateListItem(i, item);
                    }
                    if (item != null) m_SVItems.Add(i, item);
                }
            }
        }

        void AddObjectToCached(GameObject go)
        {
            go.SetActive(false);
            go.transform.SetParent(null);
            cachedItems.Push(go);
        }

        GameObject PopObjectFromCached()
        {
            if (cachedItems.Count > 0)
            {
                return cachedItems.Pop();
            }
            return null;
        }

    }
}