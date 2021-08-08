using System;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AgentEntity : MonoBehaviour
{
    public const string ID_STRING = "id";
    public const string TAG = "Player";
    public const int DEFAULT_AREA_MASK = 9;
    public const int PANIC_AREA_MASK = 9;
    public const int OFFROAD_AREA_MASK = 8;

    private Animator _stateMachineAnim;
    private Area _currentArea;

    private int _othersAmount;


    [field: SerializeField]
    public int ID { get; private set; }
    [field: SerializeField]
    public RuntimeAnimatorController StateMachineController { get; private set; }
    [field: SerializeField]
    public States State { get; private set; }
    [field: SerializeField]
    public AgentStats Stats { get; private set; }
    public Animator StateMachine => _stateMachineAnim;
    public NavMeshAgent NavMeshAgent { get; private set; }
    public Vector3 DesiredPosition { get; private set; }

    public void Initialize(int id, int othersAmount)
    {
        Stats = new AgentStats();
        Stats.Initialize();
        NavMeshAgent = GetComponent<NavMeshAgent>();
        _stateMachineAnim = gameObject.AddComponent<Animator>();

        ID = id;
        _othersAmount = othersAmount;

        SetAvoidancePriority(othersAmount);
        StartCoroutine(DelayBeforeMachineInitialization());
    }

    public void EnteredArea(Area area)
    {
        _currentArea = area;
        OnAreaEntered?.Invoke(area);
    }

    public void LeftArea(Area area)
    {
        _currentArea = default;
        OnAreaLeft?.Invoke(area);
    }

    public void ExplosionVictim(ExplosionRadiusType radius)
    {
        switch (radius)
        {
            case ExplosionRadiusType.point_blank:
                Kill();
                break;
            case ExplosionRadiusType.middle:
                NavMeshAgent.speed = NavMeshAgent.speed * 2;
                break;
            case ExplosionRadiusType.outskirts:
                break;
        }
    }

    // priority = 50
    public void SetAvoidancePriority(int priority)
    {
        NavMeshAgent.avoidancePriority = priority;
    }
    public void SetDesiredPosition(Vector3 pos)
    {
        DesiredPosition = pos;
        Debug.DrawRay(pos + new Vector3(0, 2, 0), Vector3.down * 3, Color.red, 1f);
    }
    public void ResetAvoidancePriority()
    {
        NavMeshAgent.avoidancePriority = _othersAmount;
    }

    private void Kill()
    {
        OnKill?.Invoke(ID);
        Destroy(gameObject);
    }

    public void AskBrain()
    {
        State = AgentBrain.WhatToDo(this);
        switch (State)
        {
            case States.move_to_concert:
                DesiredPosition = EntireField
                .GetRandomAreaOfType(AreaType.concert).GetPositionInArea();
                break;
            case States.move_to_food:
                DesiredPosition = EntireField
                .GetRandomAreaOfType(AreaType.food).GetPositionInArea();
                break;
            case States.move_to_open_space:
                DesiredPosition = EntireField
                .GetRandomAreaOfType(AreaType.open_space).GetPositionInArea();
                break;
        }
        NotifyStateMachine();
    }

    private void NotifyStateMachine()
    {
        switch (State)
        {
            case States.move_to_concert:
            case States.move_to_food:
            case States.move_to_open_space:
                StateMachine.SetTrigger("on_the_move");
                break;
            case States.watch_concert_positive:
            case States.watch_concert_negative:
            case States.eat:
            case States.sit:
            default:
                StateMachine.SetTrigger("stay_in_place");
                break;
            case States.panicking:
            case States.explosion_victim:
                break;
        }
    }

    public void InteractWithArea()
    {
        States? s;
         s = _currentArea?.Interact(this);
         if (s != null)
            State = s.Value;
    }

    private void Tick()
    {
        switch (State)
        {
            case States.move_to_concert:
            case States.move_to_food:
            case States.move_to_open_space:
                Stats.DecreaseTiredness();
                Stats.IncreaseBoredom(0.6f);
                Stats.IncreaseHunger();
                break;
            case States.watch_concert_positive:
                Stats.DecreaseBoredom(1.2f);
                Stats.IncreaseTiredness(.8f);
                Stats.IncreaseHunger();
                break;
            case States.watch_concert_negative:
                Stats.IncreaseTiredness(.8f);
                Stats.IncreaseBoredom(.8f);
                Stats.IncreaseHunger();
                break;
            case States.eat:
                Stats.DecreaseTiredness(.8f);
                Stats.IncreaseBoredom();
                Stats.IncreaseHunger();
                break;
            case States.sit:
                Stats.DecreaseTiredness();
                Stats.IncreaseBoredom();
                Stats.IncreaseHunger(.6f);
                break;
            default:
                break;
            case States.panicking:
                break;
            case States.explosion_victim:
                break;
        }
    }

    // Navmesh agent does not behave well right after instantiation, wait a
    // bit before starting to behave.
    private System.Collections.IEnumerator DelayBeforeMachineInitialization()
    {
        NavMeshAgent.enabled = false;
        StateMachine.enabled = false;
        yield return new WaitForSeconds(0.1f);
        StateMachine.runtimeAnimatorController = StateMachineController;
        StateMachine.SetInteger(ID_STRING, ID);
        NavMeshAgent.enabled = true;
        StateMachine.enabled = true;
    }

    public System.Action<int> OnKill; // int = ID
    private event Action<Area> OnAreaEntered;
    private event Action<Area> OnAreaLeft;

    private void OnEnable()
    {
        AgentBrain.OnTick += Tick;
    }

    private void OnDisable()
    {
        AgentBrain.OnTick -= Tick;
    }
}
