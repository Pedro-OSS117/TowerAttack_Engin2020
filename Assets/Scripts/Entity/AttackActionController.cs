using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackActionController : ActionController
{
    [SerializeField]
    private AttackActionData _attackActionData;

    [SerializeField]
    private EntityController _currentTarget;

    private int _maskLayer = 0;

    private EntityController _currentEntity;

    public GameObject prefabAttack;
    public Transform originAttack;

    public override ActionData GetData()
    {
        return _attackActionData;
    }

    public override void Awake()
    {
        base.Awake();

        _currentEntity = GetComponent<EntityController>();

        _maskLayer = LayerMask.GetMask("Building");
        if (_attackActionData.AttackUnit)
        {
            _maskLayer |= LayerMask.GetMask("Unit");
        }
    }

    public override void UpdateAction()
    {
        // S'il y a une target, on test si la target est toujours à bonne distance pr agir
        if(_currentTarget != null)
        {
            if (Vector3.Distance(originAttack.position, _currentTarget.transform.position) > _attackActionData.RangeDo)
            {
                _currentTarget = null;
            }
        }


        // Si pas de target on cherche une nouvelle target
        if (_currentTarget == null)
        {
            RaycastHit[] hits = Physics.CapsuleCastAll(originAttack.position, originAttack.position, _attackActionData.RangeDo, Vector3.up, 0, _maskLayer);

            EntityController newTarget = null;

            foreach(RaycastHit hit in hits)
            {
                // Test is pas lui même
                if (hit.transform == transform)
                {
                    continue;
                }

                EntityController toTestNewTarget = hit.transform.GetComponent<EntityController>();

                // Test si bien une entity
                if(toTestNewTarget == null)
                {
                    continue;
                }
                
                // Test l'alignement
                if (_currentEntity.Alignment == toTestNewTarget.Alignment)
                {
                    continue;
                }

                // test si pas encore de nouvelle target
                if(newTarget == null)
                {
                    newTarget = toTestNewTarget;
                    continue;
                }

                // Test si c'est l'entité la plus proche
                if(Vector3.Distance(originAttack.position, toTestNewTarget.transform.position) 
                    < Vector3.Distance(originAttack.position, newTarget.transform.position))
                {
                    newTarget = toTestNewTarget;
                    continue;
                }
            }

            _currentTarget = newTarget;
        }


        // Si target on update l'action
        if (_currentTarget != null)
        {
            base.UpdateAction();
        }
        else
        {
            ResetAction();
        }
    }

    protected override void DoAction()
    {
        if(_currentTarget != null)
        {
            if(prefabAttack != null)
            {
                GameObject newProjectile = Instantiate(prefabAttack);

                newProjectile.transform.position = originAttack.position;

                Projectile projectileCompo = newProjectile.GetComponent<Projectile>();

                if(projectileCompo)
                {
                    projectileCompo.InitTarget(_currentTarget);
                    projectileCompo.damage = _attackActionData.Damage;
                }
            }
            else
            {
                _currentTarget.ApplyDamage(_attackActionData.Damage);
            }
        }

        ResetAction();
    }

    private void OnDrawGizmos()
    {
        if(_attackActionData != null)
        {
            Gizmos.DrawWireSphere(originAttack.position, _attackActionData.RangeDo);
        }
    }
}
