using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Framework;
using VolumeRender;

public class PanelDataset : BaseWindow
{
    private Transform SVContent;

    private VolumeDataset[] m_Datasets;

    private Image[] m_items;
    private int m_selectIdx = -1;

    void Awake()
    {
        SVContent = transform.Find("Scroll View/Viewport/Content");
        
        transform.Find("Button").GetComponent<Button>().onClick.AddListener(delegate () {
            GUIManager.Instance.CloseWindow(gameObject);
            NotificationCenter.DefaultCenter().PostNotification(NotificationType.VR_DATASET_SELECT, m_Datasets[m_selectIdx]);
        });
    }
    
    public override void Init(params object[] data)
    {
        m_Datasets = (VolumeDataset[])data;
        RefreshDatasets();
    }

    private void RefreshDatasets()
    {
        GameObject item = transform.Find("Item").gameObject;
        foreach (Transform child in SVContent)
        {
            Destroy(child.gameObject);
        }
        m_items = new Image[m_Datasets.Length];
        for (int i = 0; i < m_Datasets.Length; i++)
        {
            VolumeDataset dataset = m_Datasets[i];
            GameObject go = Instantiate(item, SVContent);
            go.SetActive(true);
            go.name = i.ToString();

            go.transform.Find("TextNumber").GetComponent<Text>().text = (i + 1).ToString();
            go.transform.Find("TextModality").GetComponent<Text>().text = dataset.modality;
            go.transform.Find("TextSize").GetComponent<Text>().text = string.Format("{0}X{1}X{2}", dataset.dimX, dataset.dimY, dataset.dimZ);

            Button btn = go.GetComponent<Button>();
            btn.onClick.AddListener(delegate () {
                OnBtnItem(go);
            });

            m_items[i] = go.GetComponent<Image>();
            if (i == 0) OnBtnItem(go);
        }
    }

    private void OnBtnItem(GameObject sender)
    {
        if (m_selectIdx != -1)
        {
            m_items[m_selectIdx].color = new Color32(100, 100, 100, 255);
        }

        int idx = int.Parse(sender.name);

        m_selectIdx = idx;
        m_items[m_selectIdx].color = new Color32(150, 150, 150, 255);
    }

    private void ClearDatasets()
    {
        Debug.Log("ClearDatasets");
        if (m_Datasets == null) return;

        for (int i = 0; i < m_Datasets.Length; i++)
        {
            VolumeDataset dataset = m_Datasets[i];
            m_Datasets[i] = null;
            if (dataset != null) dataset.Dispose();
        }
        m_Datasets = null;
    }
    
}
