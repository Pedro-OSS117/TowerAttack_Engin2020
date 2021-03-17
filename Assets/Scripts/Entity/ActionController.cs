using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ActionController : MonoBehaviour
{
    [SerializeField]
    protected float _currentTimeBeforeNextDo = 0;
    protected float _timeBeforeNextDo = 0;

    public virtual void Awake()
    {
        _timeBeforeNextDo = GetData().TimeToDoAction;
    }

    public abstract ActionData GetData();

    public virtual void UpdateAction()
    {
        if(_currentTimeBeforeNextDo < _timeBeforeNextDo)
        {
            _currentTimeBeforeNextDo += Time.deltaTime;
        }
        else
        {
            DoAction();
        }
    }

    protected abstract void DoAction();

    protected virtual void ResetAction()
    {
        _currentTimeBeforeNextDo = 0;
    }
}
