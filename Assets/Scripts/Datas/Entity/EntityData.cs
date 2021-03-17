using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EntityType
{
    Unit,
    Building
}

[CreateAssetMenu(fileName = "Default Entity", menuName = "TowerAttack/Entity/New Entity")]
public class EntityData : ScriptableObject
{
    [SerializeField]
    [Min(0)]
    private int _life = 0;
    public int Life { get { return _life; } }

    [SerializeField]
    private string _nameEntity = "No Name";
    public string Name { get { return _nameEntity; } }

    [SerializeField]
    private EntityType _type = EntityType.Unit;
    public EntityType Type { get { return _type; } }

    [SerializeField]
    [Range(1, 10)]
    private int _popAmount = 0;
    public int PopAmount { get { return _popAmount; } }

    [SerializeField]
    private ActionData[] _actions = null;
    public ActionData[] Actions { get { return _actions; } }

    [SerializeField]
    [Min(0)]
    private int level = 0;
    public int Level { get { return level; } }

    [SerializeField]
    [Range(1, 15)]
    private int numberPop = 1;
    public int NumberPop { get { return numberPop; } }

}
