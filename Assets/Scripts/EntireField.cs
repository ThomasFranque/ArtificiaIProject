using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntireField : MonoBehaviour
{
    private static EntireField _instance;
    [SerializeField] private Vector2 _fieldArea = new Vector2(99, 99);
    [SerializeField] private bool _drawSpawn = true;

    [SerializeField] private GameObject _entryPointVisual;
    [SerializeField] private GameObject _exitPointVisual;
    [SerializeField] private Transform[] _entryPoints;
    [SerializeField] private Transform[] _exitPoints;

    public Vector3 GetClosestExit(Vector3 from)
    {
        Vector3 closestPos = default;
        float closest = float.PositiveInfinity;

        for (int i = 0; i < _exitPoints.Length; i++)
        {
            Transform t = _exitPoints[i];
            float d = Vector3.Distance(t.position, from);

            if (d < closest)
            {
                closest = d;
                closestPos = t.position;
            }            
        }

        return closestPos;
    }

    public Vector3 GetRandomEntrance()
    {
        return _entryPoints[Random.Range(0, _entryPoints.Length)].position;
    }

    private void Awake()
    {
        _instance = this;

        foreach (Transform t in _entryPoints)
            GameObject.Instantiate(_entryPointVisual).transform.position = t.position;
        foreach (Transform t in _exitPoints)
            GameObject.Instantiate(_exitPointVisual).transform.position = t.position;
    }

    private void OnDrawGizmos()
    {
        if (!_drawSpawn) return;
        Gizmos.color = Color.black;
        Gizmos.DrawWireCube(transform.position, new Vector3(_fieldArea.x, 1.5f, _fieldArea.y));

        Gizmos.color = Color.green;
        foreach (Transform t in _entryPoints)
            Gizmos.DrawCube(t.position, new Vector3(1f, 0.1f, 1f));
        Gizmos.color = Color.red;
        foreach (Transform t in _exitPoints)
            Gizmos.DrawCube(t.position, new Vector3(1f, 0.1f, 1f));
    }
    private Vector3 InstanceGetRandomPoint()
    {
        float x;
        float z;

        x = Random.Range(-_fieldArea.x / 2, _fieldArea.x / 2);
        z = Random.Range(-_fieldArea.y / 2, _fieldArea.y / 2);

        return new Vector3(x, 0, z);
    }
    public static Vector3 GetRandomPosition() => _instance.InstanceGetRandomPoint();
}
