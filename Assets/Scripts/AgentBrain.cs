using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public static class AgentBrain
{
    private static Dictionary<int, AgentEntity> Agents { get; set; }
    private static Populator PopulatorInstance { get; set; }
    private static int _killedAgents;
    private static int _movementPriority;

    static AgentBrain()
    {
        Agents = new Dictionary<int, AgentEntity>(100);
        PopulatorInstance = GameObject.FindObjectOfType<Populator>();

        ExplosionManager.OnExplosion += ExplosionOccurrence;
        ExplosionManager.OnExplosionWearOff += ExplosionWoreOff;

        _movementPriority = PopulatorInstance.SpawnAmount;
    }

    public static int RegisterNewAgent(AgentEntity agent)
    {
        int id;
        id = GenerateID();
        Agents.Add(id, agent);
        agent.Initialize(id, PopulatorInstance.SpawnAmount);
        return id;
    }
    public static void RemoveAgent(int id)
    {
        if (Agents.ContainsKey(id))
            Agents.Remove(id);
    }
    public static bool TryGetAgent(int id, out AgentEntity entity)
    {
        return Agents.TryGetValue(id, out entity);
    }

    private static int GenerateID()
    {
        int rnd;
        do
        {
            rnd = Random.Range(int.MinValue, int.MaxValue);
        } while (Agents.ContainsKey(rnd));
        return rnd;
    }

    private static void ExplosionOccurrence(int amountKilled)
    {
        _killedAgents += amountKilled;
        Debug.Log(amountKilled + " Killed");
        NavMesh.SetAreaCost(AgentEntity.OFFROAD_AREA_MASK, 0.8f);
    }
    private static void ExplosionWoreOff()
    {
        NavMesh.SetAreaCost(AgentEntity.OFFROAD_AREA_MASK, 2f);        
    }

    // Give priority
    public static int AgentMoving()
    {
        _movementPriority--;
        return _movementPriority;
    }
    public static void AgentStoppedMoving()
    {
        _movementPriority++;
    }

}
