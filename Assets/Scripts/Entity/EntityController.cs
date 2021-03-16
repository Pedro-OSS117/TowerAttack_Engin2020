using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityController : MonoBehaviour
{
    [SerializeField]
    private EntityData _datas;

    public EntityData Datas { get { return _datas; } }
    
    [SerializeField]
    private int _currentLife = 0;

    public void Awake()
    {
        _currentLife = _datas.Life;
    }
}
