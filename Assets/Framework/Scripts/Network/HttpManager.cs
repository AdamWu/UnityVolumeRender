using UnityEngine;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace Framework
{
	public class HttpManager : Singleton<HttpManager> 
	{
		public delegate void ResponseHandler(string data);
        public delegate void DownloadHandler(byte[] data);
        
        public void HttpGet(string host, string action, IDictionary args, ResponseHandler handler ) {

            string url = host;
            if (!string.IsNullOrEmpty(action)) {
                url = string.Format("{0}/{1}", host, action);
            }

			if (args != null) {
				int i = 0;
				foreach (DictionaryEntry e in args) {
					if (i == 0)
						url = url + "?";
					else
						url = url + "&";

					url = url + e.Key.ToString () + "=" + e.Value.ToString ();
					i++;
				}
			}

			//System.Uri.EscapeUriString(string.Format("http://{0}/{1}?{2}", GameConfig.HttpHost, action, _args));
		
			Debug.Log("HttpGet " + url);

			WWW www = new WWW(url);
			StartCoroutine(WaitForRequest(www, handler));
		}

		public void HttpPost(string host, string action, IDictionary args, ResponseHandler handler)
        {
            string url = host;
            if (!string.IsNullOrEmpty(action))
            {
                url = string.Format("{0}/{1}", host, action);
            }
            Debug.Log("HttpPost " + url);

            string post = "";
            if (args != null)
            {
                int i = 0;
                foreach (DictionaryEntry e in args)
                {
                    if (i != 0)
                        post = post + "&";

                    post = post + e.Key.ToString() + "=" + e.Value.ToString();
                    i++;
                }
            }

            Debug.Log("HttpPost post " + post);

            if (post != "")
            {
                byte[] data = System.Text.Encoding.UTF8.GetBytes(post);
                WWW www = new WWW(url, data);
                StartCoroutine(WaitForRequest(www, handler));
            } else
            {
                WWW www = new WWW(url);
                StartCoroutine(WaitForRequest(www, handler));
            }
		}

        public void HttpPost(string host, string action, byte[] data, ResponseHandler handler)
        {
            string url = host;
            if (!string.IsNullOrEmpty(action))
            {
                url = string.Format("{0}/{1}", host, action);
            }
            Debug.Log("HttpPost " + url);

            if (data != null)
            {
                WWW www = new WWW(url, data);
                StartCoroutine(WaitForRequest(www, handler));
            }
            else
            {
                WWW www = new WWW(url);
                StartCoroutine(WaitForRequest(www, handler));
            }
        }

        private IEnumerator WaitForRequest(WWW www, ResponseHandler handler) {

			//GUIManager.Instance.ShowWait();

			yield return www;

			//GUIManager.GetInstance().HideWait();

			if (www.error == null)
			{
				if (www.bytes != null && www.bytes.Length > 0)
				{
					string data = Encoding.UTF8.GetString(www.bytes);
					handler(data);
				}
			}
			else
			{
				Debug.LogError(www.error);
                //GUIManager.GetInstance().ShowAlert(LanguageManager.GetInstance().GetValue("TITLE_ERR"), www.error);
                handler(null);
            }
            www.Dispose();
            www = null;
		}

        public void HttpDownload(string host, DownloadHandler handler)
        {
            string url = host;

            Debug.Log("HttpDownload " + url);
            
            WWW www = new WWW(url);
            StartCoroutine(WaitForDownload(www, handler));
        }

        private IEnumerator WaitForDownload(WWW www, DownloadHandler handler)
        {
            //GUIManager.Instance.ShowWait();

            yield return www;

            //GUIManager.GetInstance().HideWait();

            if (www.error == null)
            {
                if (www.bytes != null)
                {
                    handler(www.bytes);
                }
            }
            else
            {
                Debug.LogError(www.error);
                //GUIManager.GetInstance().ShowAlert(LanguageManager.GetInstance().GetValue("TITLE_ERR"), www.error);
                handler(null);
            }
            www.Dispose();
            www = null;
        }
    }
}
