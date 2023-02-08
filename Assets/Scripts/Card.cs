using UnityEngine;

public class Card
{
    // lehet hogy ezt a frontpicet nem itt k�lne t�rolni
    // �s tal�n az id-t�l is meg lehet szabadulni
    public Texture FrontPic { get; }
    public int Id { get; }
    public Month Month { get; }
    public CardType Type { get; }

    public Card(Texture frontPic, int id, CardType type, Month month)
    {
        FrontPic = frontPic;
        Id = id;
        Month = month;
        Type = type;
    }

    public override string ToString()
    {
        return $"{Month.ToString()} {Type.ToString()}";
    }
}
