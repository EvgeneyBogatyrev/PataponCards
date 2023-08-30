using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    public static List<CardTypes> deck;
    public static List<CardTypes> playDeck;
    public static List<CardTypes> opponentsDeck = new();

    public static List<Runes> runes;

    public const int minDeckSize = 20;
    public const int maxCopy = 3;

    public static int opponentDeckSize = 20;

    public static void CopyDeck()
    {
        playDeck = deck;
        playDeck = playDeck.OrderBy(a => Guid.NewGuid()).ToList();
    }

    public static void ReceiveOpponentsDeck(List<int> encodedDeck)
    {
        opponentsDeck = new();
        foreach (int hash in encodedDeck)
        {
            opponentsDeck.Add((CardTypes) hash);
        }
    }

    public static List<int> GetEncodedDeck()
    {
        List<int> hashDeck = new();
        foreach (CardTypes type in playDeck)
        {
            hashDeck.Add((int) type);
        }
        return hashDeck;
    }

    public static void ResetOpponentsDeck(int cardNum = 20)
    {
        opponentDeckSize = cardNum;
    }

    public static CardTypes GetRandomCard(bool remove=true)
    {
        if (playDeck.Count == 0)
        {
            return CardTypes.Hatapon;
        }
        int index = UnityEngine.Random.Range(0, playDeck.Count);
        CardTypes card = playDeck[index];
        if (remove)
        {
            playDeck.RemoveAt(index);
        }
        return card;
    }

    public static CardTypes GetTopCard(bool remove=true)
    {
        if (playDeck.Count == 0)
        {
            return CardTypes.Hatapon;
        }

        CardTypes card = playDeck[0];
        if (remove)
        {
            playDeck.RemoveAt(0);
        }
        return card;
    }

    public static CardTypes GetTopCardOpp(bool remove=true)
    {
        if (opponentsDeck.Count == 0)
        {
            return CardTypes.Hatapon;
        }

        CardTypes card = opponentsDeck[0];
        if (remove)
        {
            opponentsDeck.RemoveAt(0);
        }
        return card;
    }

    public static bool RemoveCardFromDeck(CardTypes card)
    {
        int index = -1;
        int count = 0;
        foreach (CardTypes type in playDeck)
        {
            if (type == card)
            {
                index = count;
                break;
            }
            count++;
        }

        if (index == -1)
        {
            return false;
        }

        playDeck.RemoveAt(index);
        return true;
    }

    public static bool RemoveCardFromOppDeck(CardTypes card)
    {
        int index = -1;
        int count = 0;
        foreach (CardTypes type in opponentsDeck)
        {
            if (type == card)
            {
                index = count;
                break;
            }
            count++;
        }

        if (index == -1)
        {
            return false;
        }

        opponentsDeck.RemoveAt(index);
        return true;
    }

    public static void PutCardBack(CardTypes card)
    {
        playDeck.Add(card);
    }

    public static void SortDeck()
    {
        deck.Sort((x, y) => ((int)x).CompareTo((int)y));
    }

    public static void RemoveCard(CardTypes cardType)
    {
        deck.Remove(cardType);
    }

    public static void AddCard(CardTypes card)
    {
        deck.Add(card);
        SortDeck();
    }


    public static int GetPlayDeckSize()
    {
        return playDeck.Count;
    }
    public static int GetDeckSize()
    {
        return deck.Count;
    }
    

    public static int GetCardQty(CardTypes type)
    {
        int count = 0;
        foreach (CardTypes cardType in deck)
        {
            if (type == cardType)
            {
                count += 1;
            }
        }
        return count;
    }

    public static bool CheckCardNumber(CardTypes card)
    {
        int number = GetCardQty(card);
        
        CardManager.CardStats stats = CardGenerator.GetCardStats(card);

        if (stats.legendary)
        {
            if (number < 1)
            {
                return true;
            }
            return false;
        }

        if (number < maxCopy)
        {
            return true;
        }
        return false;
    }

    public static bool CheckDeckSize()
    {
        if (GetDeckSize() != minDeckSize)
        {
            return false;
        }
        return true;
    }
}
