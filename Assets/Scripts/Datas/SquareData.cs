using UnityEngine;
using System;

public enum SquareState
{
    Normal,
    Wall,
    Water,
}

public enum Alignment
{
    Player,
    IA,
    Neutral
}

[Serializable]
public struct SquareData
{
    [SerializeField]
    private SquareState _state;
    
    [SerializeField]
    private Alignment _alignment;
}