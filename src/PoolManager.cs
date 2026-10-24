using System.Collections.Generic;
using UnityEngine;

// 池化对象基类:替代原 IPoolable 接口,样板(Bind/Owner)在此统一实现
public abstract class PoolableBehaviour : MonoBehaviour
{
    public ObjectPool Owner { get; private set; }

    public void Bind(ObjectPool op) => Owner = op;

    public abstract void Spawn();    // 每次复用时的业务初始化
    public abstract void Despawn();  // 归还时的状态归零
}

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }
    private readonly Dictionary<string, ObjectPool> _pools = new Dictionary<string, ObjectPool>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public GameObject Spawn(string key, Transform parent, GameObject prefab = null)
    {
        if (!_pools.ContainsKey(key))
        {
            _pools.Add(key, new ObjectPool(key, parent, prefab, 6));
        }
        return _pools[key].Spawn();
    }

    public void Despawn(PoolableBehaviour item)
    {
        item.Owner?.Despawn(item);
    }
}
