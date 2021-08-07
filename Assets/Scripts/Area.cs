using System.Collections.Generic;
using UnityEngine;

public class Area : MonoBehaviour
{
    [field: SerializeField] 
    public AreaType Type { get; private set; }
    public Dictionary<Collider, AgentEntity> AgentsInArea { get; private set; }

    private void Awake()
    {
        AgentsInArea = new Dictionary<Collider, AgentEntity>(25);
    }

    public void Interact(AgentEntity agent)
    {
        Debug.Log("agent " + agent.ID + " Interacted with " + name);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsAgent(other)) return;
        AgentEntity entity;

        entity = GetAgentComponent(other);
        entity.EnteredArea(this);
        AgentsInArea.Add(other, entity);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsAgent(other)) return;
        AgentEntity agent = default;

        if (AgentsInArea.ContainsKey(other))
        {
            AgentsInArea.TryGetValue(other, out agent);
            AgentsInArea.Remove(other);
        }
        if (agent == default)
            agent = GetAgentComponent(other);

        agent.LeftArea(this);
    }

    private bool IsAgent(Collider other) => other.tag != AgentEntity.TAG;
    private AgentEntity GetAgentComponent(Collider other) => other.GetComponent<AgentEntity>();
}
