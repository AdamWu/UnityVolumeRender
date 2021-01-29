using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

namespace Framework
{
    [RequireComponent(typeof(Camera))]
    public class CameraZoom : MonoBehaviour
    {
        Camera camera;

        void Awake()
        {
            camera = GetComponent<Camera>();
        }
        void Update()
        {

            if (EventSystem.current && EventSystem.current.IsPointerOverGameObject()) return;

            if (Input.GetAxisRaw("Mouse ScrollWheel") < 0)
            {

                if (camera.fieldOfView <= 120)
                {
                    camera.fieldOfView += 2;
                }
                if (camera.orthographicSize <= 100)
                {
                    camera.orthographicSize += 0.5F;
                }
            }

            if (Input.GetAxis("Mouse ScrollWheel") > 0)
            {
                if (camera.fieldOfView > 30)
                {
                    camera.fieldOfView -= 2;
                }
                if (camera.orthographicSize >= 10)
                {
                    camera.orthographicSize -= 0.5f;
                }
            }
        }
    }
}