using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentStats
{
    public float Hunger { get; private set; }
    public float Tiredness { get; private set; }
    public float Boredom { get; private set; }

    private float _hungerMultiplier;
    private float _tirednessMultiplier;
    private float _boredomMultiplier;

    public AgentStats()
    {
        Hunger = Random.value;
        Tiredness = Random.value;
        Boredom = Random.value;

        _hungerMultiplier = Random.Range(0.5f, 1.5f);
        _tirednessMultiplier = Random.Range(0.5f, 1.5f);
        _boredomMultiplier = Random.Range(0.5f, 1.5f);
    }

    public void IncreaseHunger() => Hunger = Mathf.Clamp01(Hunger + 0.1f * _hungerMultiplier);
    public void IncreaseTiredness() => Tiredness = Mathf.Clamp01(Tiredness + 0.1f * _tirednessMultiplier);
    public void IncreaseBoredom() => Boredom = Mathf.Clamp01(Boredom + 0.1f * _boredomMultiplier);

    public void DecreaseHunger() => Hunger = Mathf.Clamp01(Hunger + 0.1f * _hungerMultiplier);
    public void DecreaseTiredness() => Tiredness = Mathf.Clamp01(Tiredness + 0.1f * _tirednessMultiplier);
    public void DecreaseBoredom() => Boredom = Mathf.Clamp01(Boredom + 0.1f * _boredomMultiplier);
}
