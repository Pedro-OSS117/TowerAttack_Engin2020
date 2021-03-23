using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Default Deck", menuName = "TowerAttack/Deck")]
public class Deck : ScriptableObject
{
#pragma warning disable 0649
    [SerializeField]
    private List<EntityData> _entities;
#pragma warning restore 0649

    public List<EntityData> Entities { get { return _entities; } }
}
