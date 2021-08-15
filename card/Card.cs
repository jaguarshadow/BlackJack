using Godot;
using System;
using System.Collections.Generic;

public class Card : Control
{
    public enum SUIT { Spades, Hearts, Diamonds, Clubs }

    public static Dictionary<string, int> Values = new Dictionary<string, int>(){ { "A", 1 }, { "2", 2 }, { "3", 3 }, { "4", 4 }, { "5", 5 },
                                                                                  { "6", 6 }, { "7", 7 }, { "8", 8 }, { "9", 9 }, { "10", 10 }, 
                                                                                  { "J", 10 }, { "Q", 10 }, { "K", 10 } };
    public string CardValue { get; set; }
    public SUIT CardSuit { get; set; }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Sprite card_sprite = (Sprite)GetNode("CardFace");
        card_sprite.Texture = (Texture)ResourceLoader.Load(string.Format("res://assets/{0}.png", CardSuit));
        switch (CardValue)
        {
            case "A":
                card_sprite.Frame = 0;
                break;
            case "J":
                card_sprite.Frame = 10;
                break;
            case "Q":
                card_sprite.Frame = 11;
                break;
            case "K":
                card_sprite.Frame = 12;
                break;
            default:
                card_sprite.Frame = Values[CardValue] - 1;
                break;
        }
    }

    public void HideCard(bool hide)
    {
        Sprite card_back = (Sprite)GetNode("CardBack");
        if (hide) card_back.Show();
        else card_back.Hide();
    }
}
