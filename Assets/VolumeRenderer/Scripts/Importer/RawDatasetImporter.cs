using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace VolumeRender
{
    public enum DataContentFormat
    {
        Int8,
        Uint8,
        Int16,
        Uint16,
        Int32,
        Uint32
    }

    public class RawDatasetImporter : DatasetImporterBase
    {
        string filePath;
        private DataContentFormat contentFormat;

        public RawDatasetImporter(string filePath, DataContentFormat contentFormat)
        {
            this.filePath = filePath;
            this.contentFormat = contentFormat;
        }

        public override VolumeDataset[] Import()
        {
            FileStream fs = new FileStream(filePath, FileMode.Open);
            BinaryReader reader = new BinaryReader(fs);

            int dimX = reader.ReadUInt16();
            int dimY = reader.ReadUInt16();
            int dimZ = reader.ReadUInt16();

            VolumeDataset dataset = new VolumeDataset(dimX, dimY, dimZ);

            int count = dataset.dimX * dataset.dimY * dataset.dimZ;

            int minVal = int.MaxValue;
            int maxVal = int.MinValue;
            int val = 0;
            for (int i = 0; i < count; i++)
            {
                switch (contentFormat)
                {
                    case DataContentFormat.Int8:
                        val = (int)reader.ReadByte();
                        break;
                    case DataContentFormat.Int16:
                        val = (int)reader.ReadInt16();
                        break;
                    case DataContentFormat.Int32:
                        val = (int)reader.ReadInt32();
                        break;
                    case DataContentFormat.Uint8:
                        val = (int)reader.ReadByte();
                        break;
                    case DataContentFormat.Uint16:
                        val = (int)reader.ReadUInt16();
                        break;
                    case DataContentFormat.Uint32:
                        val = (int)reader.ReadUInt32();
                        break;
                }
                minVal = Math.Min(minVal, val);
                maxVal = Math.Max(maxVal, val);
                dataset.SetData(i, (short)val);
            }
            Debug.Log("Loaded dataset in range: " + minVal + "  -  " + maxVal);

            return new VolumeDataset[] { dataset };
        }


        public override IEnumerator ImportAsync(LoadDatasetAsyncDelegate callback)
        {
            yield return null;
        }
    }
}