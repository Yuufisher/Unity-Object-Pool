using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class ObjectPool
{
    private readonly Stack<PoolableBehaviour> _idle = new Stack<PoolableBehaviour>();
    private readonly HashSet<PoolableBehaviour> _active = new HashSet<PoolableBehaviour>();
    private readonly string key;
    private readonly int maxSize;
    private readonly Transform root;
    private readonly GameObject _object;

    public ObjectPool(string key, Transform parent, GameObject pb = null, int prewarm = 10, int maxSize = 20)
    {
        this.maxSize = maxSize;
        this.key = key;
        this.root = parent;
        _object = pb ? pb : Object.Instantiate(Resources.Load<GameObject>("PoolPrefabs/" + key), root);
        for (int i = 0; i < prewarm; i++)
        {
            var item = CreateInstance();
            item.gameObject.SetActive(false);
            _idle.Push(item);
        }
    }

    private PoolableBehaviour CreateInstance()
    {
        var go = Object.Instantiate(_object, root);
        var item = go.GetComponent<PoolableBehaviour>();
        if (item == null)
            throw new System.Exception($"[ObjectPool] 预制体 {key} 缺少 PoolableBehaviour 派生组件");
        item.Bind(this);
        return item;
    }

    public GameObject Spawn()
    {
        if (!_idle.TryPop(out var item))
        {
            item = CreateInstance(); 
        }
        _active.Add(item);
        item.gameObject.SetActive(true);
        item.Spawn();
        return item.gameObject;
    }

    public void Despawn(PoolableBehaviour item)
    {
        item.Despawn();
        _active.Remove(item);
        if (_idle.Count > maxSize)
        {
            Object.Destroy(item.gameObject);
        }
        else
        {
            item.gameObject.SetActive(false);
            _idle.Push(item);
        }
    }
}
