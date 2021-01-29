using UnityEngine;

namespace VolumeRender
{
    public class HistogramTextureGenerator
    {
        public static Texture2D GenerateHistogramTexture(VolumeDataset dataset, bool bLog = true)
        {
            if (dataset == null) return null;

            var sw = System.Diagnostics.Stopwatch.StartNew();
            sw.Start();

            int width = dataset.GetMaxDataValue() - dataset.GetMinDataValue() + 1;
            int height = 256;

            int[] values = new int[width];
            int maxFreq = 0;
            int count = dataset.dimX * dataset.dimY * dataset.dimZ;
            for (int sample = 0; sample < count; sample++)
            {
                int i = dataset.GetData(sample) - dataset.GetMinDataValue();
                values[i] += 1;
                maxFreq = System.Math.Max(values[i], maxFreq);
            }

            Debug.Log("GenerateHistogramTexture width:"+width);
            Debug.Log("GenerateHistogramTexture max:" + maxFreq);

            Color[] cols = new Color[width * height];
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBAFloat, false);
            for (int i = 0; i < width; i++)
            {
                int h = 0;
                if (bLog)
                    h = (int)(height * Mathf.Log10((float)values[i]) / Mathf.Log10((float)maxFreq));
                else
                    h = values[i] * height / maxFreq;

                h = Mathf.Clamp(h, 0, height - 1);
                for (int j = 0; j < h; j++)
                {
                    cols[i + j * width] = Color.white;
                }
                for (int j = h; j < height; j++)
                {
                    cols[i + j * width] = Color.clear;
                }
            }
            values = null;

            texture.wrapMode = TextureWrapMode.Clamp;
            texture.SetPixels(cols);
            texture.Apply();

            cols = null;

            sw.Stop();
            Debug.LogFormat("GenerateHistogramTexture using {0}ms", sw.ElapsedMilliseconds);

            return texture;
        }
    }
}