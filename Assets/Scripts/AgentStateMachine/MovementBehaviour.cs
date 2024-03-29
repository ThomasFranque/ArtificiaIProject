using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace AgentStateMachine
{
    public class MovementBehaviour : AgentState
    {
        private Vector3 _targetPos;

        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);
            _targetPos = _agent.DesiredPosition;
            _agent?.NavMeshAgent.SetDestination(_targetPos);
            //_agent.SetAvoidancePriority(AgentBrain.AgentInMotion());
        }

        // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (_agent.State == States.panicking || _agent.State == States.explosion_victim)
            {
                if (_targetPos != _agent.DesiredExitPosition)
                {
                    _targetPos = _agent.DesiredExitPosition;
                    _agent?.NavMeshAgent.SetDestination(_targetPos);
                }
            }
            if (Vector3.Distance(animator.transform.position, _targetPos) <= 1.25f)
            {
                animator.SetTrigger("arrived");
            }
        }

        // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            //_agent.ResetAvoidancePriority();
            AgentBrain.AgentReachedDestination();
        }

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
}