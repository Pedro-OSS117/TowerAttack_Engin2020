using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Invocation", menuName = "TowerAttack/Action/New Invocation")]
public class InvocationActionData : ActionData
{
#pragma warning disable 0649
    [SerializeField]
    private EntityData _entityToInvoc;
    public EntityData EntityToInvoc { get { return _entityToInvoc; } }
#pragma warning restore 0649

    [SerializeField]
    [Min(1)]
    private int _numberToInvoc = 0;
    public int NumberToInvoc { get { return _numberToInvoc; } }
}
