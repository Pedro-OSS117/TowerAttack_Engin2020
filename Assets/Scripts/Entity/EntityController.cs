using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityController : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField]
    private EntityData _datas;

    public EntityData Datas { get { return _datas; } }

    [SerializeField]
    private Alignment _alignment;
    public Alignment Alignment { get { return _alignment; } }
#pragma warning restore 0649

    [SerializeField]
    private int _currentLife = 0;

    protected ActionController[] actionControllers = null;

    protected virtual void Awake()
    {
        _currentLife = _datas.Life;
        InitActions();
    }

    private void InitActions()
    {
        actionControllers = GetComponents<ActionController>();

        // Verification de la presence des controllers
        foreach (ActionData actionData in _datas.Actions)
        {
            bool finded = false;
            foreach(ActionController actionController in actionControllers)
            {
                if(actionController.GetActionData() == actionData)
                {
                    finded = true;
                    break;
                }
            }
            if(!finded)
            {
                Debug.LogError("No Controller for " + actionData.name, actionData);
            }
        }
    }

    protected virtual void Update()
    {
        foreach(ActionController actionController in actionControllers)
        {
            actionController.UpdateAction();
        }
    }

    public void ApplyDamage(int damage)
    {
        _currentLife -= damage;
        if (_currentLife <= 0)
        {
            // Entity Die
            //EntityManager.Instance.PoolElement(gameObject);
            Destroy(gameObject);
        }
    }

    public bool IsValidEntity()
    {
        return gameObject != null && gameObject.activeSelf && _currentLife > 0;
    }
}
