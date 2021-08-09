using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodArea : Area
{
    private Dictionary<Transform, AgentEntity> _takenSeats;
    [SerializeField] private Transform _chairFather;

    protected override void Awake()
    {
        base.Awake();
        _takenSeats = new Dictionary<Transform, AgentEntity>(_chairFather.childCount);
    }
    public override States Interact(AgentEntity agent)
    {
        return base.Interact(agent);
    }

    private Transform GetFreeChair()
    {
        foreach (Transform t in _chairFather)
        {
            if (!_takenSeats.ContainsKey(t))
            {
                t.SetAsLastSibling();
                return t;
            }
        }
        return default;
    }

    public override Vector3 GetPositionInArea(AgentEntity forE, float vr)
    {
        Transform chair = GetFreeChair();
        if (chair == default)
            return base.GetPositionInArea(forE);

        _takenSeats.Add(chair, forE);
        forE.OnGetUp += () => _takenSeats.Remove(chair);
        return chair.position;
    }
}
