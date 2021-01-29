using UnityEngine;
using UnityEngine.UI;

using Framework;

public class PanelAlert : BaseWindow
{
    private void Awake()
    {
        transform.Find("ButtonClose").GetComponent<Button>().onClick.AddListener(delegate ()
        {
            GUIManager.Instance.CloseWindow(gameObject);
        });
    }
    
    public override void Init(params object[] data)
    {
        string title = data[0].ToString();
        string msg = data[1].ToString();

        Text textTitle = transform.Find("TextTitle").GetComponent<Text>();
        Text textMsg = transform.Find("TextMsg").GetComponent<Text>();

        textTitle.text = title;
        textMsg.text = msg;

    }

}
