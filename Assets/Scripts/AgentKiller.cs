using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class AgentKiller : MonoBehaviour
{
    [SerializeField] private string _agentTag;
    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == _agentTag)
            KillAgent(other);
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.tag == _agentTag)
            KillAgent(other);
    }
    private void KillAgent(Collider c)
    {
        c.GetComponent<AgentEntity>().Kill();
    }
}
