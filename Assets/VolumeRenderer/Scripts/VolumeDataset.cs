using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace VolumeRender
{
    public class VolumeDataset
    {
        public int dimX, dimY, dimZ;
        public float sizeX, sizeY, sizeZ; // size (mm)
        public string modality;
        public string patientPosition; // 体位

        public short[] data = null;
        //private IntPtr data_ptr;
        private int minDataValue = int.MaxValue;
        private int maxDataValue = int.MinValue;
        private Texture3D texture = null;

        public VolumeDataset(int dimX, int dimY, int dimZ)
        {
            this.dimX = dimX;
            this.dimY = dimY;
            this.dimZ = dimZ;

            int n = this.dimX * this.dimY * this.dimZ;
            data = new short[n];
            //data_ptr = Marshal.AllocHGlobal(sizeof(short) * n);
        }

        public short GetData(int idx)
        {
            return data[idx];
            //IntPtr ptr = data_ptr + idx * sizeof(short);
            //return Marshal.PtrToStructure<short>(ptr);
        }

        public void SetData(int idx, short value)
        {
            data[idx] = value;
            //IntPtr ptr = data_ptr + idx * sizeof(short);
            //Marshal.StructureToPtr(value, ptr, false);
        }

        public Texture3D GetTexture()
        {
            if (texture == null)
            {
                texture = CreateTextureInternal();
            }
            return texture;
        }

        public int GetMinDataValue()
        {
            if (minDataValue == int.MaxValue)
                CalculateValueBounds();
            return minDataValue;
        }

        public int GetMaxDataValue()
        {
            if (maxDataValue == int.MinValue)
                CalculateValueBounds();
            return maxDataValue;
        }

        private void CalculateValueBounds()
        {
            minDataValue = int.MaxValue;
            maxDataValue = int.MinValue;
            int count = dimX * dimY * dimZ;
            for (int i = 0; i < count; i++)
            {
                int val = GetData(i);
                minDataValue = Math.Min(minDataValue, val);
                maxDataValue = Math.Max(maxDataValue, val);
            }
        }

        private Texture3D CreateTextureInternal()
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            sw.Start();

            int minValue = GetMinDataValue();
            int maxValue = GetMaxDataValue();
            int maxRange = maxValue - minValue;
            int n = dimX * dimY * dimZ;

#if !MEMORY_OPTIMIZE
            Texture3D texture = new Texture3D(dimX, dimY, dimZ, TextureFormat.R8, false);
            Color32[] cols = new Color32[n];
            for (int i = 0; i < n; i++)
            {
                float value = (GetData(i) - minValue) / (float)maxRange;
                byte r = (byte)(value * 255);
                cols[i] = new Color32(r, 0, 0, 0);
            }
            texture.SetPixels32(cols);
#else
            Texture3D texture = new Texture3D(dimX, dimY, dimZ, TextureFormat.RFloat, false);
            Color[] cols = new Color[n];
            for (int i = 0; i < n; i++)
            {
                float value = (GetData(i) - minValue) / (float)maxRange;
                cols[i] = new Color(value, 0, 0, 0);
            }
            texture.SetPixels(cols);
#endif
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.Apply(false, true);

            cols = null;
            GC.Collect();

            sw.Stop();
            UnityEngine.Debug.LogFormat("Create3DTexture using: {0} ms", sw.ElapsedMilliseconds);

            return texture;
        }

        public void Dispose()
        {
            Debug.Log("VolumeDataset Dispose");

            //Marshal.FreeHGlobal(data_ptr);
            data = null;
            GameObject.Destroy(texture);
            texture = null;
            GC.Collect();

        }
    }
}