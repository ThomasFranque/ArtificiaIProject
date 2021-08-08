using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AgentStats
{
    private const float TICK_VALUE = 0.05f;

    [field: SerializeField]
    public float Hunger { get; private set; }
    [field: SerializeField]
    public float Tiredness { get; private set; }
    [field: SerializeField]
    public float Boredom { get; private set; }

    [Space]
    [SerializeField]
    private float _hungerMultiplier;
    [SerializeField]
    private float _tirednessMultiplier;
    [SerializeField]
    private float _boredomMultiplier;

    public AgentStats()
    {
    }

    public void Initialize()
    {
        Hunger = Random.value;
        Tiredness = Random.value;
        Boredom = 1;

        _hungerMultiplier = Random.Range(0.5f, 1.5f);
        _tirednessMultiplier = Random.Range(0.5f, 1.5f);
        _boredomMultiplier = Random.Range(0.5f, 1.5f);
    }

    public void IncreaseHunger(float mult = 1) => Hunger = Mathf.Clamp01(Hunger + TICK_VALUE * _hungerMultiplier * mult);
    public void IncreaseTiredness(float mult = 1) => Tiredness = Mathf.Clamp01(Tiredness + TICK_VALUE * _tirednessMultiplier * mult);
    public void IncreaseBoredom(float mult = 1) => Boredom = Mathf.Clamp01(Boredom + TICK_VALUE * _boredomMultiplier * mult);

    public void DecreaseHunger(float mult = 1) => Hunger = Mathf.Clamp01(Hunger + TICK_VALUE * _hungerMultiplier * mult);
    public void DecreaseTiredness(float mult = 1) => Tiredness = Mathf.Clamp01(Tiredness + TICK_VALUE * _tirednessMultiplier * mult);
    public void DecreaseBoredom(float mult = 1) => Boredom = Mathf.Clamp01(Boredom + TICK_VALUE * _boredomMultiplier * mult);
}
