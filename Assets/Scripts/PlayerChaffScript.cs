using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerChaffScript : MonoBehaviour
{
    private List<GameObject> cards = new List<GameObject>();

    public void Receive(GameObject card)
    {
        cards.Add(card);
        if (cards.Count > 6 && cards.Count <= 9)
        {
            GridLayoutGroup glg = GetComponent<GridLayoutGroup>();
            glg.spacing = new Vector2(-10, 0);
        }
        else if (cards.Count > 9 && cards.Count <= 15)
        {
            GridLayoutGroup glg = GetComponent<GridLayoutGroup>();
            glg.spacing = new Vector2(-30, 0);
        }
        else if (cards.Count > 15)
        {
            GridLayoutGroup glg = GetComponent<GridLayoutGroup>();
            glg.spacing = new Vector2(-40, 0);
        }
        card.transform.SetParent(transform, false);
    }
}
