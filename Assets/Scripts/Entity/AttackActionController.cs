using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackActionController : ActionController
{
#pragma warning disable 0649
    [SerializeField]
    private AttackActionData _attackActionData;
    public AttackActionData AttackActionData { get { return _attackActionData; } }

    [SerializeField]
    private GameObject prefabAttack;
#pragma warning restore 0649

    private EntityController _currentTarget;
    private int _maskLayer;

    public Transform originAttack;

    public bool drawDetect = false;

    public override ActionData GetActionData()
    {
        return _attackActionData;
    }
    
    public override void InitAction()
    {
        _timeBeforeNexDo = _attackActionData.TimeToDoAction;

        _maskLayer = LayerMask.GetMask("Building");
        if(_attackActionData.AttackUnit)
        {
            _maskLayer |= LayerMask.GetMask("Unit");
        }

        if(originAttack == null)
        {
            Debug.LogWarning("NoOriginAttack");
            originAttack = transform;
        }
    }

    public override void UpdateAction()
    {
        // S'il y a une target, on test si la target est toujours à bonne distance pr agir
        if(_currentTarget != null)
        {
            if(!(Vector3.Distance(_currentTarget.transform.position, originAttack.position) <= _attackActionData.RangeDo))
            {
                // On annule la target courante si elle est plus assez pres
                _currentTarget = null;
            }
        }

        // Si pas de target on cherche une nouvelle target
        if (_currentTarget == null)
        {
            RaycastHit[] hits = Physics.CapsuleCastAll(originAttack.position, originAttack.position, _attackActionData.RangeDo, transform.up, 0, _maskLayer);
            EntityController newTarget = null;
            foreach (RaycastHit hit in hits)
            {
                EntityController toTest = hit.transform.GetComponent<EntityController>();
                if (toTest != null && hit.transform != transform)
                {
                    if (transform.GetComponent<EntityController>().Alignment != toTest.Alignment)
                    {
                        if (newTarget)
                        {
                            if (Vector3.Distance(hit.transform.position, originAttack.position) < Vector3.Distance(newTarget.transform.position, originAttack.position))
                            {
                                newTarget = toTest;
                            }
                        }
                        else
                        {
                            newTarget = toTest;
                        }
                    }
                }
            }
            _currentTarget = newTarget;
        }

        if(_currentTarget != null)
        {
            base.UpdateAction();
        }
        else
        {
            ResetAction();
        }
    }

    public EntityController DetectEntity()
    {
        RaycastHit[] hits = Physics.CapsuleCastAll(originAttack.position, originAttack.position, _attackActionData.RangeDetect, transform.up, 0, _maskLayer);
        foreach (RaycastHit hit in hits)
        {
            EntityController toTest = hit.transform.GetComponent<EntityController>();
            if (toTest != null && hit.transform != transform)
            {
                if (transform.GetComponent<EntityController>().Alignment != toTest.Alignment)
                {
                    return toTest;
                }
            }
        }
        return null;
    }

    public override void DoAction()
    {
        // Create Projectile
        if (prefabAttack)
        {
            GameObject newProjectile = Instantiate(prefabAttack);

            newProjectile.transform.position = originAttack.position;

            Projectile projectileCompo = newProjectile.GetComponent<Projectile>();

            projectileCompo.InitTarget(_currentTarget);

            projectileCompo.rangeHit = 0.1f;
            projectileCompo.damage = _attackActionData.Damage;
        }
        else
        {
            // Sinon on applique les dommages directement
            _currentTarget.ApplyDamage(_attackActionData.Damage);
        }

        // Reset Action
        ResetAction();
    }

    public void OnDrawGizmos()
    {
        if(originAttack && _attackActionData)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(originAttack.position, _attackActionData.RangeDo);

            if (drawDetect)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(originAttack.position, _attackActionData.RangeDetect);
            }
        }
    }
}
