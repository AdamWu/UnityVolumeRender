using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

using Framework;
using VolumeRender;

public class PanelRender : MonoBehaviour
{
    private Transform m_PanelMode;
    private Transform m_PanelIsoSurface;
    private Transform m_PanelColor;
    private Transform m_PanelMaterial;
    
    private VolumeDataset[] m_Datasets;
    private VolumeRenderer m_VolumeRender;

    private List<object> Presets;
    private int ColorMapIdx = 0;

    // mode
    Dropdown dropdown_mode;

    // 表面
    InputField input_iso;
    Slider slider_iso;
    Button btn_color;

    // 伪色彩
    Dropdown dropdown_color;
    private Image opacityMap, colorMap;
    private InputField inputFieldP, inputFieldX, inputFieldAlpha, inputFieldP2, inputFieldX2;
    private Image imageColor;
    private ControlPointManager CPM_Opacity, CPM_Color;
    private float CTMax, CTMin;
    private int OpacityIdx=-1, ColorIdx=-1;

    // material
    Toggle toggle_shade;
    InputField input_ambient, input_diffuse, input_specular, input_power;
    Slider slider_ambient, slider_diffuse, slider_specular, slider_power;

    private void Awake()
    {
        m_PanelMode = transform.Find("PanelMode");
        m_PanelIsoSurface = transform.Find("PanelIsoSurface");
        m_PanelColor = transform.Find("PanelColor");
        m_PanelMaterial = transform.Find("PanelMaterial");

        m_VolumeRender = GameObject.FindObjectOfType<VolumeRenderer>();

        NotificationCenter.DefaultCenter().AddListener(NotificationType.VR_DATASET_SELECT, OnDatasetSelect);
        NotificationCenter.DefaultCenter().AddListener(NotificationType.VR_CONTROLPOINT_SELECT, OnControlPointSelect);
        NotificationCenter.DefaultCenter().AddListener(NotificationType.VR_CONTROLPOINT_ADD, OnControlPointAdd);
        NotificationCenter.DefaultCenter().AddListener(NotificationType.VR_CONTROLPOINT_DELETE, OnControlPointDelete);
        NotificationCenter.DefaultCenter().AddListener(NotificationType.VR_CONTROLPOINT_UPDATE, OnControlPointUpdate);
        NotificationCenter.DefaultCenter().AddListener(NotificationType.VR_RENDER_REFRESH, OnRenderRefresh);
        NotificationCenter.DefaultCenter().AddListener(NotificationType.VR_COLORMAP_UPDATE, OnColorMapUpdate);

    }

