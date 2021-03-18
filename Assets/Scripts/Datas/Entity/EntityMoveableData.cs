using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Default Entity Moveable", menuName = "TowerAttack/Entity/New Entity Moveable")]
public class EntityMoveableData : EntityData
{
    [Header("MOVE PROPERTIES")]
    [SerializeField]
    [Min(0.1f)]
    [Tooltip("Valeurs de la vitesse de l'entité")]
    private float _speed = 1;
    public float Speed { get { return _speed; } }
}
