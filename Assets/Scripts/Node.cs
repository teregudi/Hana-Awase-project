using Assets.Scripts;
using System.Collections.Generic;

public class Node
{
    private static StateFactory stateFactory = new StateFactory();
    private static Dictionary<NodeType, NodeType> nodes = new Dictionary<NodeType, NodeType>()
    {
        { NodeType.MAX, NodeType.CHANCE_AFTER_MAX },
        { NodeType.MIN, NodeType.CHANCE_AFTER_MIN },
        { NodeType.CHANCE_AFTER_MAX, NodeType.MIN },
        { NodeType.CHANCE_AFTER_MIN, NodeType.MAX }
    };
    public StateSpace State { get; }
    public NodeType Type { get; }
    public int Value { get; }
    public bool IsTerminal
    {
        get { return State.CardsCollectedByAI.Count + State.CardsCollectedByPlayer.Count == 48; }
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
        stateFactory.InitialState = State;
        stateFactory.NodeType = Type;
        List<StateSpace> states = stateFactory.CreatePossibleStates();
        foreach (var state in states)
        {
            children.Add(new Node(state, nodes[Type]));
        } 
        return children;
    }

    private int CalculateValue()
    {
        int playerScore = State.PlayerScoreCurrently;
        int aiScore = State.AiScoreCurrently;
        return aiScore - playerScore;
    }
}
