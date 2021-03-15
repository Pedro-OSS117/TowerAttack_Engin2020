using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EntityType
{
    Unit,
    Building
}

public class EntityData : MonoBehaviour
{
    [SerializeField]
    private int _life = 0;

    public int Life { get { return _life; } }

    [SerializeField]
    private string _nameEntity = "No Name";

    public string Name { get { return _nameEntity; } }

    [SerializeField]
    private EntityType _type = EntityType.Unit;

    public EntityType Type { get { return _type; } }

    [SerializeField]
    private int _popAmount = 0;

    public int PopAmount { get { return _popAmount; } }

    [SerializeField]
    private ActionData[] _actions = null;

}