    IEnumerator Start()
    {
        string filename =
#if !UNITY_EDITOR && UNITY_WEBGL
		Path.Combine (Application.streamingAssetsPath, "presets.json");
#elif !UNITY_EDITOR && UNITY_ANDROID
		Path.Combine (Application.streamingAssetsPath , "presets.json");
#else
        Path.Combine("file://" + Application.streamingAssetsPath, "presets.json");
#endif
        UnityWebRequest www = UnityWebRequest.Get(filename);
        yield return www.SendWebRequest();
        if (www.error != null)
        {
            Debug.LogWarning(www.error + " " + filename);
            www.Dispose();
        }
        
        Dictionary<string, object> cfg = MiniJSON.Json.Deserialize(www.downloadHandler.text) as Dictionary<string, object>;
        Presets = cfg["presets"] as List<object>;
        
        // 渲染设置
        dropdown_mode = m_PanelMode.Find("Dropdown").GetComponent<Dropdown>();
        List<string> options = new List<string> { "ISO Surface", "ColorMap"};
        dropdown_mode.ClearOptions();
        dropdown_mode.AddOptions(options);
        dropdown_mode.onValueChanged.AddListener(delegate (int value) {
            if (value == 0)
            {
                SetRenderMode(m_VolumeRender, VolumeRenderMode.IsoSurfaceRendering);
            } else {
                SetRenderMode(m_VolumeRender, VolumeRenderMode.DirectVolumeRendering);
            }
        });

        // iso surface 渲染模式
        input_iso = m_PanelIsoSurface.Find("InputField").GetComponent<InputField>();
        slider_iso = m_PanelIsoSurface.Find("Slider").GetComponent<Slider>();
        slider_iso.onValueChanged.AddListener(delegate (float value) {
            input_iso.text = value.ToString();
            m_VolumeRender.SetIsoSurfaceThreshold(value);
        });
        btn_color = m_PanelIsoSurface.Find("ButtonColor").GetComponent<Button>();
        btn_color.onClick.AddListener(delegate () {
            GUIManager.Instance.ShowWindow("PanelColorPicker", btn_color.image.color, (OnCompletePickColorCallback)delegate (Color color)
            {
                btn_color.image.color = color;
                m_VolumeRender.SetIsoSurfaceColor(color);
            });
        });

        // colormap 渲染模式
        opacityMap = m_PanelColor.Find("OpacityMap").GetComponent<Image>();
        colorMap = m_PanelColor.Find("ColorMap").GetComponent<Image>();
        CPM_Opacity = m_PanelColor.Find("OpacityMap").GetComponent<ControlPointManager>();
        CPM_Color = m_PanelColor.Find("ColorMap").GetComponent<ControlPointManager>();
        inputFieldP = m_PanelColor.Find("TextP/InputField").GetComponent<InputField>();
        inputFieldX = m_PanelColor.Find("TextX/InputField").GetComponent<InputField>();
        inputFieldAlpha = m_PanelColor.Find("TextA/InputField").GetComponent<InputField>();
        inputFieldP2 = m_PanelColor.Find("TextP2/InputField").GetComponent<InputField>();
        inputFieldX2 = m_PanelColor.Find("TextX2/InputField").GetComponent<InputField>();
        imageColor = m_PanelColor.Find("TextC/ButtonColor").GetComponent<Image>();
        dropdown_color = m_PanelColor.Find("Dropdown").GetComponent<Dropdown>();
        List<string> options_color = new List<string> {};
        for (int i = 0; i < Presets.Count; i++)
        {
            Dictionary<string, object> preset = Presets[i] as Dictionary<string, object>;
            options_color.Add(preset["name"].ToString());
        }
        dropdown_color.ClearOptions();
        dropdown_color.AddOptions(options_color);
        dropdown_color.onValueChanged.AddListener(delegate (int idx) {
            ColorMapIdx = idx;
            SetTransferFuntion(Presets[ColorMapIdx] as Dictionary<string, object>);
        });
        inputFieldP.onEndEdit.AddListener(delegate (string value) {
            Debug.Log("inputFieldP " + value);
            OpacityIdx = Mathf.Clamp(int.Parse(value), 0, ColorMapManager.Instance.GetOpacityCount() - 1);
            CPM_Opacity.OnControlPointSelectByIdx(OpacityIdx);
            RefreshColorMap();
        });
        inputFieldX.onEndEdit.AddListener(delegate (string value) {
            Debug.Log("inputFieldX " + value);
            KeyValuePair<float, float> range = ColorMapManager.Instance.GetOpacityRangeByIdx(OpacityIdx);
            float X = Mathf.Clamp((float.Parse(value)-CTMin) / (CTMax - CTMin), range.Key, range.Value);
            KeyValuePair<float, float> kvp = ColorMapManager.Instance.GetOpacityByIdx(OpacityIdx);
            CPM_Opacity.SetControlPointPosition(OpacityIdx, new Vector2(X, kvp.Value));
            ColorMapManager.Instance.SetOpacity(OpacityIdx, X, kvp.Value);
            RefreshColorMap();
        });
        inputFieldP2.onEndEdit.AddListener(delegate (string value) {
            Debug.Log("inputFieldP2 " + value);
            ColorIdx = Mathf.Clamp(int.Parse(value), 0, ColorMapManager.Instance.GetColorCount() - 1);
            CPM_Color.OnControlPointSelectByIdx(ColorIdx);
            RefreshColorMap();
        });
        inputFieldX2.onEndEdit.AddListener(delegate (string value) {
            Debug.Log("inputFieldX2 " + value);
            KeyValuePair<float, float> range = ColorMapManager.Instance.GetColorRangeByIdx(ColorIdx);
            float X = Mathf.Clamp((float.Parse(value) - CTMin) / (CTMax - CTMin), range.Key, range.Value);
            KeyValuePair<float, Color> kvp = ColorMapManager.Instance.GetColorByIdx(ColorIdx);
            CPM_Color.SetControlPointPosition(ColorIdx, new Vector2(X, 0.5f));
            ColorMapManager.Instance.SetColor(ColorIdx, X, kvp.Value);
            RefreshColorMap();
        });
        inputFieldAlpha.onEndEdit.AddListener(delegate (string value) {
            Debug.Log("inputFieldAlpha " + value);
            float alpha = Mathf.Clamp01(float.Parse(value));
            KeyValuePair<float, float> kvp = ColorMapManager.Instance.GetOpacityByIdx(OpacityIdx);
            ColorMapManager.Instance.SetOpacity(OpacityIdx, kvp.Key, alpha);
            CPM_Opacity.SetControlPointPosition(OpacityIdx, new Vector2(kvp.Key, alpha));
            RefreshColorMap();
        });
        imageColor.GetComponent<Button>().onClick.AddListener(delegate ()
        {
            GUIManager.Instance.ShowWindow("PanelColorPicker", ColorMapManager.Instance.GetColorByIdx(ColorIdx).Value, (OnCompletePickColorCallback)delegate (Color color)
            {
                imageColor.color = color;
                KeyValuePair<float, Color> kvp = ColorMapManager.Instance.GetColorByIdx(ColorIdx);
                ColorMapManager.Instance.SetColor(ColorIdx, kvp.Key, color);
                RefreshColorMap();
            });
        });
        m_PanelColor.Find("TextP/ButtonUp").GetComponent<Button>().onClick.AddListener(delegate ()
        {
            OpacityIdx = Mathf.Clamp(OpacityIdx+1, 0, ColorMapManager.Instance.GetOpacityCount() - 1);
            CPM_Opacity.OnControlPointSelectByIdx(OpacityIdx);
            RefreshColorMap();
        });
        m_PanelColor.Find("TextP/ButtonDown").GetComponent<Button>().onClick.AddListener(delegate ()
        {
            OpacityIdx = Mathf.Clamp(OpacityIdx - 1, 0, ColorMapManager.Instance.GetOpacityCount() - 1);
            CPM_Opacity.OnControlPointSelectByIdx(OpacityIdx);
            RefreshColorMap();
        });
        m_PanelColor.Find("TextP2/ButtonUp").GetComponent<Button>().onClick.AddListener(delegate ()
        {
            ColorIdx = Mathf.Clamp(ColorIdx + 1, 0, ColorMapManager.Instance.GetColorCount() - 1);
            CPM_Color.OnControlPointSelectByIdx(ColorIdx);
            RefreshColorMap();
        });
        m_PanelColor.Find("TextP2/ButtonDown").GetComponent<Button>().onClick.AddListener(delegate ()
        {
            ColorIdx = Mathf.Clamp(ColorIdx - 1, 0, ColorMapManager.Instance.GetColorCount() - 1);
            CPM_Color.OnControlPointSelectByIdx(ColorIdx);
            RefreshColorMap();
        });

        // material 编辑
        toggle_shade = m_PanelMaterial.Find("ToggleShade").GetComponent<Toggle>();
        toggle_shade.onValueChanged.AddListener(delegate (bool value)
        {
            m_VolumeRender.SetMaterialShade(value);
        });
        input_ambient = m_PanelMaterial.Find("TextAmbient/InputField").GetComponent<InputField>();
        input_diffuse = m_PanelMaterial.Find("TextDiffuse/InputField").GetComponent<InputField>();
        input_specular = m_PanelMaterial.Find("TextSpecular/InputField").GetComponent<InputField>();
        input_power = m_PanelMaterial.Find("TextPower/InputField").GetComponent<InputField>();
        slider_ambient = m_PanelMaterial.Find("TextAmbient/Slider").GetComponent<Slider>();
        slider_diffuse = m_PanelMaterial.Find("TextDiffuse/Slider").GetComponent<Slider>();
        slider_specular = m_PanelMaterial.Find("TextSpecular/Slider").GetComponent<Slider>();
        slider_power = m_PanelMaterial.Find("TextPower/Slider").GetComponent<Slider>();
        slider_ambient.onValueChanged.AddListener(delegate (float value) {
            m_VolumeRender.SetAmbient(value);
            input_ambient.text = value.ToString();
        });
        slider_diffuse.onValueChanged.AddListener(delegate (float value) {
            m_VolumeRender.SetDiffuse(value);
            input_diffuse.text = value.ToString();
        });
        slider_specular.onValueChanged.AddListener(delegate (float value) {
            m_VolumeRender.SetSpecular(value);
            input_specular.text = value.ToString();
        });
        slider_power.onValueChanged.AddListener(delegate (float value) {
            m_VolumeRender.SetSpecularPower(value);
            input_power.text = value.ToString();
        });

        Refresh();
    }
    
