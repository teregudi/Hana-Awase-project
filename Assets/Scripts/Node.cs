using Assets.Scripts;
using System.Collections.Generic;
using System.Linq;

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
    public float Probability { get; }

    public Node(StateSpace state, NodeType type, float probability)
    {
        State = state;
        Type = type;
        Value = CalculateValue();
        Probability = probability;
        // ha már kijátszottak minden lapot, felesleges tovább számolgatni
        IsTerminal = !State.CardsAtAI.Any() && State.CardsAtPlayer.Any();
    }

    public List<Node> GetChildNodes()
    {
        List<Node> children = new List<Node>();
        stateFactory.InitialState = State;
        stateFactory.NodeType = Type;
        List<StateSpace> states = stateFactory.CreatePossibleStates();
        float probability = (Type == NodeType.CHANCE_AFTER_MAX || Type == NodeType.CHANCE_AFTER_MIN) ? 1f / states.Count : 1;
        foreach (var state in states)
        {
            children.Add(new Node(state, whichNodeTypeNext[Type], probability));
        } 
        return children;
    }

    private int CalculateValue()
    {
        int playerScore = State.GetPlayerScore();
        int aiScore = State.GetAiScore();
        return aiScore - playerScore;
    }
}
