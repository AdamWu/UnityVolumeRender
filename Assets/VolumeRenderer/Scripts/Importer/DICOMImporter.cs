using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;

using Dicom;
using Dicom.Imaging;
using Dicom.Imaging.Render;

namespace VolumeRender
{

    public class DICOMSliceFile
    {
        public int seriesNumber = 0;
        public int width = 0;
        public int height = 0;
        public string modality = "";
        public float location = 0;
        public float intercept = 0.0f;
        public float slope = 1.0f;
        public float thickness = 0.0000f;
        public float pixelSpace = 1.0f;
        public string patientPosition = "";
        public float[] patientOrientation;
        public int[] data = null;

    }

    public class DICOMImporter : DatasetImporterBase
    {
        private string directory;
        private bool recursive;

        Dictionary<int, List<DICOMSliceFile>> sliceFiles = new Dictionary<int, List<DICOMSliceFile>>();

        public DICOMImporter(string directory, bool recursive)
        {
            this.directory = directory;
            this.recursive = recursive;
        }

        public override VolumeDataset[] Import()
        {
            IEnumerable<string> fileCandidates = Directory.EnumerateFiles(directory, "*.*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

            Dictionary<int, List<DICOMSliceFile>> files = new Dictionary<int, List<DICOMSliceFile>>();
            foreach (string filePath in fileCandidates)
            {
                //Debug.Log("reading dcm file:" + filePath);

                DICOMSliceFile slice = LoadSliceFile(filePath);
                if (slice == null) continue;

                if (!files.ContainsKey(slice.seriesNumber))
                {
                    files.Add(slice.seriesNumber, new List<DICOMSliceFile>());
                }
                files[slice.seriesNumber].Add(slice);
            }

            if (files.Count < 1)
            {
                Debug.LogWarning("No data found! " + directory);
                return null;
            }

            // 排序
            foreach (int key in files.Keys)
            {
                files[key].Sort((DICOMSliceFile a, DICOMSliceFile b) => { return a.location.CompareTo(b.location); });
            }

            VolumeDataset[] datasets = new VolumeDataset[files.Count];
            int idx = 0;
            foreach (int key in files.Keys)
            {
                List<DICOMSliceFile> lst = files[key];
                VolumeDataset dataset = GenerateDataset(lst);
                datasets[idx++] = dataset;
            }

            return datasets;
        }


        public override IEnumerator ImportAsync(LoadDatasetAsyncDelegate callback)
        {
            IsImporting = true;

            yield return null;

            string[] fileCandidates = Directory.GetFiles(directory, "*.*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

            sliceFiles.Clear();
            for (int i = 0; i < fileCandidates.Length; i++)
            {
                string filePath = fileCandidates[i];
                //Debug.Log("import file " + filePath);

                DICOMSliceFile slice = LoadSliceFile(filePath);
                if (slice == null) continue;
                if (!sliceFiles.ContainsKey(slice.seriesNumber))
                {
                    sliceFiles.Add(slice.seriesNumber, new List<DICOMSliceFile>());
                }
                sliceFiles[slice.seriesNumber].Add(slice);

                Progress = 1.0f * i / fileCandidates.Length;
                
                yield return null;
            }

            VolumeDataset[] datasets = null;
            if (sliceFiles.Count > 0)
            {
                foreach (int key in sliceFiles.Keys)
                {
                    sliceFiles[key].Sort((DICOMSliceFile a, DICOMSliceFile b) => { return a.location.CompareTo(b.location); });
                }
                int idx = 0;
                datasets = new VolumeDataset[sliceFiles.Count];
                foreach (int key in sliceFiles.Keys)
                {
                    List<DICOMSliceFile> lst = sliceFiles[key];
                    VolumeDataset dataset = GenerateDataset(lst);
                    datasets[idx++] = dataset;
                }
            }
            else
            {
                Debug.LogWarning("No data found! " + directory);
            }

            Progress = 1f;
            IsImporting = false;

            if (callback != null) callback(datasets);
        }

        private void LoadSliceFile(object obj)
        {
            string filepath = obj as string;
            FileStream fs = File.OpenRead(filepath);
            DicomFile dcmFile = DicomFile.Open(fs);
            DICOMSliceFile slice = ParseSliceFile(dcmFile);
            fs.Close();
        }


        // 加载单个dcm文件
        public static DICOMSliceFile LoadSliceFile(string filepath)
        {
            Debug.Log("LoadSliceFile:" + filepath);

            var sw = System.Diagnostics.Stopwatch.StartNew();
            sw.Start();

            try
            {
                FileStream fs = File.OpenRead(filepath);
                DicomFile dcmFile = DicomFile.Open(fs);
                DICOMSliceFile slice = ParseSliceFile(dcmFile);
                fs.Close();

                sw.Stop();
                UnityEngine.Debug.LogFormat("LoadSliceFile using: {0} ms", sw.ElapsedMilliseconds);

                return slice;
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
                return null;
            }

        }

        public static DICOMSliceFile LoadSliceFile(Stream stream)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            sw.Start();

            DicomFile dcmFile = DicomFile.Open(stream);
            DICOMSliceFile slice = ParseSliceFile(dcmFile);

            sw.Stop();
            UnityEngine.Debug.LogFormat("LoadSliceFile using: {0} ms", sw.ElapsedMilliseconds);

            return slice;
        }

        private static DICOMSliceFile ParseSliceFile(DicomFile dcmFile)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            sw.Start();

            if (!dcmFile.Dataset.Contains(DicomTag.PixelData))
            {
                Debug.LogWarning("no pixel data exist!");
                return null;
            }

            DicomImage dcmImage = new DicomImage(dcmFile.Dataset);
            DICOMSliceFile slice = new DICOMSliceFile();
            slice.width = dcmImage.Width;
            slice.height = dcmImage.Height;
            dcmImage = null;
            if (dcmFile.Dataset.Contains(DicomTag.SeriesNumber))
            {
                slice.seriesNumber = dcmFile.Dataset.Get<int>(DicomTag.SeriesNumber);
            }

            if (dcmFile.Dataset.Contains(DicomTag.InstanceNumber))
            {
                slice.location = dcmFile.Dataset.Get<float>(DicomTag.InstanceNumber);
            }
            if (dcmFile.Dataset.Contains(DicomTag.SliceLocation))
            {
                slice.location = dcmFile.Dataset.Get<float>(DicomTag.SliceLocation);
                //Debug.Log("SliceLocation " + slice.location);
            }
            if (dcmFile.Dataset.Contains(DicomTag.RescaleSlope))
            {
                slice.slope = dcmFile.Dataset.Get<float>(DicomTag.RescaleSlope);
            }
            if (dcmFile.Dataset.Contains(DicomTag.RescaleIntercept))
            {
                slice.intercept = dcmFile.Dataset.Get<float>(DicomTag.RescaleIntercept);
            }
            if (dcmFile.Dataset.Contains(DicomTag.PixelSpacing))
            {
                slice.pixelSpace = dcmFile.Dataset.Get<float>(DicomTag.PixelSpacing);
            }
            // 检查模态(MRI/CT/CR/DR)
            if (dcmFile.Dataset.Contains(DicomTag.Modality))
            {
                slice.modality = dcmFile.Dataset.Get<string>(DicomTag.Modality);
                //Debug.Log("Modality " + slice.modality);
            }
            if (dcmFile.Dataset.Contains(DicomTag.PatientPosition))
            {
                string PatientPosition = dcmFile.Dataset.Get<string>(DicomTag.PatientPosition);
                //Debug.Log("PatientPosition " + PatientPosition);
                slice.patientPosition = PatientPosition;
            }
            if (dcmFile.Dataset.Contains(DicomTag.ImageOrientationPatient))
            {
                slice.patientOrientation = new float[6];
                for (int i = 0; i < 6; i++)
                {
                    float ImageOrientationPatient = dcmFile.Dataset.Get<float>(DicomTag.ImageOrientationPatient, i);
                    //Debug.Log("ImageOrientationPatient " + ImageOrientationPatient);
                    slice.patientOrientation[i] = ImageOrientationPatient;
                }
            }
            if (dcmFile.Dataset.Contains(DicomTag.ImagePositionPatient))
            {
                for (int i = 0; i < 3; i++)
                {
                    string ImagePositionPatient = dcmFile.Dataset.Get<string>(DicomTag.ImagePositionPatient, i);
                    //Debug.Log("ImagePositionPatient " + ImagePositionPatient);
                }
            }
            if (dcmFile.Dataset.Contains(DicomTag.SliceThickness))
            {
                try
                {
                    slice.thickness = dcmFile.Dataset.Get<float>(DicomTag.SliceThickness);
                }
                catch (Exception e)
                {
                    Debug.Log(e.ToString());
                }
                //Debug.Log("SliceThickness " + slice.thickness);
            }

            // CT值计算保存
            int n = slice.width * slice.height;
            slice.data = new int[n];
            DicomPixelData header = DicomPixelData.Create(dcmFile.Dataset);
            var pixelData = PixelDataFactory.Create(header, 0);
            pixelData.Render(null, slice.data);

            for (int i = 0; i < n; i++)
            {
                int value = slice.data[i];
                float hounsfieldValue = value * slice.slope + slice.intercept;

                slice.data[i] = (short)Mathf.Clamp(hounsfieldValue, -1024.0f, 3071.0f);
            }
            dcmFile = null;
            GC.Collect();

            sw.Stop();
            UnityEngine.Debug.LogFormat("ParseSliceFile using: {0} ms", sw.ElapsedMilliseconds);

            return slice;
        }

