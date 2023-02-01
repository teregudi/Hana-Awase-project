using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpponentChaffScript : MonoBehaviour
{
    public List<GameObject> cards = new List<GameObject>();

    public void Receive(GameObject card)
    {
        cards.Add(card);
        card.transform.SetParent(transform, false);
    }
}
