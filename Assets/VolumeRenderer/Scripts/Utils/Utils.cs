using UnityEngine;

namespace VolumeRender
{
    public class Utils
    {
        // ����ray��plane�Ľ���
        public static Vector3 RayIntersectPlane(Vector3 point, Vector3 direct, Vector3 planeNormal, Vector3 planePoint)
        {
            float d = Vector3.Dot(planePoint - point, planeNormal) / Vector3.Dot(direct.normalized, planeNormal);
            return d * direct.normalized + point;
        }
    }
}