        public static VolumeDataset GenerateDataset(List<DICOMSliceFile> slices)
        {
            if (slices.Count == 0) return null;

            Debug.Log("GenerateDataset slices:" + slices.Count);

            var sw = System.Diagnostics.Stopwatch.StartNew();
            sw.Start();

            int width = slices[0].width;
            int height = slices[0].height;

            VolumeDataset dataset = new VolumeDataset(width, height, slices.Count);
            dataset.modality = slices[0].modality;
            dataset.patientPosition = slices[0].patientPosition;

            for (int iSlice = 0; iSlice < slices.Count; iSlice++)
            {
                DICOMSliceFile slice = slices[iSlice];

                int n = slice.width * slice.height;
                int start = iSlice * n;
                for (int i = 0; i < n; i++)
                {
                    int dataIndex = start + i;
                    dataset.SetData(dataIndex, (short)slice.data[i]);
                }
                slice.data = null;
                dataset.sizeX = slice.pixelSpace * width;
                dataset.sizeY = slice.pixelSpace * height;
                dataset.sizeZ += slice.thickness;
            }

            sw.Stop();
            Debug.LogFormat("GenerateDataset ok size:{0} {1} {2} using {3}ms", dataset.sizeX, dataset.sizeY, dataset.sizeZ, sw.ElapsedMilliseconds);

            return dataset;
        }

    }

}