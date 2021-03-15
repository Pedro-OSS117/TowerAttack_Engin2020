using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ActionData
{
    [SerializeField]
    private float _timeToDoAction = 0;

    public float TimeToDoAction { get { return _timeToDoAction; } }
}
