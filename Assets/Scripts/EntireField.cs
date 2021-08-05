using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntireField : MonoBehaviour
{
    private static EntireField _instance;
    [SerializeField] private Vector2 _spawnRadius;
    [SerializeField] private bool _drawSpawnRadius = true;

    private void Awake()
    {
        _instance = this;
    }

    private void OnDrawGizmos()
    {
        if (!_drawSpawnRadius) return;
        Gizmos.color = Color.black;
        Gizmos.DrawWireCube(transform.position, new Vector3(_spawnRadius.x, 1.5f, _spawnRadius.y));
    }
    private Vector3 InstanceGetRandomPoint()
    {
        float x;
        float z;

        x = Random.Range(-_spawnRadius.x / 2, _spawnRadius.x / 2);
        z = Random.Range(-_spawnRadius.y / 2, _spawnRadius.y / 2);

        return new Vector3(x, 0, z);
    }
    public static Vector3 GetRandomPosition() => _instance.InstanceGetRandomPoint();
}
