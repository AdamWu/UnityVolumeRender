using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Framework;
using VolumeRender;

public class PanelLoading : BaseWindow {

    private Image bar;

	public override void Init(params object[] data) {
        
	}

	void Start()
	{
        bar = transform.Find("bar").GetComponent<Image>();
        bar.fillAmount = 0.0f;

    }

    private void Update()
    {
        if (DatasetImporterBase.IsImporting)
        {
            bar.fillAmount = DatasetImporterBase.Progress;
        }
    }
}
