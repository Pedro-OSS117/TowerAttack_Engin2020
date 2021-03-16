using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Attack", menuName = "TowerAttack/Action/New Attack")]
public class AttackActionData : ActionData
{
    [SerializeField]
    private int _damage = 0;
    public int Damage { get { return _damage; } }

    [SerializeField]
    [Min(0)]
    private float _rangeDo = 0;
    public float RangeDo { get { return _rangeDo; } }

    [SerializeField]
    [Min(0)]
    private float _rangeDetect = 0;
    public float RangeDetect { get { return _rangeDetect; } }

    [SerializeField]
    private bool _attackUnit = true;
    public bool AttackUnit { get { return _attackUnit; } }

    [SerializeField]
    private bool _attackFly = false;
    public bool AttackFly { get { return _attackFly; } }
}
