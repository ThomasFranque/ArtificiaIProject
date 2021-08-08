using System.Collections.Generic;
using UnityEngine;

public class Area : MonoBehaviour
{
    [field: SerializeField]
    public AreaType Type { get; private set; }

    [SerializeField]
    private Transform _ground;
    [SerializeField]
    private LayerMask _obstructionLayers;
    public Dictionary<Collider, AgentEntity> AgentsInArea { get; private set; }

    protected virtual void Awake()
    {
        AgentsInArea = new Dictionary<Collider, AgentEntity>(25);
    }

    public virtual States Interact(AgentEntity agent)
    {
        //agent.SetDesiredPosition(GetPositionInArea());
        return GetInteractionState();
    }

    private States GetInteractionState()
    {
        switch (Type)
        {
            case AreaType.concert:
                return States.watch_concert_positive;
            case AreaType.food:
                return States.eat;
            case AreaType.open_space:
                return States.sit;
        }

        return default;
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

    public virtual Vector3 GetPositionInArea(AgentEntity forE)
    {
        float x;
        float z;
        Vector3 pos;

        int maxIterations = 30;
        int i = 0;

        do
        {

            x = Random.Range(-_ground.localScale.x / 2, _ground.localScale.x / 2);
            z = Random.Range(-_ground.localScale.z / 2, _ground.localScale.z / 2);

            x += _ground.transform.position.x;
            z += _ground.transform.position.z;
            pos = new Vector3(x, 0, z);
            i++;
            if (i == maxIterations)
                pos = EntireField.GetRandomAreaOfType(Type, this).GetPositionInArea(forE);
        } while (!ValidatePosition(pos));
        return pos;
    }

    protected bool ValidatePosition(Vector3 pos)
    {
        return !Physics.CheckSphere(pos, 0.7f, _obstructionLayers);
    }

    private bool IsAgent(Collider other) => other.tag == AgentEntity.TAG;
    private AgentEntity GetAgentComponent(Collider other) => other.GetComponent<AgentEntity>();

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.black;
        Gizmos.DrawWireCube(_ground.transform.position, _ground.localScale);
    }
}
