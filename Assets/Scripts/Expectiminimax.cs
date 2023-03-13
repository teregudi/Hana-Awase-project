using System;
using UnityEngine;

public static class Expectiminimax
{
    public static int counter = 0;

    public static int CalculateNodeValue(Node node, int depth, int alpha, int beta)
    {
        if (depth == 0 || node.IsTerminal)
        {
            counter++;
            return node.Value;
        }

        if (node.Type == NodeType.MAX)
        {
            int maxValue = int.MinValue;
            foreach (Node child in node.GetChildNodes())
            {
                counter++;
                maxValue = Math.Max(maxValue, CalculateNodeValue(child, depth - 1, alpha, beta));
                if (maxValue > beta)
                    break;
                alpha = Math.Max(alpha, maxValue);
            }
            return maxValue;
        }

        if (node.Type == NodeType.MIN)
        {
            int minValue = int.MaxValue;
            foreach (Node child in node.GetChildNodes())
            {
                counter++;
                minValue = Math.Min(minValue, CalculateNodeValue(child, depth - 1, alpha, beta));
                if (minValue < alpha)
                    break;
                beta = Math.Min(beta, minValue);
            }
            return minValue;
        }

        // if node type is chance
        float expectedValue = 0;
        foreach (Node child in node.GetChildNodes())
        {
            counter++;
            expectedValue += CalculateNodeValue(child, depth - 1, alpha, beta) * child.Probability;
        }
        return (int)expectedValue;
    }
}
