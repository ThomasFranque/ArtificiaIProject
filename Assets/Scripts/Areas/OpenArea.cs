using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenArea : Area
{
    protected override void Awake()
    {
        base.Awake();
    }
    public override States Interact(AgentEntity agent)
    {
        return base.Interact(agent);
    }

    private Vector3 GetOpenestSpace()
    {
        List<AgentEntity> _closestAgentSort = new List<AgentEntity>();

        float longestDist = float.NegativeInfinity;
        Vector3 position = default;
        foreach (KeyValuePair<Collider, AgentEntity> entry in AgentsInArea)
        {
            AgentEntity closestToPivot = default; // Closest to pivot
            AgentEntity pivotAgent = entry.Value; // Pivot agent
            _closestAgentSort.Add(pivotAgent);

            // Loop through all agents to find closest
            float closestDist = float.PositiveInfinity;
            foreach (KeyValuePair<Collider, AgentEntity> subEntry in AgentsInArea)
            {
                AgentEntity curA = entry.Value; // Pivot agent
                if (curA == pivotAgent) continue;

                float dist = Vector3.Distance(pivotAgent.transform.position, curA.transform.position);

                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestToPivot = curA;
                }
                if (dist > longestDist)
                {
                    longestDist = dist;
                    position = curA.transform.position;
                }
            }

            _closestAgentSort.Add(closestToPivot);
        }

        return default;
    }

    public override Vector3 GetPositionInArea(AgentEntity forE, float positionValidateRadius = 0.7f)
    {
        float x;
        float z;
        int maxIterations = 50;
        int i = 0;
        Vector3 pos;

        positionValidateRadius = 2f;
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
        } while (!ValidatePosition(pos, positionValidateRadius - (i * 0.02f)) && i < maxIterations && AgentsInArea.Count < 6);

        if (i >= maxIterations || AgentsInArea.Count >= 6)
            pos = GetOpenestSpace();
        return pos;
    }
}
