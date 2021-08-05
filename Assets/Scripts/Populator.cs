using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Populator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject _agentPrefab;

    [Header("Settings")]
    [SerializeField] private float _spawnAmount = 100;

    private GameObject _agentsFather;

    private void Awake()
    {
        _agentsFather = new GameObject("All Agents");
    }

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < _spawnAmount; i++)
        {
            Vector3 spawnPos;
            GameObject spawnedObject;

            spawnPos = EntireField.GetRandomPosition();
            spawnedObject = GameObject.Instantiate(_agentPrefab, _agentsFather.transform);

            spawnedObject.transform.position = spawnPos;
        }
    }
}
