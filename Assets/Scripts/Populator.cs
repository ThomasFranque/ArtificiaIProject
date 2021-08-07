using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Populator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject _agentPrefab;

    [field: SerializeField] public int SpawnAmount { get; private set; } = 100;

    private GameObject _agentsFather;

    private void Awake()
    {
        _agentsFather = new GameObject("All Agents");
    }

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < SpawnAmount; i++)
        {
            Vector3 spawnPos;
            GameObject spawnedObject;
            AgentEntity agent;

            spawnPos = EntireField.GetRandomPosition();
            spawnedObject = GameObject.Instantiate(_agentPrefab, _agentsFather.transform);
            agent = spawnedObject.GetComponent<AgentEntity>();


            AgentBrain.RegisterNewAgent(agent);
            spawnedObject.transform.position = spawnPos;
        }
    }
}
