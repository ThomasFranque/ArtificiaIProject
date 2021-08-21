using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DeathCounter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _valueTxt;
    [SerializeField] private int _deathCount;
    private void Start()
    {
        AgentEntity.CommonOnKill += Increment;
        _deathCount = 0;
        _valueTxt.SetText("0");
    }

    private void Increment(int killedAgentId)
    {
        _deathCount++;
        _valueTxt.SetText(_deathCount.ToString());
    }
}
