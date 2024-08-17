using System.Collections.Generic;
using UnityEngine;

public class MemoryPoolSystem : MonoBehaviour
{
    public static MemoryPoolSystem Instance { get; private set; } = null;
    private Dictionary<GameObject, List<GameObject>> objectPools = new Dictionary<GameObject, List<GameObject>>();
    private static GameObject parentPool;
    private List<GameObject> pendingSetParent = new List<GameObject>();

    GameObject GetparentPool()
    {
        if(parentPool == null)
        {
            parentPool = new GameObject("MemoryPoolParent");   
            AutoClear_MemotyPoolParentOnDestroy autoDestory = parentPool.GetComponent<AutoClear_MemotyPoolParentOnDestroy>(); 
            if (autoDestory == null) autoDestory = parentPool.AddComponent<AutoClear_MemotyPoolParentOnDestroy>(); //AddComp Auto Clear
        }
        return parentPool;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            parentPool = GetparentPool();
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    void Update() {
        if(pendingSetParent.Count <= 0) return;
        for (int i = pendingSetParent.Count - 1; i >= 0; i--)
        {
            GameObject obj = pendingSetParent[i];
            if (obj != null)
            {
                obj.transform.SetParent(GetparentPool().transform);
                pendingSetParent.RemoveAt(i);  // 리스트에서 제거
            }
        }
    }

    public GameObject GetObject(GameObject prefab, Vector3 position = default, Quaternion rotation = default, Transform parent = null)
    {
        if (!objectPools.TryGetValue(prefab, out List<GameObject> pool))
        {
            pool = new List<GameObject>();
            objectPools[prefab] = pool;
        }

        GameObject obj = FindInactiveObject(pool);
        if (obj == null)
        {
            obj = Object.Instantiate(prefab);
            pool.Add(obj);
        }

        obj.transform.SetPositionAndRotation(position, rotation);
        if(parent == null)
        {
            obj.transform.SetParent(null); //Detach From Memorypool Parent
        }
        else
        {
            obj.transform.SetParent(parent);
        }
        obj.SetActive(true);

        // Check if the object already has AutoReleaseOnDisable and initialize it if needed
        AutoReleaseOnDisable autoRelease = obj.GetComponent<AutoReleaseOnDisable>();
        if (autoRelease == null)
        {
            autoRelease = obj.AddComponent<AutoReleaseOnDisable>();
        }
        autoRelease.Initialize(this, prefab);  // Pass the prefab to AutoReleaseOnDisable

        return obj;
    }

    public void ReleaseObject(GameObject obj)
    {
        if (obj == null)
        {
            Debug.LogWarning("Attempted to release a null object.");
            return;
        }

        obj.SetActive(false);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localScale = Vector3.one;
        pendingSetParent.Add(obj); //Setparent Target Late
    }

    public void ClearAllObjects()
    {
        foreach (var pool in objectPools.Values)
        {
            foreach (var obj in pool)
            {
                GameObject.Destroy(obj);
            }
        }
        objectPools.Clear();
    }

    private GameObject FindInactiveObject(List<GameObject> pool)
    {
        foreach (var obj in pool)
        {
            if (!obj.activeSelf && obj.transform.parent.gameObject == GetparentPool())
            {
                return obj;
            }
        }
        return null;
    }
}

public class AutoReleaseOnDisable : MonoBehaviour
{
    private MemoryPoolSystem poolSystem;
    public GameObject Prefab { get; private set; }

    public void Initialize(MemoryPoolSystem system, GameObject prefab)
    {
        poolSystem = system;
        Prefab = prefab;
    }

    private void OnDisable()
    {
        if (poolSystem != null)
        {
            poolSystem.ReleaseObject(gameObject);
        }
    }
}

public class AutoClear_MemotyPoolParentOnDestroy : MonoBehaviour
{
    void OnDestroy() {
        MemoryPoolSystem.Instance.ClearAllObjects();
    }
}
