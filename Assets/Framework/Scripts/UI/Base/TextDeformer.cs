using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Framework
{

    public class TextDeformer : BaseMeshEffect
    {
        [SerializeField]
        public float Radius = 100f;
        [SerializeField]
        public float Space = 0.02f;

        public override void ModifyMesh(VertexHelper vh)
        {
            if (!this.IsActive()) return;

            // get vertexs
            List<UIVertex> verts = new List<UIVertex>();
            vh.GetUIVertexStream(verts);

            if (verts.Count == 0) return;

            /*
            for (int i = 0; i < verts.Count; i++)
            {
                UIVertex vertex = verts[i];
                Debug.LogFormat("vertext({0},{1},{2})", vertex.position.x, vertex.position.y, vertex.position.z);
            }
            */

            int count = verts.Count / 6;

            // 统计每一行字数(每个字宽度角度)
            List<List<float>> words = new List<List<float>>();
            {
                for (int i = 0; i < count; i++)
                {
                    // 是否换行符
                    UIVertex vertex = verts[i * 6];
                    UIVertex vertex2 = verts[i * 6 + 1];
                    if (vertex.position.x == vertex2.position.x)
                    {
                        words.Add(new List<float>());
                        continue;
                    }

                    if (words.Count == 0) words.Add(new List<float>());

                    int width = (int)Mathf.Abs(vertex.position.x - vertex2.position.x);
                    float angle = Mathf.Asin(width / 2 / Radius) * 2;
                    words[words.Count - 1].Add(angle);

                }
            }

            // 逐字调整xz坐标
            int widx = 0;
            for (int i = 0; i < words.Count; i++)
            {
                int wcount = words[i].Count;
                float angTotal = 0;
                for (int j = 0; j < wcount; j++)
                {
                    angTotal += words[i][j];
                }
                //Debug.LogFormat("word count {0} angle {1}", wcount, angTotal);

                // 第i行文字
                angTotal += Space * (wcount - 1);
                float angleBegin = -angTotal / 2f;
                for (int j = 0; j < wcount; j++)
                {
                    //Debug.Log("angleBegin " + angleBegin);
                    float angle = words[i][j];
                    for (int k = 0; k < 6; k++)
                    {
                        int vidx = widx * 6 + k;
                        UIVertex vertex = verts[vidx];
                        if (k == 1 || k == 2 || k == 3)
                        {
                            vertex.position.x = Radius * Mathf.Sin(angleBegin + angle);
                            vertex.position.z = -Radius * Mathf.Cos(angleBegin + angle);
                        }
                        else
                        {
                            vertex.position.x = Radius * Mathf.Sin(angleBegin);
                            vertex.position.z = -Radius * Mathf.Cos(angleBegin);
                        }
                        verts[vidx] = vertex;
                    }

                    angleBegin += words[i][j] + Space;
                    widx++;
                }

                // 换行符
                widx++;
            }

            // save vertex
            vh.Clear();
            vh.AddUIVertexTriangleStream(verts);
        }

    }

}