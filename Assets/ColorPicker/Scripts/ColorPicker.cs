using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ColorPicker : MonoBehaviour
{
    public Color InitColor = Color.white;

    private Slider SpectrumSlider;

    private Image ColorBackground;
    private RectTransform ColorSelectorRT;
    private Image ColorSelectedImage;

    // rgba
    private Slider RSlider, GSlider, BSlider, ASlider;
    private Image RSliderBg, GSliderBg, BSliderBg, ASliderBg;
    private InputField RInputField, GInputField, BInputField, AInputField;

    // color texture
    Texture2D TextureBuffer;
    private int TextureSize = 256;
    private Color[] ColorData;
    private int ColorSelectedIdx;

    private bool lockRGBA = false;
    private bool lockRSlider, lockGSlider, lockBSlider, lockASlider;

    public Color GetColor() { return new Color(RSlider.value, GSlider.value, BSlider.value, ASlider.value); }

    // Use this for initialization
    void Start () {

        TextureBuffer = new Texture2D(TextureSize, TextureSize, TextureFormat.ARGB32, false);
        TextureBuffer.filterMode = FilterMode.Point;
        ColorData = new Color[TextureSize * TextureSize];

        // UI
        SpectrumSlider = transform.Find("SpectrumSlider").GetComponent<Slider>();
        SpectrumSlider.onValueChanged.AddListener(delegate(float value) {
            CreateHSBTexture(new HSBColor(SpectrumSlider.value, 1, 1, 1));
            UpdateSelectColorBySelector(ColorSelectedIdx);
        });
        ColorBackground = transform.Find("ColorBackground").GetComponent<Image>();
        ColorBackground.GetComponent<UIEvent>().OnPointerDownCallback = CalculateColor;
        ColorBackground.GetComponent<UIEvent>().OnDragCallback = CalculateColor;
        ColorSelectorRT = ColorBackground.transform.Find("ColorSelector") as RectTransform;
        ColorSelectedImage = transform.Find("ColorSelected").GetComponent<Image>();

        // rgba
        RSlider = transform.Find("R/Slider").GetComponent<Slider>();
        GSlider = transform.Find("G/Slider").GetComponent<Slider>();
        BSlider = transform.Find("B/Slider").GetComponent<Slider>();
        ASlider = transform.Find("A/Slider").GetComponent<Slider>();
        RSlider.onValueChanged.AddListener(delegate (float value) {
            if (lockRSlider) return;
            RInputField.text = ((int)(value * 255)).ToString();
            UpdateSelectColorByRGBA();
        });
        GSlider.onValueChanged.AddListener(delegate (float value) {
            if (lockGSlider) return;
            GInputField.text = ((int)(value * 255)).ToString();
            UpdateSelectColorByRGBA();
        });
        BSlider.onValueChanged.AddListener(delegate (float value) {
            if (lockBSlider) return;
            BInputField.text = ((int)(value * 255)).ToString();
            UpdateSelectColorByRGBA();
        });
        ASlider.onValueChanged.AddListener(delegate (float value) {
            if (lockASlider) return;
            AInputField.text = ((int)(value * 255)).ToString();
            UpdateSelectColorByRGBA();
        });

        RInputField = transform.Find("R/InputField").GetComponent<InputField>();
        GInputField = transform.Find("G/InputField").GetComponent<InputField>();
        BInputField = transform.Find("B/InputField").GetComponent<InputField>();
        AInputField = transform.Find("A/InputField").GetComponent<InputField>();
        RInputField.onValueChanged.AddListener(delegate (string value) {
            lockRSlider = true;
            RSlider.value = int.Parse(value) / 255f;
            UpdateSelectColorByRGBA();
            lockRSlider = false;
        });
        GInputField.onValueChanged.AddListener(delegate (string value) {
            lockGSlider = true;
            GSlider.value = int.Parse(value) / 255f;
            UpdateSelectColorByRGBA();
            lockGSlider = false;
        });
        BInputField.onValueChanged.AddListener(delegate (string value) {
            lockBSlider = true;
            BSlider.value = int.Parse(value) / 255f;
            UpdateSelectColorByRGBA();
            lockBSlider = false;
        });
        AInputField.onValueChanged.AddListener(delegate (string value) {
            lockASlider = true;
            ASlider.value = int.Parse(value) / 255f;
            UpdateSelectColorByRGBA();
            lockASlider = false;
        });


        RSlider.value = InitColor.r;
        GSlider.value = InitColor.g;
        BSlider.value = InitColor.b;
        ASlider.value = InitColor.a;
        UpdateSelectColorByRGBA();

    }

    private void CalculateColor(PointerEventData eventData)
    {
        // 屏幕坐标换算局部坐标
        Vector2 localPos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(ColorBackground.transform as RectTransform, eventData.position, eventData.pressEventCamera, out localPos))
        {
            localPos.x = Mathf.Min(localPos.x, ColorBackground.rectTransform.sizeDelta.x / 2);
            localPos.x = Mathf.Max(localPos.x, -ColorBackground.rectTransform.sizeDelta.x / 2);
            localPos.y = Mathf.Min(localPos.y, ColorBackground.rectTransform.sizeDelta.y / 2);
            localPos.y = Mathf.Max(localPos.y, -ColorBackground.rectTransform.sizeDelta.y / 2);

            ColorSelectorRT.anchoredPosition = localPos;

            //get color data
            int x = (int)(localPos.x / ColorBackground.rectTransform.sizeDelta.x * TextureSize) + TextureSize / 2;
            int y = (int)(localPos.y / ColorBackground.rectTransform.sizeDelta.y * TextureSize) + TextureSize / 2;

            x = Mathf.Min(x, TextureSize-1);
            x = Mathf.Max(x, 0);
            y = Mathf.Min(y, TextureSize - 1);
            y = Mathf.Max(y, 0);

            UpdateSelectColorBySelector(y * TextureSize + x);
        }
    }

    // 通过选择器选择颜色
    private void UpdateSelectColorBySelector(int idx)
    {
        ColorSelectedIdx = idx;

        Color color = ColorData[idx];

        lockRGBA = true;

        RSlider.value = color.r;
        GSlider.value = color.g;
        BSlider.value = color.b;
        
        RInputField.text = ((int)(color.r * 255)).ToString();
        GInputField.text = ((int)(color.g * 255)).ToString();
        BInputField.text = ((int)(color.b * 255)).ToString();
        
        ColorSelectedImage.color = color;
        
        lockRGBA = false;
    }

    // 通过修改rgb选择颜色
    private void UpdateSelectColorByRGBA()
    {
        if (lockRGBA) return;

        Color color = new Color(RSlider.value, GSlider.value, BSlider.value, ASlider.value);

        HSBColor hsb = HSBColor.FromColor(color);

        // new selector position
        int x = (int)(hsb.s * TextureSize);
        int y = (int)(hsb.b * TextureSize);
        x = Mathf.Min(x, TextureSize - 1);
        x = Mathf.Max(x, 0);
        y = Mathf.Min(y, TextureSize - 1);
        y = Mathf.Max(y, 0);
        ColorSelectedIdx = y * TextureSize + x;

        SpectrumSlider.value = hsb.h;

        float posx = hsb.s * ColorBackground.rectTransform.sizeDelta.x - ColorBackground.rectTransform.sizeDelta.x/2;
        float posy = hsb.b * ColorBackground.rectTransform.sizeDelta.y - ColorBackground.rectTransform.sizeDelta.y/2;

        ColorSelectorRT.anchoredPosition = new Vector3(posx, posy);
    }

    //Generates a 256x256 texture with all variations for the selected HUE
    void CreateHSBTexture(HSBColor color)
    {
        //create this texture.
        HSBColor temp = new HSBColor(color.ToColor());
        temp.s = 0;
        temp.b = 1;

        for (int x = 0; x < TextureSize; x++)
        {
            for (int y = 0; y < TextureSize; y++)
            {
                temp.s = Mathf.Clamp(x / (float)(TextureSize - 1), 0, 1);
                temp.b = Mathf.Clamp(y / (float)(TextureSize - 1), 0, 1);
                ColorData[x + y * TextureSize] = temp.ToColor();
            }
        }

        TextureBuffer.SetPixels(ColorData);
        TextureBuffer.Apply();

        ColorBackground.sprite = Sprite.Create(TextureBuffer, new Rect(0, 0, TextureSize, TextureSize), new Vector2(0f, 1f), TextureSize);
    }
}
