using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;

using Framework;

public delegate void OnCompletePickColorCallback(Color color);

public class PanelPickColor : BaseWindow {

    private Color initColor;
	private OnCompletePickColorCallback callback;

	public override void Init(params object[] data) {

        initColor = (Color)data[0];
        callback = data [1] as OnCompletePickColorCallback;

	}

	void Start()
	{
		ColorPicker colorpicker = transform.Find("ColorPicker").GetComponent<ColorPicker>();
		colorpicker.InitColor = initColor;

		Button btn_close = transform.Find("ButtonClose").GetComponent<Button>();
		btn_close.onClick.AddListener(delegate () {
			GUIManager.Instance.CloseWindow(name);
		});
        Button btn_ok = transform.Find("ButtonOK").GetComponent<Button>();
        btn_ok.onClick.AddListener(delegate () {
            GUIManager.Instance.CloseWindow(name);
            Color color = colorpicker.GetColor();
            if (callback != null)
            {
                callback(color);
            }
        });
    }

}
