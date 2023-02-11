using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OpponentAnimalScript : MonoBehaviour
{
    private List<GameObject> cards = new List<GameObject>();

    public void Receive(GameObject card)
    {
        cards.Add(card);
        if (cards.Count > 6)
        {
            GridLayoutGroup glg = GetComponent<GridLayoutGroup>();
            glg.spacing = new Vector2(-10, 0);
        }
        card.transform.SetParent(transform, false);
    }
}