    private void SetRenderMode(VolumeRenderer vro, VolumeRenderMode mode)
    {
        if (vro == null) return;

        if (mode == VolumeRenderMode.IsoSurfaceRendering)
        {
            m_PanelColor.gameObject.SetActive(false);
            m_PanelIsoSurface.gameObject.SetActive(true);
        }
        else
        {
            m_PanelColor.gameObject.SetActive(true);
            m_PanelIsoSurface.gameObject.SetActive(false);
        }
        vro.SetRenderMode(mode);
    }

    private void SetTransferFuntion(Dictionary<string, object> preset)
    {
        string[] opacity_values = preset["opacity"].ToString().Trim().Split(' ');
        string[] color_values = preset["color"].ToString().Trim().Split(' ');

        ColorMapManager.Instance.Clear();
        OpacityIdx = -1;
        ColorIdx = -1;

        CPM_Opacity.ClearAllControlPoint();
        CPM_Color.ClearAllControlPoint();
        
        CTMin = Mathf.Min(float.Parse(opacity_values[0]),float.Parse(color_values[0]));
        CTMax = Mathf.Max(float.Parse(opacity_values[opacity_values.Length - 2]), float.Parse(color_values[color_values.Length - 4]));

        VolumeDataset dataset = m_VolumeRender.dataset;
        if (dataset != null)
        {
            CTMin = dataset.GetMinDataValue();
            CTMax = dataset.GetMaxDataValue();
        }
        
        for (int i = 0; i < opacity_values.Length; i += 2)
        {
            if (opacity_values[i].Trim().Length == 0) continue;

            float pos = float.Parse(opacity_values[i]) - CTMin;
            if (pos < 0) continue;
            float alpha = float.Parse(opacity_values[i + 1]);
            ColorMapManager.Instance.AddOpacity(pos / (CTMax-CTMin), alpha);
            CPM_Opacity.AddControlPoint(new Vector2(pos/ (CTMax - CTMin), alpha), true);
        }
        for (int i = 0; i < color_values.Length; i += 4)
        {
            if (color_values[i].Trim().Length == 0) continue;

            float pos = float.Parse(color_values[i]) - CTMin;
            if (pos < 0) continue;
            float r = float.Parse(color_values[i + 1]);
            float g = float.Parse(color_values[i + 2]);
            float b = float.Parse(color_values[i + 3]);
            ColorMapManager.Instance.AddColor(pos / (CTMax - CTMin), new Color(r, g, b, 1));
            CPM_Color.AddControlPoint(new Vector2(pos / (CTMax - CTMin), 0.5f),true);
        }
        
        RefreshColorMap();
    }


