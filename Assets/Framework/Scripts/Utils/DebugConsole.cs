using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DebugConsole : MonoBehaviour {

	public bool LogEnable = true;

    public static DebugConsole instance;

	public class Log {
		public string msg;
		public string stacktrace;
		public LogType type;
		public Log(string msg, string stacktrace, LogType type) {
			this.msg = msg;
			this.stacktrace = stacktrace;
			this.type = type;
		}
	}
	static readonly Dictionary<LogType, Color> logTypeColors = new Dictionary<LogType, Color>  
        {  
            { LogType.Assert, Color.white },  
            { LogType.Error, Color.red },  
            { LogType.Exception, Color.red },  
            { LogType.Log, Color.white },  
            { LogType.Warning, Color.yellow },  
        };  

	private double 		lastInterval = 0.0;
	private int 		frames = 0;
	private float 		m_fps;
	private float 		m_accumTime = 0;
	private float 		m_fpsUpdateInterval = 0.5f;

	private string 		strFPS;
	private string 		strMem;

	private List<Log> 	m_logs = new List<Log>();
	private Vector2 scrollPosition = Vector2.zero;
	private bool m_logEnabled = true;


	void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
	}

	void Start () {
		lastInterval = Time.realtimeSinceStartup;
		frames = 0;
	}

	void HandleLog(string msg, string stacktrace, LogType type) {
		
		if (!m_logEnabled) {
			return;
		}

		//if (type == LogType.Assert || type == LogType.Error || type == LogType.Exception) {
		m_logs.Add(new Log(msg, stacktrace, type));
		//}

		scrollPosition.y = float.MaxValue;
	}

	void OnEnable() {
		if (LogEnable) {
			Application.logMessageReceived += HandleLog;
		}
	}

	void OnDisable () {
		if (LogEnable) {
			Application.logMessageReceived -= HandleLog;
		}
	}

	// Update is called once per frame
	void Update () {
		++frames;
		
		float timeNow = Time.realtimeSinceStartup;
		if (timeNow - lastInterval > m_fpsUpdateInterval) {
			float fps =  frames /(float) (timeNow - lastInterval);
			float ms = 1000.0f / Mathf.Max(fps, 0.0001f);
			strFPS = string.Format("{0} ms {1}FPS", ms.ToString("f1"), fps.ToString("f2"));
			frames = 0;
			lastInterval = timeNow;
		}

		// system info
		if (Time.frameCount % 30 == 0) {
			strMem = string.Format("memory : {0} MB", System.GC.GetTotalMemory(false) / (1024*1024));
		}
	}


	void OnGUI()  { 
        
		GUI.contentColor = Color.green;
		GUI.skin.label.fontSize = 16;
		GUI.skin.button.fontSize = 16;
		GUI.skin.button.margin = new RectOffset (20, 10, 10, 10);

		GUILayout.Label (strFPS);
		//GUILayout.Label (strMem);

		if (LogEnable) {
			
			scrollPosition = GUILayout.BeginScrollView (scrollPosition, GUILayout.Width (Screen.width / 3), GUILayout.Height (Screen.height - 200));

			foreach (Log log in m_logs) { 
				GUI.contentColor = logTypeColors [log.type];
				GUILayout.Label (log.msg);
				if (log.type == LogType.Assert || log.type == LogType.Error || log.type == LogType.Exception) {
					GUILayout.Label (log.stacktrace);
				}
			}
			GUILayout.EndScrollView ();
			GUI.contentColor = Color.white;

			// toolbar
			GUILayout.BeginHorizontal ();
			if (GUILayout.Button ("clear", GUILayout.Width (100), GUILayout.Height (50))) {
				m_logs.Clear ();
			}
			if (m_logEnabled) {
				if (GUILayout.Button ("close", GUILayout.Width (100), GUILayout.Height (50))) {
					m_logEnabled = false;
				}
			} else {
				if (GUILayout.Button ("open", GUILayout.Width (100), GUILayout.Height (50))) {
					m_logEnabled = true;
				}
			}

			GUILayout.EndHorizontal ();
		}
	} 
}
