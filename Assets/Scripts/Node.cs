using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public StateSpace State { get; }
    public NodeType Type { get; }
    public int Value { get; }

    public bool IsTerminal
    {
        get
        {
            return State.CardsAtPlayer.Count == 0 && State.CardsAtAI.Count == 0;
        }
    }

    public Node(StateSpace state, NodeType type)
    {
        State = state;
        Type = type;
        Value = CalculateValue();
    }

    private int CalculateValue()
    {
        return 0;
    }
}