    void Refresh()
    {
        // mode 
        dropdown_mode.value = m_VolumeRender.GetRenderMode()== VolumeRenderMode.IsoSurfaceRendering?0:1;
        SetRenderMode(m_VolumeRender, m_VolumeRender.GetRenderMode());

        // 表面渲染
        slider_iso.value = m_VolumeRender.GetIsoSurfaceThreshold();
        btn_color.image.color = m_VolumeRender.GetIsoSurfaceColor();

        // colormap
        dropdown_color.value = ColorMapIdx;

        // material
        toggle_shade.isOn = m_VolumeRender.GetMaterialShade();
        input_ambient.text = m_VolumeRender.GetAmbient().ToString();
        input_diffuse.text = m_VolumeRender.GetDiffuse().ToString();
        input_specular.text = m_VolumeRender.GetSpecular().ToString();
        input_power.text = m_VolumeRender.GetSpecularPower().ToString();
        slider_ambient.value = m_VolumeRender.GetAmbient();
        slider_diffuse.value = m_VolumeRender.GetDiffuse();
        slider_specular.value = m_VolumeRender.GetSpecular();
        slider_power.value = m_VolumeRender.GetSpecularPower();
    }

    void RefreshColorMap()
    {
        //Debug.Log("RefreshColorMap");

        if (OpacityIdx != -1)
        {
            KeyValuePair<float, float> kvp = ColorMapManager.Instance.GetOpacityByIdx(OpacityIdx);
            inputFieldP.text = string.Format("{0}", OpacityIdx);
            inputFieldX.text = string.Format("{0:N2}", kvp.Key * (CTMax - CTMin) + CTMin);
            inputFieldAlpha.text = string.Format("{0:N2}", kvp.Value);
        }
        if (ColorIdx != -1)
        {
            KeyValuePair<float, Color> kvp2 = ColorMapManager.Instance.GetColorByIdx(ColorIdx);
            inputFieldP2.text = string.Format("{0}", ColorIdx);
            inputFieldX2.text = string.Format("{0:N2}", kvp2.Key * (CTMax - CTMin) + CTMin);
            imageColor.color = kvp2.Value;
        }

        TransferFunction tf = new TransferFunction();
        ColorMapManager.Instance.FillTransferFunction(tf);

        tf.GenerateTexture();
        Texture2D texture = tf.GetTexture();
        Texture2D textureNoAlpha = tf.GetTextureNoAlpha();
        opacityMap.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        colorMap.sprite = Sprite.Create(textureNoAlpha, new Rect(0, 0, textureNoAlpha.width, textureNoAlpha.height), new Vector2(0.5f, 0.5f));
        m_VolumeRender.SetTFTexture(texture);
    }
   

