using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

namespace Framework
{
    // camera旋转
    public class CameraRotate : MonoBehaviour
    {
        public Transform Target;
        public float xSpeed = 8;
        public float ySpeed = 8;
        public float yMinLimit = -50, yMaxLimit = 50;

        private float distance;
        private float angleX = 0.0f, angleY = 0.0f;

        public void SetTarget(Transform target)
        {
            Target = target;

            distance = (transform.position - Target.position).magnitude;

            //Vector3 angles = transform.eulerAngles;
            //angleX = angles.y;
            //angleY = angles.x;
        }

        public void Reset()
        {

            transform.rotation = Quaternion.identity;
            transform.localPosition = new Vector3(0, 0, -distance);
            angleX = 0;
            angleY = 0;
        }

        void Start()
        {

            if (Target == null) return;
            distance = (transform.position - Target.position).magnitude;
        }

        void Update()
        {
            if (Target == null) return;
            if (EventSystem.current && EventSystem.current.IsPointerOverGameObject()) return;

            if (Input.GetMouseButton(0))
            {

                angleX += Input.GetAxis("Mouse X") * xSpeed;
                angleY -= Input.GetAxis("Mouse Y") * xSpeed;
                angleY = ClampAngle(angleY, yMinLimit, yMaxLimit);

                Quaternion rotation = Quaternion.Euler(angleY, angleX, 0.0f);
                Vector3 disVector = new Vector3(0.0f, 0f, -distance);
                //四元数左*向量
                Vector3 position = rotation * disVector + Target.position;

                transform.rotation = rotation;
                transform.position = position;

            }
        }

        static float ClampAngle(float angle, float min, float max)
        {
            if (angle < -360)
                angle += 360;
            if (angle > 360)
                angle -= 360;
            return Mathf.Clamp(angle, min, max);
        }
    }

}