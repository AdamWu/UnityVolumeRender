/* 
 * Unless otherwise licensed, this file cannot be copied or redistributed in any format without the explicit consent of the author.
 * (c) Preet Kamal Singh Minhas, http://marchingbytes.com
 * contact@marchingbytes.com
 */
using UnityEngine;
using System.Collections;

namespace Framework
{
    public class PoolObject : MonoBehaviour
    {
        public string poolName;
        //defines whether the object is waiting in pool or is in use
        public bool isPooled;

        public float lifetime = 0;

        void OnEnable()
        {
            if (lifetime > 0)
            {
                StartCoroutine(AutoDestroy(lifetime));
            }
        }

        void OnDisable()
        {
            StopAllCoroutines();
        }

        IEnumerator AutoDestroy(float time)
        {
            yield return new WaitForSeconds(time);

            // recycle
            ObjectPool.Recycle(gameObject);
        }

    }
}

