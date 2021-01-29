using UnityEngine;

namespace VolumeRender
{
    public enum SlicingPlaneType
    {
        Transverse=0,
        Coronal,
        Sagittal,
    }

    public class SlicingPlane : MonoBehaviour, IGL
    {
        public SlicingPlaneType Type;

        private Material m_Material;
        private Color BorderColor;
        private Material m_LineMaterial;

        private void Awake()
        {
            m_Material = GetComponent<MeshRenderer>().material;

            switch (Type)
            {
                case SlicingPlaneType.Transverse:
                    BorderColor = Color.blue;
                    break;
                case SlicingPlaneType.Coronal:
                    BorderColor = Color.green;
                    break;
                case SlicingPlaneType.Sagittal:
                    BorderColor = Color.red;
                    break;
            }
            m_Material.SetColor("_BorderColor", BorderColor);
            Object prefab = Resources.Load("SlicingBorderMaterial", typeof(Material));
            m_LineMaterial = Instantiate(prefab) as Material;
            m_LineMaterial.color = BorderColor;
        }

        private void OnEnable()
        {
            if (GLRenderer.Instance != null)
                GLRenderer.Instance.Add(this);
        }

        private void OnDisable()
        {
            if (GLRenderer.Instance != null)
                GLRenderer.Instance.Remove(this);
        }

        private void OnDestroy()
        {
            if (GLRenderer.Instance != null)
                GLRenderer.Instance.Remove(this);
        }

        private void Update()
        {
            if (m_Material == null) m_Material = GetComponent<MeshRenderer>().material;

            m_Material.SetMatrix("_parentInverseMat", transform.parent.worldToLocalMatrix);
            m_Material.SetMatrix("_planeMat", transform.localToWorldMatrix);
        }

        public void SetDataset(VolumeDataset dataset)
        {
            if (m_Material == null) m_Material = GetComponent<MeshRenderer>().material;
            
            Texture3D texture = dataset.GetTexture();
            m_Material.SetTexture("_VolumeTex", texture);
        }

        void IGL.Draw()
        {
            Vector3 Size = transform.parent.localScale;

            Matrix4x4 mat = Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale);

            GL.PushMatrix();

            GL.MultMatrix(mat);
            m_LineMaterial.SetPass(0);
            GL.Begin(GL.LINES);

            float sx = 1f, sy = 1f;
            switch (Type)
            {
                case SlicingPlaneType.Transverse:
                    sx = Size.x;sy = Size.y;
                    break;
                case SlicingPlaneType.Coronal:
                    sx = Size.x; sy = Size.z;
                    break;
                case SlicingPlaneType.Sagittal:
                    sx = Size.z; sy = Size.y;
                    break;
            }

            GL.Color(BorderColor);
            GL.Vertex3(-0.5f * sx, -0.5f * sy, 0); GL.Vertex3(0.5f * sx, -0.5f * sy, 0);
            GL.Vertex3(0.5f * sx, -0.5f * sy, 0); GL.Vertex3(0.5f * sx, 0.5f * sy, 0);
            GL.Vertex3(0.5f * sx, 0.5f * sy, 0); GL.Vertex3(-0.5f * sx, 0.5f * sy, 0);
            GL.Vertex3(-0.5f * sx, 0.5f * sy, 0); GL.Vertex3(-0.5f * sx, -0.5f * sy, 0);
            GL.End();

            GL.PopMatrix();

        }
    }
}
