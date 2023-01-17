using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AiMoveHandler
{
    public static int ExMinMax(Node node, int depth)
    {
        if (node.IsTerminal || depth == 0)
        {
            return node.Value;
        }
        if (node.Type == NodeType.MIN)
        {

        }
        else if (node.Type == NodeType.MAX)
        {

        }
        else if (node.Type == NodeType.CHANCE)
        {

        }
        return 0;
    }
}
