using UnityEngine;

namespace VolumeRender
{

    public enum VolumeRenderMode
    {
        IsoSurfaceRendering,
        DirectVolumeRendering,
        //MaximumIntensityProjectipon
    }

    [RequireComponent(typeof(MeshRenderer))]
    public class VolumeRenderer : MonoBehaviour
    {
        [HideInInspector]
        public VolumeDataset dataset;
        private Material m_Material;

        private VolumeRenderMode m_RenderMode = VolumeRenderMode.DirectVolumeRendering;

        // iso surface render
        private float m_IsoSurfaceThreshold = 0.1f;
        private Color m_IsoSurfaceColor = Color.yellow;

        // light
        private bool m_Shade = false;
        private float m_Ambient = 0.3f;
        private float m_Diffuse = 0.5f;
        private float m_Specular = 0.5f;
        private float m_SpecularPower = 20;

        private float MinValue = 0;
        private float MaxValue = 1;

        private void Awake()
        {
            MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
            m_Material = meshRenderer.sharedMaterial;
        }

        public void SetDataset(VolumeDataset dataset)
        {
            this.dataset = dataset;
            m_Material.SetTexture("_VolumeTex", dataset.GetTexture());

            Vector4 VolumeSize = new Vector4(dataset.dimX, dataset.dimY, dataset.dimZ, 0f);
            m_Material.SetVector("_VolumeTex_TexelSize", VolumeSize);


            Texture2D noiseTexture = NoiseTextureGenerator.GenerateNoiseTexture(512, 512);
            m_Material.SetTexture("_NoiseTex", noiseTexture);
        }

        public void SetTFTexture(Texture2D texture)
        {
            m_Material.SetTexture("_TFTex", texture);
        }

        public float GetAmbient() { return m_Ambient; }
        public void SetAmbient(float value)
        {
            m_Ambient = value;
            m_Material.SetFloat("_Ambient", m_Ambient);
        }
        public float GetDiffuse() { return m_Diffuse; }
        public void SetDiffuse(float value)
        {
            m_Diffuse = value;
            m_Material.SetFloat("_Diffuse", m_Diffuse);
        }
        public float GetSpecular() { return m_Specular; }
        public void SetSpecular(float value)
        {
            m_Specular = value;
            m_Material.SetFloat("_Specular", m_Specular);
        }
        public float GetSpecularPower() { return m_SpecularPower; }
        public void SetSpecularPower(float value)
        {
            m_SpecularPower = value;
            m_Material.SetFloat("_SpecularPower", m_SpecularPower);
        }
        public float GetIsoSurfaceThreshold() { return m_IsoSurfaceThreshold; }
        public void SetIsoSurfaceThreshold(float value)
        {
            m_IsoSurfaceThreshold = value;
            m_Material.SetFloat("_IsoSurfaceThreshold", m_IsoSurfaceThreshold);
        }
        public Color GetIsoSurfaceColor() { return m_IsoSurfaceColor; }
        public void SetIsoSurfaceColor(Color color)
        {
            m_IsoSurfaceColor = color;
            m_Material.SetColor("_IsoSurfaceColor", m_IsoSurfaceColor);
        }

        public void SetCutMinValue(float value)
        {
            MinValue = Mathf.Clamp(value, 0, MaxValue);
            m_Material.SetFloat("_MinVal", MinValue);
        }

        public void SetCutMaxValue(float value)
        {
            MaxValue = Mathf.Clamp(value, MinValue, 1f);
            m_Material.SetFloat("_MaxVal", MaxValue);
        }

        public VolumeRenderMode GetRenderMode()
        {
            return m_RenderMode;
        }

        public void SetRenderMode(VolumeRenderMode mode)
        {
            m_RenderMode = mode;
            switch (mode)
            {
                case VolumeRenderMode.DirectVolumeRendering:
                    {
                        m_Material.EnableKeyword("MODE_DVR");
                        m_Material.DisableKeyword("MODE_SURF");
                        break;
                    }
                case VolumeRenderMode.IsoSurfaceRendering:
                    {
                        m_Material.DisableKeyword("MODE_DVR");
                        m_Material.EnableKeyword("MODE_SURF");
                        break;
                    }
            }
        }

        public bool GetMaterialShade() { return m_Shade; }
        public void SetMaterialShade(bool value)
        {
            m_Shade = value;
            Debug.Log("SetMaterialShade:" + value);
            if (value)
            {
                m_Material.EnableKeyword("SHADE");
            }
            else
            {
                m_Material.DisableKeyword("SHADE");
            }
        }

        private void OnWillRenderObject()
        {
            Matrix4x4 PVM = (Camera.current.projectionMatrix * Camera.current.worldToCameraMatrix * transform.localToWorldMatrix).inverse;
            Shader.SetGlobalMatrix("VolumeClipToObject", PVM);
        }
    }

}