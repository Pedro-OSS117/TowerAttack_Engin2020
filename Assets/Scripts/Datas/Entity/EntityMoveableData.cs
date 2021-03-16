using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Default Entity Moveable", menuName = "TowerAttack/Entity/New Entity Moveable")]
public class EntityMoveableData : EntityData
{
    [SerializeField]
    private float _speed;

    public float Speed { get { return _speed; } }
}
