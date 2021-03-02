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

    public SquareState State
    {
        get { return _state; }
        set { _state = value; }
    }

    [SerializeField]
    private Alignment _alignment;

    public Alignment Alignment
    {
        get { return _alignment; }
        set { _alignment = value; }
    }

    public static Color GetColorFromState(SquareState state)
    {
        switch (state)
        {
            case SquareState.Wall:
                return Color.black;
            case SquareState.Water:
                return Color.cyan;
            default:
                return Color.green;
        }
    }

    public static Color GetColorFromAlignment(Alignment alignment)
    {
        switch (alignment)
        {
            case Alignment.Player:
                return Color.blue;
            case Alignment.IA:
                return Color.red;
            default:
                return Color.white;
        }
    }
}