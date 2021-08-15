using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class Blackjack : Node2D
{
    // Declare member variables here. Examples:
    private List<Card> Deck = new List<Card>();
    private List<Card> DiscardPile = new List<Card>();
    private List<Card> DealerHand = new List<Card>();
    private List<Card> PlayerHand = new List<Card>();

    private HBoxContainer DealerContainer;
    private HBoxContainer PlayerContainer;
    private Label DealerTotal;
    private Label PlayerTotal;
    private Label GameStatus;
    private int Score = 0;

    [Signal]
    public delegate void TotalChanged(Label label, string Total);

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        DealerContainer = (HBoxContainer)GetNode("CanvasLayer/UI/Dealer/Hand");
        PlayerContainer = (HBoxContainer)GetNode("CanvasLayer/UI/Player/Hand");
        DealerTotal = (Label)GetNode("CanvasLayer/UI/Dealer/Total");
        PlayerTotal = (Label)GetNode("CanvasLayer/UI/Player/Total");
        CreateDeck();
        ShuffleDeck(ref Deck);
        ((Button)GetNode("CanvasLayer/UI/ButtonContainer/DealButton")).Disabled = false;
    }

    public async void DealHand()
    {
        if (GameStatus != null) GameStatus.Hide();
        ((Button)GetNode("CanvasLayer/UI/ButtonContainer/DealButton")).Disabled = true;
        ChangeTotal(DealerTotal, -1);
        await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
        DealCard(PlayerHand, PlayerContainer);
        await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
        DealCard(DealerHand, DealerContainer, true);
        await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
        DealCard(PlayerHand, PlayerContainer);
        await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
        DealCard(DealerHand, DealerContainer);
        ((Button)GetNode("CanvasLayer/UI/ButtonContainer/HitButton")).Disabled = false;
        ((Button)GetNode("CanvasLayer/UI/ButtonContainer/StandButton")).Disabled = false;
    }

    public void CreateDeck()
    {
        foreach (Card.SUIT suit in Enum.GetValues(typeof(Card.SUIT)))
        {
            foreach (string value in Card.Values.Keys)
            {
                Card c = (Card)((PackedScene)ResourceLoader.Load("res://card/Card.tscn")).Instance();
                c.CardSuit = suit;
                c.CardValue = value;
                Deck.Add(c);
            }
        }
    }

    public void ShuffleDeck(ref List<Card> deck)
    {
        Random random = new Random();
        deck = deck.OrderBy(item => random.Next()).ToList();
    }

    public Card DealCard(List<Card> hand, HBoxContainer container, bool hidden = false)
    {
        if (Deck.Count < 1)
        {
            ShuffleDeck(ref DiscardPile);
            Deck.AddRange(DiscardPile);
            DiscardPile.Clear();
        }
        Card c = Deck[0];
        Deck.RemoveAt(0);
        if (hidden) c.HideCard(true);
        else c.HideCard(false);
        hand.Add(c);
        container.AddChild(c);
        if (hand == PlayerHand)
        {
            GD.Print("Updating player total");
            ChangeTotal(PlayerTotal, CheckHand(PlayerHand));
        }
        return c;
    }

    public void ChangeTotal(Label label, int total)
    {
        if (total == -1) label.Text = "Total: ??";
        else label.Text = "Total: " + total;
    }

    public void ChangeScore(int score)
    {
        ((Label)GetNode("CanvasLayer/UI/GameStatus/Score")).Text = string.Format("Score: {0}", score);
    }

    public void ChangeResult(string result, bool won = true)
    {
        if (won) GameStatus = (Label)GetNode("CanvasLayer/UI/GameStatus/ResultWin");
        else GameStatus = (Label)GetNode("CanvasLayer/UI/GameStatus/ResultLose");
        GameStatus.Text = result;
        GameStatus.Show();
    }

    public int CheckHand(List<Card> hand)
    {
        List<string> hand_values = hand.Select(card => card.CardValue).ToList();
        int total = hand_values.Sum(value => Card.Values[value]);
        if (hand_values.Contains("A"))
        {
            // Handle aces
            int number_of_aces = hand_values.Where(value => value == "A").ToList().Count;
            if (total + (10 * number_of_aces) <= 21) total = total + (10 * number_of_aces);
        }
        return total;
    }

    // Event Handlers
    public void _OnDealButtonPressed()
    {
        foreach (HBoxContainer container in new List<HBoxContainer> { PlayerContainer, DealerContainer})
        {
            foreach (Node card in container.GetChildren())
            {
                DiscardPile.Add((Card)card);
                container.RemoveChild(card);
            }
        }
        PlayerHand.Clear();
        DealerHand.Clear();
        DealHand();
    }

    public void _OnHitButtonPressed()
    {
        DealCard(PlayerHand, PlayerContainer);
        int total = CheckHand(PlayerHand);
        ChangeTotal(PlayerTotal, total);
        if (total > 21)
        {
            ChangeResult("Player Busted\nHouse Wins.", false);
            Score -= 1;
            ChangeScore(Score);
            ((Button)GetNode("CanvasLayer/UI/ButtonContainer/HitButton")).Disabled = true;
            ((Button)GetNode("CanvasLayer/UI/ButtonContainer/StandButton")).Disabled = true;
            ((Button)GetNode("CanvasLayer/UI/ButtonContainer/DealButton")).Disabled = false;
        }
    }

    public async void _OnStandButtonPressed()
    {
        ((Button)GetNode("CanvasLayer/UI/ButtonContainer/HitButton")).Disabled = true;
        ((Button)GetNode("CanvasLayer/UI/ButtonContainer/StandButton")).Disabled = true;
        ChangeTotal(DealerTotal, CheckHand(DealerHand));
        DealerHand[0].HideCard(false);
        while (CheckHand(DealerHand) < 17)
        {
            await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
            DealCard(DealerHand, DealerContainer);
            ChangeTotal(DealerTotal, CheckHand(DealerHand));
        }

        // Check Final Scores
        int player_total = CheckHand(PlayerHand);
        int dealer_total = CheckHand(DealerHand);

        if (dealer_total < 22) 
        {
            if ((21 - player_total) < (21 - dealer_total))
            {
                ChangeResult("Player Wins!");
                Score += 1;
            }
            else
            {
                ChangeResult("House Wins.", false);
                Score -= 1;
            }
        }
        else
        {
            ChangeResult("Dealer Busted\nPlayer Wins!");
            Score += 1;
        }
        ChangeScore(Score);
        ((Button)GetNode("CanvasLayer/UI/ButtonContainer/DealButton")).Disabled = false;
    }


}
