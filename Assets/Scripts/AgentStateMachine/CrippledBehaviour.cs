using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrippledBehaviour : AgentState
{
    private bool _doDeathTimer;
    private float _deathTimer;
    private float _paralyzeTimer;
    private float _crippledSpeed;
    private Vector3 _closestExit;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);

        // Timers
        _paralyzeTimer = Random.value * 3 + 1;
        _deathTimer = Random.value * 5 + 5 + _paralyzeTimer;
        _doDeathTimer = Random.value >= 0.5f;

        // Speeds
        _crippledSpeed = _agent.NavMeshAgent.speed + Random.value * 0.4f + 0.1f;
        _agent.NavMeshAgent.speed = 0;

        // Exit
        _closestExit = EntireField.GetClosestExit(_agent.transform.position);
        _agent.SetDesiredExitPosition(_closestExit);
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _paralyzeTimer -= Time.deltaTime;
        _deathTimer -= Time.deltaTime;

        if (_paralyzeTimer <= 0)
            _agent.NavMeshAgent.speed = _crippledSpeed;

        if (_doDeathTimer && _deathTimer <= 0)
            _agent.Kill();
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