    void OnDatasetSelect(Notification notification)
    {
        VolumeDataset dataset = (VolumeDataset)notification.data;

        m_VolumeRender.gameObject.SetActive(true);
        m_VolumeRender.SetDataset(dataset);
        switch (dataset.patientPosition)
        {
            case "HFS":
                m_VolumeRender.transform.localEulerAngles = new Vector3(-90, 180, 0);
                break;
            case "HFP":
                m_VolumeRender.transform.localEulerAngles = new Vector3(-90, 0, 0);
                break;
            case "HFDL":
                m_VolumeRender.transform.localEulerAngles = new Vector3(-90, 0, 0);
                break;
            case "HFDR":
                m_VolumeRender.transform.localEulerAngles = new Vector3(-90, 0, 0);
                break;
            case "FFS":
                m_VolumeRender.transform.localEulerAngles = new Vector3(90, 0, 0);
                break;
            case "FFP":
                m_VolumeRender.transform.localEulerAngles = new Vector3(90, 0, 0);
                break;
            case "FFDL":
                m_VolumeRender.transform.localEulerAngles = new Vector3(90, 0, 0);
                break;
            case "FFDR":
                m_VolumeRender.transform.localEulerAngles = new Vector3(90, 0, 0);
                break;
        }

        // test
        SlicingPlane PlaneTransverse = m_VolumeRender.transform.Find("SlicingPlaneTransverse").GetComponent<SlicingPlane>();
        SlicingPlane PlaneCoronal = m_VolumeRender.transform.Find("SlicingPlaneCoronal").GetComponent<SlicingPlane>();
        SlicingPlane PlaneSagittal = m_VolumeRender.transform.Find("SlicingPlaneSagittal").GetComponent<SlicingPlane>();
        PlaneTransverse.SetDataset(dataset);
        PlaneCoronal.SetDataset(dataset);
        PlaneSagittal.SetDataset(dataset);
        if (dataset.patientPosition.Contains("D"))//DR DL
        {
            PlaneCoronal.transform.localEulerAngles = new Vector3(0, 90, 0);
            PlaneSagittal.transform.localEulerAngles = new Vector3(90, 0, 0);
        } else
        {
            PlaneSagittal.transform.localEulerAngles = new Vector3(0, 90, 0);
            PlaneCoronal.transform.localEulerAngles = new Vector3(90, 0, 0);
        }

        float max = Mathf.Max(dataset.sizeX, Mathf.Max(dataset.sizeY, dataset.sizeZ));
        float x = Mathf.Max(dataset.sizeX / max, 0.00001f);
        float y = Mathf.Max(dataset.sizeY / max, 0.00001f);
        float z = Mathf.Max(dataset.sizeZ / max, 0.00001f);
        m_VolumeRender.transform.localScale = new Vector3(x, y, z);

        SetTransferFuntion(Presets[ColorMapIdx] as Dictionary<string, object>);

    }


