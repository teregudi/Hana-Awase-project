using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Node
{
    public StateSpace State { get; }
    public NodeType Type { get; }
    public int Value { get; }
    public int Probability // do I need this?
    {
        get
        {
            return 1;
        } 
    }

    public bool IsTerminal
    {
        get
        {
            return !State.CardsInMiddle.Any();
        }
    }

    public Node(StateSpace state, NodeType type)
    {
        State = state;
        Type = type;
        Value = CalculateValue();
    }

    public List<Node> GetChildNodes()
    {
        List<Node> children = new List<Node>();

        return children;
    }

    private int CalculateValue()
    {
        int playerScore = State.PlayerScoreCurrently;
        int aiScore = State.AiScoreCurrently;
        return aiScore - playerScore;
    }
}
