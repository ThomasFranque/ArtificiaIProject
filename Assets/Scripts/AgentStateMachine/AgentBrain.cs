using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AgentBrain : MonoBehaviour
{
    private static AgentBrain _instance;
    private int _killedAgents;
    private int _movementPriority;
    private Dictionary<int, AgentEntity> Agents { get; set; }
    private Populator PopulatorInstance { get; set; }

    private void Awake()
    {
        Agents = new Dictionary<int, AgentEntity>(100);
        PopulatorInstance = GameObject.FindObjectOfType<Populator>();

        ExplosionManager.OnExplosion += ExplosionOccurrence;
        ExplosionManager.OnExplosionWearOff += ExplosionWoreOff;

        _movementPriority = PopulatorInstance.SpawnAmount;

        _instance = this;
    }

    private void Start()
    {
        PopulatorInstance.StartPopulating(this);
    }

    public int RegisterNewAgent(AgentEntity agent)
    {
        int id;
        id = GenerateID();
        Agents.Add(id, agent);
        agent.Initialize(id, PopulatorInstance.SpawnAmount);
        agent.OnKill += RemoveAgent;
        return id;
    }
    public void RemoveAgent(int id)
    {
        if (Agents.ContainsKey(id))
            Agents.Remove(id);
    }
    public static bool TryGetAgent(int id, out AgentEntity entity)
    {
        return _instance.Agents.TryGetValue(id, out entity);
    }

    private int GenerateID()
    {
        int rnd;
        do
        {
            rnd = Random.Range(int.MinValue, int.MaxValue);
        } while (Agents.ContainsKey(rnd));
        return rnd;
    }

    private void ExplosionOccurrence(int amountKilled)
    {
        _killedAgents += amountKilled;
        Debug.Log(amountKilled + " Killed");
        NavMesh.SetAreaCost(AgentEntity.OFFROAD_AREA_MASK, 0.8f);
    }
    private void ExplosionWoreOff()
    {
        NavMesh.SetAreaCost(AgentEntity.OFFROAD_AREA_MASK, 2f);
    }

    // Give priority
    public static int AgentInMotion()
    {
        _instance._movementPriority--;
        return _instance._movementPriority;
    }
    public static void AgentReachedDestination()
    {
        _instance._movementPriority++;
    }

    public States WhatToDo(AgentEntity agent)
    {
        return default;
    }

    public static System.Action OnTick;

}
