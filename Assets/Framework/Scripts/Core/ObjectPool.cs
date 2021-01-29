using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Framework
{
    [System.Serializable]
    public class PoolInfo
    {
        public string poolName;
        public GameObject prefab;
        public int poolSize;
        public bool fixedSize;
    }

    class Pool
    {
        private Stack<PoolObject> availableObjStack = new Stack<PoolObject>();

        private bool fixedSize;
        private GameObject poolObjectPrefab;
        private int poolSize;
        private string poolName;

        private ObjectPool parent;

        public Pool(string poolName, GameObject poolObjectPrefab, int initialCount, bool fixedSize, ObjectPool parent)
        {
            this.poolName = poolName;
            this.poolObjectPrefab = poolObjectPrefab;
            this.poolSize = initialCount;
            this.fixedSize = fixedSize;
            this.parent = parent;
            //populate the pool
            for (int index = 0; index < initialCount; index++)
            {
                AddObjectToPool(NewObjectInstance());
            }
        }

        //o(1)
        private void AddObjectToPool(PoolObject po)
        {
            //add to pool
            po.gameObject.SetActive(false);
            availableObjStack.Push(po);
            po.isPooled = true;
            po.transform.SetParent(parent.transform);
        }

        private PoolObject NewObjectInstance()
        {
            GameObject go = (GameObject)GameObject.Instantiate(poolObjectPrefab);
            PoolObject po = go.GetComponent<PoolObject>();
            if (po == null)
            {
                po = go.AddComponent<PoolObject>();
            }
            //set name
            po.poolName = poolName;
            return po;
        }

        //o(1)
        public PoolObject NextAvailableObject()
        {
            PoolObject po = null;
            if (availableObjStack.Count > 0)
            {
                po = availableObjStack.Pop();
            }
            else if (fixedSize == false)
            {
                //increment size var, this is for info purpose only
                poolSize++;
                Debug.Log(string.Format("Growing pool {0}. New size: {1}", poolName, poolSize));
                //create new object
                po = NewObjectInstance();
            }
            else
            {
                Debug.LogWarning("No object available & cannot grow pool: " + poolName);
            }

            if (po != null)
            {
                po.isPooled = false;
            }

            return po;
        }

        //o(1)
        public void ReturnObjectToPool(PoolObject po)
        {

            if (poolName.Equals(po.poolName))
            {

                /* we could have used availableObjStack.Contains(po) to check if this object is in pool.
                 * While that would have been more robust, it would have made this method O(n) 
                 */
                if (po.isPooled)
                {
                    Debug.LogWarning(po.gameObject.name + " is already in pool. Why are you trying to return it again? Check usage.");
                }
                else
                {
                    AddObjectToPool(po);
                }

            }
            else
            {
                Debug.LogError(string.Format("Trying to add object to incorrect pool {0} {1}", po.poolName, poolName));
            }
        }
    }

    public class ObjectPool : MonoBehaviour
    {

        static ObjectPool _instance;
        [Header("Editing Pool Info value at runtime has no effect")]
        public PoolInfo[] poolInfo;

        //mapping of pool name vs list
        private Dictionary<string, Pool> poolDictionary = new Dictionary<string, Pool>();

        public static ObjectPool instance
        {
            get
            {
                if (_instance != null)
                    return _instance;

                _instance = Object.FindObjectOfType<ObjectPool>();
                if (_instance != null)
                    return _instance;

                var obj = new GameObject("ObjectPool");
                obj.transform.localPosition = Vector3.zero;
                obj.transform.localRotation = Quaternion.identity;
                obj.transform.localScale = Vector3.one;
                _instance = obj.AddComponent<ObjectPool>();
                return _instance;
            }
        }

        // Use this for initialization
        void Awake()
        {
            //set instance
            _instance = this;
            //create pools
            CreatePools();
        }

        private void CreatePools()
        {
            if (poolInfo == null) return;

            foreach (PoolInfo currentPoolInfo in poolInfo)
            {

                Pool pool = new Pool(currentPoolInfo.poolName, currentPoolInfo.prefab,
                                     currentPoolInfo.poolSize, currentPoolInfo.fixedSize, this);


                Debug.Log("Creating pool: " + currentPoolInfo.poolName);
                //add to mapping dict
                poolDictionary[currentPoolInfo.poolName] = pool;
            }
        }

        public static void CreatePool(string poolName, GameObject poolPrefab, int initialCount, bool fixedSize)
        {

            if (poolPrefab != null && !instance.poolDictionary.ContainsKey(poolName))
            {
                Pool pool = new Pool(poolName, poolPrefab, initialCount, fixedSize, instance);

                Debug.Log("Creating pool: " + poolName);

                //add to mapping dict
                instance.poolDictionary[poolName] = pool;
            }
            else
            {
                Debug.LogWarning(string.Format("Pool: {0} have been already pooled.", poolName));
            }
        }
        public static void CreatePool(string poolName, GameObject poolPrefab, int initialCount)
        {
            CreatePool(poolName, poolPrefab, initialCount, false);
        }

        public static GameObject Spawn(string poolName, Transform parent, Vector3 position, Quaternion rotation, float lifetime)
        {
            GameObject result = null;

            if (instance.poolDictionary.ContainsKey(poolName))
            {
                Pool pool = instance.poolDictionary[poolName];
                PoolObject po = pool.NextAvailableObject();

                po.gameObject.SetActive(false);

                if (po != null)
                {
                    po.lifetime = lifetime;
                    result = po.gameObject;
                    Transform trans = po.transform;
                    trans.SetParent(parent);
                    trans.localPosition = position;
                    trans.localRotation = rotation;

                    po.gameObject.SetActive(true);
                }
                else
                {
                    Debug.LogWarning("No object available in pool. Consider setting fixedSize to false.: " + poolName);
                }

            }
            else
            {
                Debug.LogError("Invalid pool name specified: " + poolName);
            }

            return result;
        }
        public static GameObject Spawn(string poolName, Transform parent, Vector3 position, float lifetime)
        {
            return Spawn(poolName, parent, position, Quaternion.identity, lifetime);
        }
        public static GameObject Spawn(string poolName, Transform parent, Vector3 position)
        {
            return Spawn(poolName, parent, position, Quaternion.identity, 0);
        }
        public static GameObject Spawn(string poolName, Vector3 position, Quaternion rotation)
        {
            return Spawn(poolName, null, position, rotation, 0);
        }
        public static GameObject Spawn(string poolName, Transform parent)
        {
            return Spawn(poolName, parent, Vector3.zero, Quaternion.identity, 0);
        }
        public static GameObject Spawn(string poolName, Vector3 position)
        {
            return Spawn(poolName, null, position, Quaternion.identity, 0);
        }
        public static GameObject Spawn(string poolName)
        {
            return Spawn(poolName, null, Vector3.zero, Quaternion.identity, 0);
        }

        public static void Recycle(GameObject go)
        {
            PoolObject po = go.GetComponent<PoolObject>();
            if (po == null)
            {
                Debug.LogWarning("Specified object is not a pooled instance: " + go.name);
            }
            else
            {
                if (instance.poolDictionary.ContainsKey(po.poolName))
                {
                    Pool pool = instance.poolDictionary[po.poolName];
                    pool.ReturnObjectToPool(po);
                }
                else
                {
                    Debug.LogWarning("No pool available with name: " + po.poolName);
                }
            }
        }
    }
}