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

    [SerializeField] private float _tickInterval = 5;
    [SerializeField] private float _tickCountdown;

    private void Awake()
    {
        Agents = new Dictionary<int, AgentEntity>(100);
        PopulatorInstance = GameObject.FindObjectOfType<Populator>();

        ExplosionManager.OnExplosion += ExplosionOccurrence;
        ExplosionManager.OnExplosionWearOff += ExplosionWoreOff;

        _movementPriority = PopulatorInstance.SpawnAmount;
        _tickCountdown = _tickInterval;
        OnTick += () => _tickCountdown = _tickInterval;
        _instance = this;
    }

    private void Start()
    {
        PopulatorInstance.StartPopulating(this);
    }

    private void Update()
    {
        _tickCountdown -= Time.deltaTime;
        if (_tickCountdown <= 0)
            OnTick.Invoke();
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
        NavMesh.SetAreaCost(AgentEntity.OFFROAD_AREA_MASK, 1f);
    }
    private void ExplosionWoreOff()
    {
        NavMesh.SetAreaCost(AgentEntity.OFFROAD_AREA_MASK, 3f);
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

    // Agent action heuristic
    public static States WhatToDo(AgentEntity agent)
    {
        // Agent prioritizes resting, then food
        // If both resting and food are above 0.9, goes eat
        // eating restores rest and food

        // While watching concert, boredom will drop to zero and then to 0.5
        // when watching concert and boredom is above 0.5, move to other concert
        States s = agent.State;
        AgentStats sts = agent.Stats;

        switch (agent.State)
        {
            case States.move_to_concert:
                if (agent.CurrentArea != default)
                    if (agent.CurrentArea.Type == AreaType.concert)
                        s = States.watch_concert_positive;
                break;
            case States.move_to_food:
                if (agent.CurrentArea != default)
                    if (agent.CurrentArea.Type == AreaType.food)
                        s = States.eat;
                break;
            case States.move_to_open_space:
                if (agent.CurrentArea != default)
                    if (agent.CurrentArea.Type == AreaType.open_space)
                        s = States.sit;
                break;
            case States.watch_concert_positive:
            case States.watch_concert_negative:
            case States.eat:
            case States.sit:
            case States.none:
            default:

                // Keep resting/eating until certain threshold
                if ((s == States.eat && sts.Hunger >= 0.1f) || // eat until full
                (s == States.eat && sts.Tiredness >= 0.35f)|| // rest a bit while eating
                (s == States.sit && sts.Tiredness >= 0.15f && sts.Hunger <= 0.9))  // rest until rested || move to food
                    break;

                // Agent wants to do something else?
                if (sts.Hunger > 0.8 && sts.Tiredness > 0.8 && s != States.eat)
                    s = States.move_to_food;
                else if (sts.Tiredness > 0.9f && s != States.sit)
                    s = States.move_to_open_space;
                else if (sts.Hunger > 0.9f && s != States.eat)
                    s = States.move_to_food;
                else if (sts.Boredom > 0.5f && s != States.watch_concert_positive)
                    s = States.move_to_concert;
                else if (sts.Boredom <= 0.001f && s != States.watch_concert_negative)
                    s = States.watch_concert_negative;
                break;
            case States.panicking:
            case States.explosion_victim:
                break;
        }
        return s;
    }

    public static System.Action OnTick;

}
