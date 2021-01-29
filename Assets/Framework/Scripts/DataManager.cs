using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace Framework
{
    public class DataManager : Singleton<DataManager>
    {
        public Mode m_Mode
        {
            set;
            get;
        }
        /// <summary>
        /// 学生id
        /// </summary>
		private string stuid;
        public string StuId
        {
            set
            {
                stuid = value;
            }
            get
            {
                return stuid == null ? "1" : stuid;
            }
        }

        /// <summary>
        /// 开始试验时间
        /// </summary>
        public string StartTime
        {
            set;
            get;
        }
        /// <summary>
        /// 结束试验时间
        /// </summary>
        public string EndTime
        {
            set;
            get;
        }
        /// <summary>
        /// 成绩
        /// </summary>
		private int allscore = 0;
        public int AllScore
        {
            set
            {
                allscore = value;
            }
            get
            {
                return allscore;
            }
        }
        /// <summary>
        /// 场景名
        /// </summary>
        /// <value>The name of the scene.</value>
        public string SceneName
        {
            set;
            get;
        }
        public string Key
        {
            set;
            get;
        }
        /// <summary>
        /// 输出文件路径
        /// </summary>
        private string outFilePath;//=Application.streamingAssetsPath+"/outfile.json";
        public string OutFilePath
        {
            set
            {
                outFilePath = value;

            }
            get
            {
                return outFilePath == null ? Application.streamingAssetsPath + "/outfile.json" : outFilePath;
            }
        }

        private Dictionary<string, object> values = new Dictionary<string, object>();
        public void SetValue(string key, object value)
        {
            values[key] = value;
        }
        public object GetValue(string key)
        {
            if (values.ContainsKey(key))
            {
                return values[key];
            }
            return null;
        }

        public void ParseCommandArgs(bool bTokenValide = false)
        {
            Debug.Log("ParseArgs");

            string[] args = System.Environment.GetCommandLineArgs();
            foreach (string item in args)
            {
                string[] values = item.Split('=');
                switch (values[0])
                {
                    case "scenename":
                        SceneName = item.Replace("scenename=", "");
                        break;
                    case "id":
                        StuId = item.Replace("id=", "");
                        break;
                    case "outfile":
                        OutFilePath = item.Replace("outfile=", "");
                        break;
                    case "score":
                        AllScore = int.Parse(item.Replace("score=", ""));
                        break;
                    case "token":
                        Key = item.Replace("token=", "");
                        break;
                }
            }
            if (AllScore == 0)
            {
                m_Mode = Mode.Practice;
                AllScore = 100;
            }
            else
            {
                m_Mode = Mode.Exam;
            }

            if (bTokenValide && Key != "ly4f29W2zFcv7rMpXTaoGA==")
            {
                Application.Quit();
            }
        }

        public string GetResultToJson(int score, string answer, List<string> logs = null, List<string> errors = null)
        {

            Dictionary<string, object> dic = new Dictionary<string, object>();
            dic.Add("score", score);
            dic.Add("answer", answer);
            dic.Add("log", new List<string>());
            dic.Add("error", new List<string>());

            if (logs != null)
            {
                List<string> list = dic["log"] as List<string>;
                for (int i = 0; i < logs.Count; i++)
                {
                    list.Add(logs[i]);
                }
            }
            if (errors != null)
            {
                List<string> list = dic["error"] as List<string>;
                for (int i = 0; i < errors.Count; i++)
                {
                    list.Add(errors[i]);
                }
            }

            return MiniJSON.Json.Serialize(dic);
        }

        /// <summary>
        /// 写文件
        /// </summary>
        /// <param name="file">File.</param>
        /// <param name="content">Content.</param>
        public void WriteFile(string file, string content)
        {
            Debug.Log("读写文件=========>" + file);
            StreamWriter sw;
            if (!File.Exists(file))
            {
                sw = File.CreateText(file);//创建一个用于写入 UTF-8 编码的文本  
                Debug.Log("文件创建成功！");
            }
            else
            {
                sw = new StreamWriter(file, false);//File.AppendText(file);//打开现有 UTF-8 编码文本文件以进行读取  
            }
            sw.WriteLine(content);//以行为单位写入字符串  
            sw.Close();
            sw.Dispose();//文件流释放
        }

        /// <summary>
        /// 将字符串list
        /// </summary>
        /// <param name="lists">在list结尾加入一个标签</param>
        /// <returns></returns>
        public string GetResultToXml(string label, Dictionary<string, List<string>> lists)
        {
            string xmlString = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>";
            xmlString = xmlString + "<" + label + ">";
            foreach (string name in lists.Keys)
            {
                xmlString = xmlString + "<" + name + "s>";
                for (int i = 0; i < lists[name].Count; i++)
                {
                    xmlString = xmlString + "<" + name + ">" + lists[name][i] + "</" + name + ">";
                }
                xmlString = xmlString + "</" + name + "s>";
            }
            xmlString = xmlString + "</" + label + ">";
            return xmlString;
        }

        public void SendResultMsg(string url, Hashtable args, System.Action<string> action)
        {
            HttpManager.Instance.HttpGet(url, "", args, delegate (string data)
            {
                if (string.IsNullOrEmpty(data))
                {
                    Debug.Log("接收失败！！！");
                    if (action != null)
                        action(null);
                }
                else
                {
                    Debug.Log("data====>" + data);
                    Dictionary<string, object> dic = MiniJSON.Json.Deserialize(data) as Dictionary<string, object>;
                    if (dic["status"].ToString() != "0")
                    {
                        Debug.Log("发送成功");
                    }
                    if (action != null)
                        action(data);

                }
            });
        }

    }
    public enum Mode
    {
        Practice,
        Exam,
    }
}
