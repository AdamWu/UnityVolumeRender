using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

namespace Framework
{
    // move
    public class CameraMove : MonoBehaviour
    {
        float delta_x, delta_y, speed;
        Quaternion rotation;

        void Start()
        {
            delta_x = 1; delta_y = 1; speed = 0.2f;
            StartCoroutine(OnMouseDown());
        }
        IEnumerator OnMouseDown()
        {
            float temp_x, temp_y;
            while (true)
            {
                if (Input.GetMouseButton(1))
                {
                    temp_x = Input.GetAxis("Mouse X") * speed;
                    if (transform.position.x - temp_x > -1.8f && transform.position.x - temp_x < 1.8f)
                        delta_x = temp_x;
                    else
                        delta_x = 0;
                    temp_y = Input.GetAxis("Mouse Y") * speed;

                    if (transform.position.y - temp_y > -1.8f && transform.position.y - temp_y < 1.8f)
                        delta_y = temp_y;
                    else
                        delta_y = 0;
                    //  camera.transform.localEulerAngles =  new Vector3(0,0, 0);
                    rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);

                    transform.position = rotation * new Vector3(-delta_x, -delta_y, 0) + transform.position;

                }
                yield return new WaitForFixedUpdate(); //这个很重要，循环执行
            }

        }
    }

}