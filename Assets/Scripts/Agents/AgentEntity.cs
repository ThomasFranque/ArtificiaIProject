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
    public AgentStats Stats { get; private set; }
    public NavMeshAgent NavMeshAgent { get; private set; }

    public void Initialize(int id, int othersAmount)
    {
        NavMeshAgent = GetComponent<NavMeshAgent>();
        _stateMachineAnim = GetComponent<Animator>();

        ID = id;
        _othersAmount = othersAmount;

        SetAvoidancePriority(othersAmount);
        _stateMachineAnim.SetInteger(ID_STRING, id);
        StartCoroutine(AAAAA());
    }
    
    public void EnteredArea(Area area)
    {
        _currentArea = area;
    }

    public void LeftArea(Area area)
    {
        _currentArea = default;
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
    public void ResetAvoidancePriority()
    {
        NavMeshAgent.avoidancePriority = _othersAmount;
    }

    private void Kill()
    {
        OnKill?.Invoke(ID);
        Destroy(gameObject);
    }

    public System.Action<int> OnKill; // int = ID

    System.Collections.IEnumerator AAAAA()
    {
        NavMeshAgent.enabled = false;
        yield return new WaitForSeconds(0.1f);
        NavMeshAgent.enabled = true;
    }
}
