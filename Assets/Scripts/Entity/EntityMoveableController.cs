using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EntityMoveableController : EntityController
{
    private EntityMoveableData _datasMove;

    private NavMeshAgent _navMeshAgent;

    public EntityController currentMoveTarget;
    public EntityController globalMoveTarget;

    private List<AttackActionController> _attackActions;

    protected override void Awake()
    {
        base.Awake();
        _datasMove = (EntityMoveableData)Datas;

        _navMeshAgent = GetComponent<NavMeshAgent>();
        _navMeshAgent.speed = _datasMove.Speed;

        _attackActions = new List<AttackActionController>();
        foreach(ActionController action in actionControllers)
        {
            if(action is AttackActionController attackAction)
            {
                attackAction.drawDetect = true;
                _attackActions.Add(attackAction);
            }
        }
    }

    protected override void Update()
    {
        currentMoveTarget = GetCurrentMoveTarget();

        if(currentMoveTarget != null)
        {
            _navMeshAgent.isStopped = false;
            _navMeshAgent.stoppingDistance = GetMinRangeDo();
            _navMeshAgent.SetDestination(currentMoveTarget.transform.position);
        }
        else
        {
            _navMeshAgent.isStopped = true;
        }

        base.Update();
    }

    private float GetMinRangeDo()
    {
        float range = -1;
        foreach (AttackActionController action in _attackActions)
        {
            if(range == -1 || range < action.AttackActionData.RangeDo)
            {
                range = action.AttackActionData.RangeDo;

            }
        }
        return range;
    }

    private EntityController GetCurrentMoveTarget()
    {
        foreach (AttackActionController action in _attackActions)
        {
            EntityController entity = action.DetectEntity();
            if(entity)
            {
                return entity;
            }
        }

        return globalMoveTarget;
    }
}
