using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Agent : MonoBehaviour
{
    private NavMeshAgent _navMeshAgent;
    private void Awake()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        StartCoroutine(CMoveToNewPosition());
    }

    protected virtual void MoveTo(Vector3 position)
    {
        _navMeshAgent.destination = position;
    }

    private IEnumerator CMoveToNewPosition()
    {
        yield return new WaitForSeconds(Random.Range(1f, 10f));
        MoveTo(EntireField.GetRandomPosition());
        StartCoroutine(CMoveToNewPosition());
    }
}
