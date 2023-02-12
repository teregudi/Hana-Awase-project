using System;

public static class Expectiminimax
{
    public static int CalculateNodeValue(Node node, int depth)
    {
        if (depth == 0 || node.IsTerminal)
        {
            return node.Value;
        }

        if (node.Type == NodeType.MAX)
        {
            int maxValue = int.MinValue;
            foreach (Node child in node.GetChildNodes())
            {
                maxValue = Math.Max(maxValue, CalculateNodeValue(child, depth - 1));
            }
            return maxValue;
        }

        if (node.Type == NodeType.MIN)
        {
            int minValue = int.MaxValue;
            foreach (Node child in node.GetChildNodes())
            {
                minValue = Math.Min(minValue, CalculateNodeValue(child, depth - 1));
            }
            return minValue;
        }

        // if node type is chance
        float expectedValue = 0;
        foreach (Node child in node.GetChildNodes())
        {
            expectedValue += CalculateNodeValue(child, depth - 1) * child.State.Probability;
        }
        return (int)expectedValue;
    }
}
