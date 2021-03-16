using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ActionController : MonoBehaviour
{
    [SerializeField]
    protected float _currentTimeElapsedBeforeNextDo = 0;
    protected float _timeBeforeNexDo = 0;

    public void Awake()
    {
        InitAction();
    }

    public abstract ActionData GetActionData();

    public abstract void InitAction();

    public virtual void ResetAction()
    {
        _currentTimeElapsedBeforeNextDo = 0;
    }

    public virtual void UpdateAction()
    {
        if(_currentTimeElapsedBeforeNextDo < _timeBeforeNexDo)
        {
            _currentTimeElapsedBeforeNextDo += Time.deltaTime;
        }
        else
        {
            DoAction();
        }
    }

    public abstract void DoAction();
}
