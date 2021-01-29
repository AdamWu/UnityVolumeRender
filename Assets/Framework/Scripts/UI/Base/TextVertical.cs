using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace Framework
{
    public class TextVertical : BaseMeshEffect
    {

        public float lineSpacing = 1f;

        public override void ModifyMesh(VertexHelper vh)
        {
            if (!this.IsActive())
                return;

            // get vertexs
            List<UIVertex> verts = new List<UIVertex>();
            vh.GetUIVertexStream(verts);

            // test
            /*
            for (int i = 0; i < verts.Count; i ++) {
                UIVertex vertex = verts [i];
                Debug.LogFormat ("vertext({0},{1},{2})", vertex.position.x, vertex.position.y, vertex.position.z);
            }
            //*/

            Text text = GetComponent<Text>();
            RectTransform tf = GetComponent<RectTransform>();
            string str = text.text;
            float width = tf.sizeDelta.x;
            float height = tf.sizeDelta.y;
            float size = text.fontSize;
            float x = width / 2 - size / 2;
            float y = height / 2;
            TextAnchor anchor = text.alignment;
            bool bHorizontalCenter = false;
            bool bVerticalCenter = false;
            if (anchor == TextAnchor.MiddleCenter || anchor == TextAnchor.MiddleLeft || anchor == TextAnchor.MiddleRight)
            {
                bHorizontalCenter = true;
            }
            if (anchor == TextAnchor.MiddleCenter)
            {
                bVerticalCenter = true;
            }

            int count = verts.Count / 6;
            int lineStartIdx = 0;
            for (int i = 0; i < count; i++)
            {
                // 一个文字6个顶点
                float xmin = float.MaxValue, xmax = float.MinValue;
                float ymin = float.MaxValue, ymax = float.MinValue;
                for (int j = 0; j < 6; j++)
                {
                    UIVertex vertex = verts[i * 6 + j];
                    if (vertex.position.x < xmin) xmin = vertex.position.x;
                    if (vertex.position.x > xmax) xmax = vertex.position.x;
                    if (vertex.position.y < ymin) ymin = vertex.position.y;
                    if (vertex.position.y > ymax) ymax = vertex.position.y;
                }
                float xcenter = (xmin + xmax) / 2;
                float ycenter = (ymin + ymax) / 2;
                float h = ymax - ymin;
                //Debug.LogFormat ("height {0}", h);

                // next line?
                if (str.Substring(i, 1) == "\n")
                { // 换行符
                    if (bVerticalCenter) ModifyCenter(verts, lineStartIdx, i, y, height);
                    lineStartIdx = i;
                    y = height / 2;
                    x -= (size + lineSpacing);
                    continue;
                }
                else
                {
                    // no space
                    if (h <= 0) continue;
                }
                if (y - size < -height / 2)
                {
                    if (bVerticalCenter) ModifyCenter(verts, lineStartIdx, i, y, height);
                    lineStartIdx = i;
                    y = height / 2;
                    x -= (size + lineSpacing);
                }
                y -= size / 2;

                for (int j = 0; j < 6; j++)
                {
                    int idx = i * 6 + j;
                    UIVertex vertex = verts[idx];
                    vertex.position.x -= xcenter;
                    vertex.position.y -= ycenter;
                    vertex.position.x += x;
                    vertex.position.y += y;
                    verts[idx] = vertex;
                    //Debug.LogFormat ("vertext({0},{1},{2})", vertex.position.x, vertex.position.y, vertex.position.z);
                }
                y -= size / 2;
            }

            if (bVerticalCenter) ModifyCenter(verts, lineStartIdx, count, y, height);
            if (bHorizontalCenter)
            {
                float offsetx = (x + width / 2 - size / 2) / 2;
                for (int i = 0; i < count; i++)
                {
                    for (int j = 0; j < 6; j++)
                    {
                        int idx = i * 6 + j;
                        UIVertex vertex = verts[idx];
                        vertex.position.x -= offsetx;
                        verts[idx] = vertex;
                    }
                }
            }

            // save vertex
            vh.Clear();
            vh.AddUIVertexTriangleStream(verts);
        }

        void ModifyCenter(List<UIVertex> verts, int startidx, int endidx, float y, float height)
        {

            // 居中显示
            float offsety = (y + height / 2) / 2;
            for (int j = startidx; j < endidx; j++)
            {
                for (int k = 0; k < 6; k++)
                {
                    int idx = j * 6 + k;
                    UIVertex vertex = verts[idx];
                    vertex.position.y -= offsety;
                    verts[idx] = vertex;
                }
            }
        }
    }
}