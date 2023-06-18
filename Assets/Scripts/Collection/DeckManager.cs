using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    public static List<CardTypes> deck;
    public static List<CardTypes> playDeck;

    public static List<Runes> runes;

    public const int minDeckSize = 20;
    public const int maxCopy = 3;

    public static int opponentDeckSize = 20;

    public static void CopyDeck()
    {
        playDeck = deck;
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
        int index = Random.Range(0, playDeck.Count);
        CardTypes card = playDeck[index];
        if (remove)
        {
            playDeck.RemoveAt(index);
        }
        return card;
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
