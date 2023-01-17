using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardLift : MonoBehaviour
{
    private Vector2 startPosition;
    public GameObject PlayerArea;
    public GameObject MiddleArea;
    public GameEngine GLS;

    private void Start()
    {
        GLS = GameEngine.getGE();
    }

    public void OnHoverEnter()
    {
        if (this.transform.parent.name == PlayerArea.name && IsItOkay())
        {
            startPosition = transform.position;
            transform.position = new Vector2(startPosition.x, startPosition.y + 30);
        }
    }

    public void OnHoverExit()
    {
        if (this.transform.parent.name == PlayerArea.name && IsItOkay())
        {
            transform.position = startPosition;
        }
    }

    private bool IsItOkay()
    {
        int thisMonth = Int32.Parse(this.name) / 10;
        foreach (Card card in GLS.State.CardsInMiddle)
        {
            if (card.Id / 10 == thisMonth)
            {
                return true;
            }
        }
        return false;
    }
}
