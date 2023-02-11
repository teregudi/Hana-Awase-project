using Assets.Scripts;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Node
{
    private static StateFactory stateFactory = new StateFactory();
    private static Dictionary<NodeType, NodeType> whichNodeTypeNext = new Dictionary<NodeType, NodeType>()
    {
        { NodeType.MAX, NodeType.CHANCE_AFTER_MAX },
        { NodeType.MIN, NodeType.CHANCE_AFTER_MIN },
        { NodeType.CHANCE_AFTER_MAX, NodeType.MIN },
        { NodeType.CHANCE_AFTER_MIN, NodeType.MAX }
    };
    public StateSpace State { get; }
    public NodeType Type { get; }
    public int Value { get; }
    public bool IsTerminal { get; }

    public Node(StateSpace state, NodeType type)
    {
        State = state;
        Type = type;
        Value = CalculateValue();
        // ha már kijátszottak minden lapot, felesleges tovább számolgatni
        IsTerminal = !State.CardsAtAI.Any() && State.CardsAtPlayer.Any();
    }

    public List<Node> GetChildNodes()
    {
        List<Node> children = new List<Node>();
        stateFactory.InitialState = State;
        stateFactory.NodeType = Type;
        List<StateSpace> states = stateFactory.CreatePossibleStates();
        foreach (var state in states)
        {
            children.Add(new Node(state, whichNodeTypeNext[Type]));
        } 
        return children;
    }

    private int CalculateValue()
    {
        int playerScore = State.GetPlayerAdditiveScore();
        int aiScore = State.GetAiAdditiveScore();
        return aiScore - playerScore;
    }
}
