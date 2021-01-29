using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Framework
{
    public enum WindowLayer
    {
        Normal = 0,
        Top,
        Tip,
    }

    public enum WindowMode
    {
        None = 0,
        HideOther,
    }

    public class GUIManager : Singleton<GUIManager>
    {

        private class UIView
        {
            public string name;
            public RectTransform rt;
        }

        private RectTransform m_Root;
        public RectTransform Root
        {
            get
            {
                if (m_Root == null)
                {
                    Canvas[] gos = (Canvas[])GameObject.FindObjectsOfType(typeof(Canvas));
                    for (int i = 0; i < gos.Length; i++)
                    {
                        if (gos[i].transform.parent == null)
                        {
                            m_Root = gos[i].GetComponent<RectTransform>();
                        }
                    }
                }
                return m_Root;
            }
        }

        private Dictionary<string, WindowLayer> m_PanelLayerMap = new Dictionary<string, WindowLayer>();
        private Dictionary<string, WindowMode> m_PanelModeMap = new Dictionary<string, WindowMode>();
        private List<UIView> m_Stack = new List<UIView>();


        void Awake()
        {

            m_PanelLayerMap.Add("PanelAlert", WindowLayer.Top);
            m_PanelLayerMap.Add("PanelTip", WindowLayer.Tip);

        }

        public void SetWindowLayer(string window, WindowLayer layer)
        {
            if (m_PanelLayerMap.ContainsKey(window))
            {
                m_PanelLayerMap[window] = layer;
            }
            else
            {
                m_PanelLayerMap.Add(window, layer);
            }
        }
        public void SetWindowMode(string window, WindowMode mode)
        {
            if (m_PanelModeMap.ContainsKey(window))
            {
                m_PanelModeMap[window] = mode;
            }
            else
            {
                m_PanelModeMap.Add(window, mode);
            }
        }

        public float GetUIScale()
        {
            Vector3 scale = Root.localScale;
            return scale.x;
        }

        public bool ShowWindow(string window, params object[] data)
        {
            int index = m_Stack.FindIndex(x => x.name == window);
            UIView view = null;
            bool ret = false;
            if (index >= 0)
            {
                view = m_Stack[index];
                view.rt.SetAsLastSibling();
                if (view.rt.gameObject.activeInHierarchy == false)
                {
                    view.rt.gameObject.SetActive(true);
                }
                m_Stack.RemoveAt(index);

				SetLayerAndOrder (view);

				BaseWindow script = view.rt.GetComponent<BaseWindow> ();
				if (script) script.Init (data);
            }
            else
            {
				ResourceManager.Instance.LoadAssetAsync("ui/"+window, delegate(Object obj) {
					
					GameObject panel = Instantiate(obj as GameObject);
					panel.name = window;
					//if (lua == null) lua = panel.AddComponent<LuaBehaviour>();

					RectTransform rt = panel.GetComponent<RectTransform>();
					rt.SetParent(Root);

					rt.sizeDelta = new Vector2(0, 0);
					rt.localScale = Vector3.one;
					rt.localPosition = new Vector3(0, 0, 0);

					//view = new UIView { name = window, rt = rt };
					view = new UIView{name=window,rt=rt};

					SetLayerAndOrder (view);

					BaseWindow script = view.rt.GetComponent<BaseWindow> ();
					if (script) script.Init (data);
				});

                ret = true;
            }


            return ret;
        }

		void SetLayerAndOrder(UIView view) {
		
			string window = view.name;

			// set layer
			WindowLayer layer = WindowLayer.Normal;
			if (m_PanelLayerMap.ContainsKey(window)) layer = m_PanelLayerMap[window];
			int idx = 0;
			if (m_Stack.Count > 0)
			{
				for (; idx < m_Stack.Count; idx++)
				{
					WindowLayer layer2 = WindowLayer.Normal;
					if (m_PanelLayerMap.ContainsKey(m_Stack[idx].name)) layer2 = m_PanelLayerMap[m_Stack[idx].name];
					if (layer2 > layer)
					{
						break;
					}
				}
				if (idx == m_Stack.Count)
				{
					view.rt.SetAsLastSibling();
					m_Stack.Add(view);
				}
				else
				{
					int siblingIndex = m_Stack[idx].rt.GetSiblingIndex();
					for (int i = m_Stack.Count - 1; i >= idx; i--)
					{// move back
						m_Stack[i].rt.SetSiblingIndex(m_Stack[i].rt.GetSiblingIndex() + 1);
					}
					view.rt.SetSiblingIndex(siblingIndex);

					m_Stack.Insert(idx, view);
				}
			}
			else
			{
				view.rt.SetSiblingIndex(Root.childCount - 1);
				m_Stack.Add(view);
			}

			// set mode
			WindowMode mode = WindowMode.None;
			if (m_PanelModeMap.ContainsKey(window)) mode = m_PanelModeMap[window];
			if (mode == WindowMode.HideOther)
			{
				for (int i = idx - 1; i >= 0; i--)
				{
					view = m_Stack[i];
					WindowMode mode2 = WindowMode.None;
					if (m_PanelModeMap.ContainsKey(view.name)) mode2 = m_PanelModeMap[view.name];

					if (mode2 == WindowMode.HideOther)
					{
						// 隐藏后面的window
						view.rt.gameObject.SetActive(false);
						break;
					}
				}
			}
		}


        public void CloseWindow(string window)
        {
            int index = m_Stack.FindIndex(x => x.name == window);
            if (index >= 0)
            {
                WindowMode mode = WindowMode.None;
                if (m_PanelModeMap.ContainsKey(window))
                {
                    mode = m_PanelModeMap[window];
                }
                //LuaBehaviour script = m_Stack [index].script;
                Destroy(m_Stack[index].rt.gameObject);

                m_Stack.RemoveAt(index);
                //script.OnClose ();

                // 打开之前隐藏的window
                if (mode == WindowMode.HideOther)
                {
                    for (int i = index - 1; i >= 0; i--)
                    {
                        UIView view = m_Stack[i];
                        WindowMode mode2 = WindowMode.None;
                        if (m_PanelModeMap.ContainsKey(view.name)) mode2 = m_PanelModeMap[view.name];

                        if (mode2 == WindowMode.HideOther)
                        {
                            view.rt.gameObject.SetActive(true);
                            break;
                        }
                    }
                }
            }
        }

        public void HideWindow(string window)
        {
            int index = m_Stack.FindIndex(x => x.name == window);
            if (index >= 0)
            {
                WindowMode mode = WindowMode.None;
                if (m_PanelModeMap.ContainsKey(window))
                {
                    mode = m_PanelModeMap[window];
                }

                m_Stack[index].rt.gameObject.SetActive(false);

                // 打开之前隐藏的window
                if (mode == WindowMode.HideOther)
                {
                    for (int i = index - 1; i >= 0; i--)
                    {
                        UIView view = m_Stack[i];
                        WindowMode mode2 = WindowMode.None;
                        if (m_PanelModeMap.ContainsKey(view.name)) mode2 = m_PanelModeMap[view.name];

                        if (mode2 == WindowMode.HideOther)
                        {
                            view.rt.gameObject.SetActive(true);
                            break;
                        }
                    }
                }
            }
        }

		public void CloseWindow(Transform tf)
		{
            CloseWindow(tf.name);
		}
        public void CloseWindow(GameObject go)
        {
            CloseWindow(go.name);
        }

        public void HideWindow(Transform tf)
        {
            HideWindow(tf.name);
        }
        public void HideWindow(GameObject go)
        {
            HideWindow(go.name);
        }

        public bool IsWindowOpen(string window)
        {
            return m_Stack.Exists(x => x.name == window);
        }

        public void ShowTip(string msg)
        {
            ShowWindow("PanelTip", msg);
        }

        public void HideTip()
        {
            HideWindow("PanelTip");
        }

        public void Clear()
        {
            m_Stack.Clear();
        }
    }
}