    void OnRenderRefresh(Notification notification)
    {
        Refresh();
    }

    void OnControlPointSelect(Notification notification)
    {
        ControlPointManager mgr = (ControlPointManager)notification.data;
        int idx = mgr.GetSelectedIdx();
        //Debug.LogFormat("OnControlPointSelect {0} {1}",mgr, idx);

        if (mgr == CPM_Opacity)
        {
            OpacityIdx = idx;
        } else if (mgr == CPM_Color) {
            ColorIdx = idx;
        }

        RefreshColorMap();
    }

    void OnControlPointAdd(Notification notification)
    {
        Hashtable ht = (Hashtable)notification.data;
        ControlPointManager mgr = (ControlPointManager)ht["mgr"];
        int idx = (int)ht["idx"];
        Vector2 pos = (Vector2)ht["pos"];

        Debug.Log("OnControlPointAdd " + idx);

        if (mgr == CPM_Opacity)
        {
            ColorMapManager.Instance.InsertOpacity(idx, pos.x, pos.y);
            
        }
        else if (mgr == CPM_Color)
        {
            ColorMapManager.Instance.InsertColor(idx, pos.x, Color.white);
        }

        RefreshColorMap();
    }

    void OnControlPointDelete(Notification notification)
    {
        Hashtable ht = (Hashtable)notification.data;
        ControlPointManager mgr = (ControlPointManager)ht["mgr"];
        int idx = (int)ht["idx"];

        Debug.Log("OnControlPointDelete " + idx);

        if (mgr == CPM_Opacity)
        {
            ColorMapManager.Instance.RemoveOpacity(idx);
            if (idx >= ColorMapManager.Instance.GetOpacityCount())
            {
                OpacityIdx = ColorMapManager.Instance.GetOpacityCount() - 1;
            }
        }
        else if (mgr == CPM_Color)
        {
            ColorMapManager.Instance.RemoveColor(idx);
            if (idx >= ColorMapManager.Instance.GetColorCount())
            {
                ColorIdx = ColorMapManager.Instance.GetColorCount() - 1;
            }
        }

        RefreshColorMap();
    }

    void OnControlPointUpdate(Notification notification)
    {
        Hashtable ht = (Hashtable)notification.data;
        ControlPointManager mgr = (ControlPointManager)ht["mgr"];
        int idx = (int)ht["idx"];
        Vector2 pos = (Vector2)ht["pos"];

        //Debug.Log("OnControlPointUpdate " + idx);

        if (mgr == CPM_Opacity)
        {
            ColorMapManager.Instance.SetOpacity(idx, pos.x, pos.y);
            OpacityIdx = idx;
        }
        else if (mgr == CPM_Color)
        {
            KeyValuePair<float, Color> kvp = ColorMapManager.Instance.GetColorByIdx(idx);
            ColorMapManager.Instance.SetColor(idx, pos.x, kvp.Value);
            ColorIdx = idx;
        }

        RefreshColorMap();
    }

    void OnColorMapUpdate(Notification notification)
    {
        Dictionary<string, object> data = (Dictionary<string, object>)notification.data;

        if (data == null) data = Presets[ColorMapIdx] as Dictionary<string, object>;

        SetTransferFuntion(data);
    }
}
