
using System.Collections;
using UnityEngine;

namespace VolumeRender
{

    public delegate void LoadDatasetAsyncDelegate(VolumeDataset[] datasets);

    public abstract class DatasetImporterBase
    {
        public static bool IsImporting { get; protected set; }
        public static float Progress { get; protected set; }

        public abstract VolumeDataset[] Import();

        public abstract IEnumerator ImportAsync(LoadDatasetAsyncDelegate callback);
    }

}