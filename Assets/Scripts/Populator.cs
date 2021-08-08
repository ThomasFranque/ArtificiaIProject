using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Populator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject _agentPrefab;
    [SerializeField] private EntireField _field;
    [SerializeField] private float _delayBetweenSpawns = 0.5f;
    [SerializeField] private bool _suppress;

    [field: SerializeField] public int SpawnAmount { get; private set; } = 100;
    private int _agentsPopulated;

    private GameObject _agentsFather;
    private AgentBrain _brain;
    private WaitForSeconds _waitForDelayBetweenSpawn;

    // Start is called before the first frame update
    public void StartPopulating(AgentBrain brain)
    {
        if (_agentsFather == default)
            _agentsFather = new GameObject("All Agents");

        _brain = brain;
        _waitForDelayBetweenSpawn = new WaitForSeconds(_delayBetweenSpawns);
        StartCoroutine(CPopulate());
    }

    private IEnumerator CPopulate()
    {
        while (true)
        {
            Vector3 spawnPos;
            GameObject spawnedObject;
            AgentEntity agent;

            spawnPos = _field.GetRandomEntrance();
            spawnedObject = GameObject.Instantiate(_agentPrefab, _agentsFather.transform);
            agent = spawnedObject.GetComponent<AgentEntity>();


            spawnedObject.transform.position = spawnPos;
            _brain.RegisterNewAgent(agent);
            agent.OnKill += (int id) => _agentsPopulated--;
            _agentsPopulated++;

            if (_agentsPopulated == SpawnAmount)
                yield return new WaitUntil(() => _agentsPopulated < SpawnAmount);
            yield return _waitForDelayBetweenSpawn;
        }
    }

    public void Suppress()
    {
        _suppress = true;
        Debug.Log("Spawning Suppressed");
    }

    public void UnSuppress()
    {
        _suppress = false;
        Debug.Log("Spawning Resumed");
    }
}
