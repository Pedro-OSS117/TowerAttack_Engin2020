using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EntityMoveableController : EntityController
{
    private EntityMoveableData _moveableData;

    private NavMeshAgent _navMeshAgent;

    public GameObject globalTarget;

    [SerializeField]
    private GameObject _currentTargetToMove;

    private List<AttackActionController> _attackActionControllers;

    public override void Awake()
    {
        base.Awake();

        if(Datas is EntityMoveableData)
        {
            _moveableData = (EntityMoveableData)Datas;
        }
        else
        {
            Debug.LogError("Not Valid Entity Data - Need Moveable", gameObject);
        }

        _navMeshAgent = GetComponent<NavMeshAgent>();
        _navMeshAgent.speed = _moveableData.Speed;

        _attackActionControllers = new List<AttackActionController>();
        foreach (ActionController actionController in actionControllers)
        {
            if(actionController is AttackActionController)
            {
                _attackActionControllers.Add((AttackActionController)actionController);
            }
        }
    }

    public override void Update()
    {
        // Recuperation d'une destination
        _currentTargetToMove = GetCurrentTarget();

        // Update de la destination
        if(_currentTargetToMove != null)
        {
            _navMeshAgent.isStopped = false;
            _navMeshAgent.stoppingDistance = GetMaxDistanceStop();
            _navMeshAgent.SetDestination(_currentTargetToMove.transform.position);
        }
        else
        {
            _navMeshAgent.isStopped = true;
        }

        base.Update();
    }

    private float GetMaxDistanceStop()
    {
        float maxDistance = -1;
        foreach (AttackActionController attackActionController in _attackActionControllers)
        {
            if(maxDistance == -1 || maxDistance < attackActionController.AttackActionData.RangeDo)
            {
                maxDistance = attackActionController.AttackActionData.RangeDo;
            }
        }
        return maxDistance;
    }

    private GameObject GetCurrentTarget()
    {
        foreach(AttackActionController attackActionController in _attackActionControllers)
        {
            GameObject newTarget = attackActionController.DetectNewTarget();
            if(newTarget)
            {
                return newTarget;
            }
        }

        return globalTarget;
    }
}
