using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ObjectPoolManager : Singleton<ObjectPoolManager>
{
    [SerializeField] private List<IObjectPool<GameObject>> objectPools = new List<IObjectPool<GameObject>>();

    protected override void Awake()
    {
        base.Awake();
    }
    public void Initialize(List<GameObject> objectPrefabs)
    {
        foreach (var prefab in objectPrefabs)
        {
            Transform pooledObjParent = new GameObject(prefab.name + " Pool").transform;
            pooledObjParent.SetParent(this.transform);

            var pool = new ObjectPool<GameObject>(
                createFunc: () => GameInstance.Instance.SpawnGameObjectInWorld(prefab, pooledObjParent),
                actionOnGet: (obj) => obj.SetActive(true),
                actionOnRelease: (obj) => obj.SetActive(false),
                actionOnDestroy: (obj) => Destroy(obj),
                collectionCheck: false,
                defaultCapacity: 10,
                maxSize: 100
            );

            objectPools.Add(pool);
        }
    }
    
    public GameObject GetObjectFromPool(int poolIndex)
    {
        if (poolIndex >= 0 && poolIndex < objectPools.Count)
        {
            return objectPools[poolIndex].Get();
        }
        else
        {
            Debug.LogError($"Invalid pool index {poolIndex}. Returning null.");
            return null;
        }
    }

    public void ReleaseObjectToPool(int poolIndex, GameObject obj)
    {
        if (poolIndex >= 0 && poolIndex < objectPools.Count)
        {
            objectPools[poolIndex].Release(obj);
        }
        else
        {
            Debug.LogError($"Invalid pool index {poolIndex}. Cannot release object to pool.");
        }
    }
}
