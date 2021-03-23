using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityController : MonoBehaviour
{
    [Header("Entity Properties")]
#pragma warning disable 0649
    [SerializeField]
    private EntityData _datas;
#pragma warning restore 0649

    public EntityData Datas { get { return _datas; } }
    
    [SerializeField]
    private int _currentLife = 0;

    protected ActionController[] actionControllers;

#pragma warning disable 0649
    [SerializeField]
    private Alignment _alignment;
#pragma warning restore 0649

    public Alignment Alignment { get { return _alignment; } }

    public virtual void Awake()
    {
        _currentLife = _datas.Life;
        actionControllers = GetComponents<ActionController>();

        ValidateActionControllers();
    }

    private void ValidateActionControllers()
    {
        if (actionControllers != null)
        {
            foreach (ActionData actionData in _datas.Actions)
            {
                bool finded = false;
                foreach (ActionController actionController in actionControllers)
                {
                    if (actionData == actionController.GetData())
                    {
                        finded = true;
                        break;
                    }
                }

                if (!finded)
                {
                    Debug.LogError("No Controller for action data : " + actionData.name, gameObject);
                }
            }
        }
    }

    public virtual void Update()
    {
        if (actionControllers != null)
        {
            foreach (ActionController actionController in actionControllers)
            {
                actionController.UpdateAction();
            }
        }
    }

    public void ApplyDamage(int damage)
    {
        _currentLife -= damage;

        if(_currentLife <= 0)
        {
            EntityManager.Instance.DestroyEntity(gameObject);
        }
    }

    public bool IsValidEntity()
    {
        return gameObject != null && gameObject.activeSelf && _currentLife > 0;
    }
}
