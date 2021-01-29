using UnityEngine;
using System.Collections.Generic;

using Framework;
using VolumeRender;

public class ColorMapManager : Singleton<ColorMapManager>
{
    private List<KeyValuePair<float, float>> ListOpacity = new List<KeyValuePair<float, float>>();
    private List<KeyValuePair<float, Color>> ListColor = new List<KeyValuePair<float, Color>>();

    private void Awake()
    {
    }

    public int GetOpacityCount() { return ListOpacity.Count; }
    public int GetColorCount() { return ListColor.Count; }

    public KeyValuePair<float, float> GetOpacityByIdx(int idx)
    {
        return ListOpacity[idx];
    }
    public KeyValuePair<float, Color> GetColorByIdx(int idx)
    {
        return ListColor[idx];
    }
    public KeyValuePair<float, float> GetOpacityRangeByIdx(int idx)
    {
        float min=0f, max=1f;
        if (idx > 0) min = ListOpacity[idx - 1].Key;
        if (idx < ListOpacity.Count - 1) max = ListOpacity[idx + 1].Key;
        return new KeyValuePair<float, float>( min, max );
    }
    public KeyValuePair<float, float> GetColorRangeByIdx(int idx)
    {
        float min = 0f, max = 1f;
        if (idx > 0) min = ListColor[idx - 1].Key;
        if (idx < ListColor.Count - 1) max = ListColor[idx + 1].Key;
        return new KeyValuePair<float, float>(min, max);
    }

    public float GetOpacity(float value)
    {
        Debug.Assert(value >= 0 && value <= 1);
        int idx = 0;
        for (; idx < ListOpacity.Count; idx ++)
        {
            KeyValuePair<float, float> kvp = ListOpacity[idx];

            if (value < kvp.Key) break;
        }

        if (idx == 0 || idx == ListOpacity.Count - 1)
        {
            return ListOpacity[idx].Value;
        } else
        {
            KeyValuePair<float, float> kvp_left = ListOpacity[idx - 1];
            KeyValuePair<float, float> kvp_right = ListOpacity[idx];
            float r = (value - kvp_left.Key) / (kvp_right.Key - kvp_left.Key);
            return kvp_left.Value + r * (kvp_right.Value - kvp_left.Value);
        }

    }

    public Color GetColor(float value)
    {
        return Color.white;
    }

    public void AddOpacity(float value, float alpha)
    {
        ListOpacity.Add(new KeyValuePair<float, float>(value, alpha));
    }
    public void AddColor(float value, Color color)
    {
        ListColor.Add(new KeyValuePair<float, Color>(value, color));
    }

    public void InsertOpacity(int idx, float value, float alpha)
    {
        ListOpacity.Insert(idx, new KeyValuePair<float, float>(value, alpha));
    }
    public void InsertColor(int idx, float value, Color color)
    {
        ListColor.Insert(idx, new KeyValuePair<float, Color>(value, color));
    }

    public void SetOpacity(int idx, float value, float alpha)
    {
        ListOpacity[idx] = new KeyValuePair<float, float>(value, alpha);
    }
    public void SetColor(int idx, float value, Color color)
    {
        ListColor[idx] = new KeyValuePair<float, Color>(value, color);
    }

    public void RemoveOpacity(int idx)
    {
        ListOpacity.RemoveAt(idx);
    }
    public void RemoveColor(int idx)
    {
        ListColor.RemoveAt(idx);
    }

    // Ìî³äÊý¾Ý
    public void FillTransferFunction(TransferFunction tf)
    {
        for (int i = 0; i < ListOpacity.Count; i++)
        {
            tf.AddControlPoint(new TFAlphaControlPoint(ListOpacity[i].Key, ListOpacity[i].Value));
        }
        for (int i = 0; i < ListColor.Count; i++)
        {
            tf.AddControlPoint(new TFColourControlPoint(ListColor[i].Key, ListColor[i].Value));
        }
    }

    public void Clear()
    {
        ListOpacity.Clear();
        ListColor.Clear();
    }
    
    public Dictionary<string, object> ToJson(float min, float max)
    {
        string opacity = "";
        string color = "";
        for (int i = 0; i < ListOpacity.Count; i++)
        {
            KeyValuePair<float, float> kvp = ListOpacity[i];
            opacity += string.Format("{0} {1} ", min+kvp.Key*(max-min), kvp.Value);
        }
        for (int i = 0; i < ListColor.Count; i++)
        {
            KeyValuePair<float, Color> kvp = ListColor[i];
            color += string.Format("{0} {1} {2} {3} ", min+kvp.Key*(max-min), kvp.Value.r, kvp.Value.g, kvp.Value.b);
        }

        Dictionary<string, object> preset = new Dictionary<string, object>();
        preset.Add("opacity", opacity);
        preset.Add("color", color);

        return preset;
    }
}