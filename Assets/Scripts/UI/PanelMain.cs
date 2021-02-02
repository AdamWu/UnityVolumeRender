using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using Framework;
using VolumeRender;

public class PanelMain : MonoBehaviour
{
    private VolumeRenderer m_VolumeRender;

    private Transform Root;
    Button btnSlice;

    ModelShowController modelShowController;

    Transform PlaneTransverse, PlaneCoronal, PlaneSagittal;

    private bool isShowSlice = false;

    private void Awake()
    {
        Application.targetFrameRate = 60;

        Screen.SetResolution(1280, 720, false);

        m_VolumeRender = GameObject.FindObjectOfType<VolumeRenderer>();
        Root = m_VolumeRender.transform.parent;

        modelShowController = Root.GetComponent<ModelShowController>();
        
        PlaneTransverse = m_VolumeRender.transform.Find("SlicingPlaneTransverse");
        PlaneCoronal = m_VolumeRender.transform.Find("SlicingPlaneCoronal");
        PlaneSagittal = m_VolumeRender.transform.Find("SlicingPlaneSagittal");
        PlaneTransverse.gameObject.SetActive(false);
        PlaneCoronal.gameObject.SetActive(false);
        PlaneSagittal.gameObject.SetActive(false);
        
        btnSlice = transform.Find("ButtonSlice").GetComponent<Button>();
        btnSlice.onClick.AddListener(delegate ()
        {
            isShowSlice = !isShowSlice;
            RefreshButtonState();
        });


        NotificationCenter.DefaultCenter().AddListener(NotificationType.VR_DATASET_LOAD, OnDatasetLoad);
        NotificationCenter.DefaultCenter().AddListener(NotificationType.VR_DATASET_SELECT, OnDatasetSelect);
    }

    void Start()
    {
        m_VolumeRender.gameObject.SetActive(false);

        RefreshButtonState();

        transform.Find("ButtonImport").GetComponent<Button>().onClick.AddListener(delegate() 
        {
            OpenFileName openFileName = new OpenFileName();
            openFileName.structSize = Marshal.SizeOf(openFileName);
            openFileName.filter = "ALL文件(*.*)\0*.*";
            openFileName.file = new string(new char[256]);
            openFileName.maxFile = openFileName.file.Length;
            openFileName.fileTitle = new string(new char[64]);
            openFileName.maxFileTitle = openFileName.fileTitle.Length;
            openFileName.title = "选择文件";
            openFileName.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000008;
            if (WindowDll.GetOpenFileNameWindows(openFileName))
            {
                string filename = openFileName.file;
                string dir = Path.GetDirectoryName(filename);
                string ext = Path.GetExtension(filename).ToLower();

                Debug.LogFormat("LoadData dir:{0} ext:{1}", dir, ext);

                DatasetImporterBase importer = new DICOMImporter(dir, false);

                GUIManager.Instance.ShowWindow("PanelLoading");
                StartCoroutine(importer.ImportAsync(delegate (VolumeDataset[] datasets) {

                    GUIManager.Instance.CloseWindow("PanelLoading");

                    if (datasets != null && datasets.Length > 0)
                    {
                        GUIManager.Instance.ShowWindow("PanelDataset", datasets);
                    }
                    else
                    {
                        GUIManager.Instance.ShowWindow("PanelAlert", "报错", "无效数据或文件未找到！");
                    }

                }));

            }
        });
    }

    bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }

    private void Update()
    {
        if (IsPointerOverUIObject()) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (m_VolumeRender.dataset == null) return;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Vector3 orign = ray.origin;
            Vector3 dir = ray.direction;

            //Debug.LogFormat("orgin {0} dir {1}", orign, dir);
            Debug.DrawLine(orign, orign + dir * 100, Color.red, 5);
 
            // 选择最优扫描方向
            float angleX = Vector3.Dot(dir, m_VolumeRender.transform.right);
            float angleY = Vector3.Dot(dir, m_VolumeRender.transform.up);
            float angleZ = Vector3.Dot(dir, m_VolumeRender.transform.forward);
            //Debug.LogFormat("angle X:{0} Y:{1} Z:{2}", angleX, angleY, angleZ);

            VolumeDataset dataset = m_VolumeRender.dataset;
            
            if (Mathf.Abs(angleX) > Mathf.Abs(angleY) && Mathf.Abs(angleX) > Mathf.Abs(angleZ))
            {
                // plane X
                FindCrossPointInVolume(orign, dir, m_VolumeRender.transform.right, angleX > 0 ? 1:-1, 0, 0, dataset.dimX);
            }
            else if (Mathf.Abs(angleY) > Mathf.Abs(angleX) && Mathf.Abs(angleY) > Mathf.Abs(angleZ))
            {
                // plane Y
                FindCrossPointInVolume(orign, dir, m_VolumeRender.transform.up, 0, angleY> 0 ? 1:-1, 0, dataset.dimY);
            }
            else if (Mathf.Abs(angleZ) > Mathf.Abs(angleX) && Mathf.Abs(angleZ) > Mathf.Abs(angleY))
            {
                // plane Z
                FindCrossPointInVolume(orign, dir, m_VolumeRender.transform.forward, 0, 0, angleZ > 0 ? 1:-1, dataset.dimZ);
            }
            
        }
    }

    // 查找ray和volume的一组交点，并根据alpha判断
    private void FindCrossPointInVolume(Vector3 origin, Vector3 dir, Vector3 planeN, float kx, float ky, float kz, int count)
    {
        VolumeDataset dataset = m_VolumeRender.dataset;

        float begin = -0.5f;
        float step = 1f / count;
        for (int i = 0; i < count; i++)
        {
            Vector3 p_local = new Vector3(kx,ky,kz) * (begin + i * step);
            Vector3 p = m_VolumeRender.transform.TransformPoint(p_local);
            Vector3 crossp = VolumeRender.Utils.RayIntersectPlane(origin, dir, planeN, p);
            Vector3 crossp_local = m_VolumeRender.transform.InverseTransformPoint(crossp);

            //Debug.LogFormat("---[Z]found cross point local--- ({0:F4},{1:F4},{2:F4})", crossp_local.x, crossp_local.y, crossp_local.z);

            if (CheckPointInVolume(dataset, crossp_local))
            {
                OnSelectVolume(crossp, crossp_local);
                break;
            }
        }
    }
    

    // 判断点在volume中
    private bool CheckPointInVolume(VolumeDataset dataset, Vector3 p)
    {
        if (p.x < -0.5f || p.x > 0.5f || p.y < -0.5f || p.y > 0.5f || p.z < -0.5f || p.z > 0.5f) return false;
        
        int X = (int)((p.x + 0.5f) * dataset.dimX);
        int Y = (int)((p.y + 0.5f) * dataset.dimY);
        int Z = (int)((p.z + 0.5f) * dataset.dimZ);
        
        if (X < 0 || X > dataset.dimX || Y < 0 || Y > dataset.dimY || Z < 0 || Z > dataset.dimZ) return false;

        int idx = X + Y * dataset.dimX + Z * dataset.dimX * dataset.dimY;

        float value = (dataset.GetData(idx) - dataset.GetMinDataValue()) / (float)(dataset.GetMaxDataValue()-dataset.GetMinDataValue());
        //Debug.LogFormat("CheckPointInVolume {0} {1} {2} - value {3} {4}", X, Y, Z, dataset.GetData(idx), value);
               
        if (m_VolumeRender.GetRenderMode() == VolumeRenderMode.IsoSurfaceRendering)
        {
            float threshold = m_VolumeRender.GetIsoSurfaceThreshold();
            return value > threshold;
        } else
        {
            float alpha = ColorMapManager.Instance.GetOpacity(value);
            return alpha > 0;
        }
    }

    void RefreshButtonState()
    {
        btnSlice.image.color = isShowSlice? Color.white: Color.clear;
        PlaneTransverse.gameObject.SetActive(isShowSlice);
        PlaneCoronal.gameObject.SetActive(isShowSlice);
        PlaneSagittal.gameObject.SetActive(isShowSlice);
    }

    void OnSelectVolume(Vector3 wpos, Vector3 lpos)
    {
        if (isShowSlice)
        {
            PlaneTransverse.localPosition = new Vector3(0, 0, lpos.z);
            PlaneCoronal.localPosition = new Vector3(0, lpos.y, 0);
            PlaneSagittal.localPosition = new Vector3(lpos.x, 0, 0);
        }        
    }
    
    void OnDatasetLoad(Notification notification)
    {
        m_VolumeRender.gameObject.SetActive(false);

        isShowSlice = false;
        RefreshButtonState();
        
    }

    void OnDatasetSelect(Notification notification)
    {
        NotificationCenter.DefaultCenter().PostNotification(NotificationType.VR_RENDER_REFRESH);
        NotificationCenter.DefaultCenter().PostNotification(NotificationType.VR_COLORMAP_UPDATE);
    }

}
