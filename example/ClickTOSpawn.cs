using System;
using UnityEngine;
using UnityEngine.UIElements;

public class ClickTOSpawn : MonoBehaviour
{
    public GameObject  prefab;
    public Transform parent;
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            PoolManager.Instance.Spawn("triangle", parent,prefab);
        }
    }
}
