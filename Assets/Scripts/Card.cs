using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Card
{
    public Texture FrontPic { get; }
    public int Id { get; }
    public Month Month { get; }
    public CardType Type { get; }
    public bool IsSelected { get; set; }

    public Card(Texture frontPic, int id, CardType type, Month month)
    {
        FrontPic = frontPic;
        Id = id;
        Month = month;
        Type = type;
        IsSelected = false;
    }
}
