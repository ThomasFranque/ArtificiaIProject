using System;
using UnityEngine;
using UnityEngine.AI;
using TMPro;

[RequireComponent(typeof(NavMeshAgent))]
public class AgentEntity : MonoBehaviour
{
    public const string ID_STRING = "id";
    public const string TAG = "Player";
    public const int DEFAULT_AREA_MASK = 9;
    public const int PANIC_AREA_MASK = 9;
    public const int OFFROAD_AREA_MASK = 8;

    private Animator _stateMachineAnim;
    private Transform _initialParent;
    private int _othersAmount;

    [SerializeField] private TextMeshPro _infoTxt;

    [field: SerializeField] public int ID { get; private set; }
    [field: SerializeField] public RuntimeAnimatorController StateMachineController { get; private set; }
    [field: SerializeField] public States State { get; private set; }
    [field: SerializeField] public AgentStats Stats { get; private set; }
    public Animator StateMachine => _stateMachineAnim;
    public NavMeshAgent NavMeshAgent { get; private set; }
    public Vector3 DesiredPosition { get; private set; }
    public Vector3 DesiredExitPosition { get; private set; }
    public Area CurrentArea { get; private set; }

    public void Initialize(int id, int othersAmount)
    {
        Stats = new AgentStats();
        Stats.Initialize();
        NavMeshAgent = GetComponent<NavMeshAgent>();
        _stateMachineAnim = gameObject.AddComponent<Animator>();

        ID = id;
        _othersAmount = othersAmount;
        _initialParent = transform.parent;

        SetAvoidancePriority(othersAmount);
        StartCoroutine(DelayBeforeMachineInitialization());
    }

    public void EnteredArea(Area area)
    {
        CurrentArea = area;
        OnAreaEntered?.Invoke(area);
    }

    public void LeftArea(Area area)
    {
        CurrentArea = default;
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
                State = States.explosion_victim;
                break;
            case ExplosionRadiusType.outskirts:
                if (State == States.explosion_victim) break;
                State = States.panicking;
                break;
        }

        NotifyStateMachine();
    }

    public void ForceChangeState(States state)
    {
        if (state == State) return;
        Debug.Log("NewState:" + state.ToString());
        State = state;
        NotifyStateMachine();
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
    public void SetDesiredExitPosition(Vector3 pos)
    {
        DesiredExitPosition = pos;
        Debug.DrawRay(pos + new Vector3(0, 2, 0), Vector3.down * 3, Color.red, 1f);
    }
    public void ResetAvoidancePriority()
    {
        NavMeshAgent.avoidancePriority = _othersAmount;
    }

    public void Kill()
    {
        OnKill?.Invoke(ID);
        Destroy(gameObject);
    }
    public void ExitField()
    {
        OnExitField?.Invoke();
        Destroy(gameObject);
    }

    public void AskBrain()
    {
        States prevState = State;
        State = AgentBrain.WhatToDo(this);
        switch (State)
        {
            case States.move_to_concert:
                DesiredPosition = EntireField
                .GetRandomAreaOfType(AreaType.concert).GetPositionInArea(this);
                break;
            case States.move_to_food:
                DesiredPosition = EntireField
                .GetRandomAreaOfType(AreaType.food).GetPositionInArea(this);
                break;
            case States.move_to_open_space:
                DesiredPosition = EntireField
                .GetRandomAreaOfType(AreaType.open_space).GetPositionInArea(this);
                break;
        }

        if (State != prevState)
        {
            if (prevState == States.sit)
            {
                OnGetUp?.Invoke();
                OnGetUp = default;
            }
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
                StateMachine.SetTrigger("explosion");
                break;
        }

        string infoTxt = State.ToString() +
            "\nBored:" + Stats.Boredom +
            "\nHunger:" + Stats.Hunger +
            "\nTired:" + Stats.Tiredness;
        if (CurrentArea != default)
            infoTxt += "\nArea:" + CurrentArea.Type;
        _infoTxt.SetText(infoTxt);
    }

    public void InteractWithArea()
    {
        States? s;
        s = CurrentArea?.Interact(this);
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
                Stats.IncreaseTiredness();
                Stats.IncreaseBoredom(0.6f);
                Stats.IncreaseHunger();
                break;
            case States.watch_concert_positive:
                Stats.DecreaseBoredom(1.2f);
                Stats.IncreaseTiredness(.8f);
                Stats.IncreaseHunger();
                break;
            case States.watch_concert_negative:
                Stats.IncreaseTiredness(.9f);
                Stats.IncreaseBoredom(.8f);
                Stats.IncreaseHunger();
                break;
            case States.eat:
                Stats.DecreaseTiredness(.8f);
                Stats.DecreaseHunger();
                Stats.IncreaseBoredom();
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
    public System.Action OnExitField;
    public System.Action OnGetUp;
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
