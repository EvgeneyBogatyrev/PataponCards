using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class CollectionControl : MonoBehaviour
{
    public int rows = 2;
    public int columns = 3;
    public float cardScale = 0.75f;
    public GameObject cardPrefab;
    public GameObject cardRepr;
    public List<GameObject> runeObjects;

    public float xOffset;
    public float yOffset;
    public float cardSpaceX;
    public float cardSpaceY;

    public float deckStartX;
    public float deckStartY;
    public float deckCardSpace;

    public int spearDevotion = 0;
    public int shieldDevotion = 0;
    public int bowDevotion = 0;


    private List<CardManager> currentCards;
    private int currentPage;

    private List<GameObject> cardList;

    public List<CardTypes> GetForbiddenCards()
    {
        // Cards that are not collectable and should not be displayed
        List<CardTypes> reservedList = new()
        {
            CardTypes.Hatapon,
            CardTypes.Nutrition,
            CardTypes.GiveFang,
            CardTypes.Motiti_option1,
            CardTypes.Motiti_option2,
            CardTypes.MotitiAngry,
            CardTypes.Boulder,
            CardTypes.TonKampon_option1,
            CardTypes.TonKampon_option2,
            CardTypes.IceWall,
            CardTypes.IceWall_option,
            CardTypes.Concede,
            CardTypes.StoneFree,
            CardTypes.Mushroom,
            CardTypes.TrentOnFire,
            CardTypes.Armory_option1,
            CardTypes.Armory_option2,
            CardTypes.Horserider,
            CardTypes.TokenTatepon,
            CardTypes.SpeedBoost,
            CardTypes.TurnToStone,
            CardTypes.Moribu,
            CardTypes.Grenburr,
            CardTypes.Wondabarappa,
            CardTypes.Venomist,
            CardTypes.KibaForm,
            CardTypes.BirdForm,
            //CardTypes.TraitorBoulder,
        };

        return reservedList;
    }

    private void Start()
    {
        cardList = new List<GameObject>();
        currentCards = new List<CardManager>();
        List<CardTypes> cardTypes = GetPage(0);
        currentPage = 0;
        ShowCards(cardTypes);
        //DeckManager.FillDeck();

        DeckManager.deck = SaveSystem.LoadDeck();
        DeckManager.runes = SaveSystem.LoadRunes();

        spearDevotion = 0;
        shieldDevotion = 0;
        bowDevotion = 0;
        foreach (var (rune, runeObject) in Enumerable.Zip(DeckManager.runes, runeObjects, (rune, runeObject) => (rune, runeObject)))
        {
            runeObject.GetComponent<RuneDropdownManager>().SetRune(rune);
            if (rune == Runes.Spear)
            {
                spearDevotion += 1;
            }
            else if (rune == Runes.Shield)
            {
                shieldDevotion += 1;
            }
            else if (rune == Runes.Bow)
            {
                bowDevotion += 1;
            }
        }

        ShowDeck();
    }

    public void UpdateRunes()
    {
        spearDevotion = 0;
        shieldDevotion = 0;
        bowDevotion = 0;
        DeckManager.runes = new List<Runes>();
        foreach (GameObject runeObject in runeObjects)
        {
            Runes value = runeObject.GetComponent<RuneDropdownManager>().value;
            DeckManager.runes.Add(value);
            if (value == Runes.Spear)
            {
                spearDevotion += 1;
            }
            else if (value == Runes.Shield)
            {
                shieldDevotion += 1;
            }
            else if (value == Runes.Bow)
            {
                bowDevotion += 1;
            }
        }

        List<int> indices = new();

        int i = 0;
        foreach (CardTypes type in DeckManager.deck)
        {
            GameObject card = GenerateCard(type);
            List<Runes> runes = card.GetComponent<CardManager>().GetCardStats().runes;
            //card.GetComponent<CardManager>().DestroyCard();
            Destroy(card);

            int tmpSpear = spearDevotion;
            int tmpShield = shieldDevotion;
            int tmpBow = bowDevotion;

            bool bad = false;

            foreach (Runes rune in runes)
            {
                if (rune == Runes.Spear)
                {
                    tmpSpear--;
                }
                else if (rune == Runes.Shield)
                {
                    tmpShield--;
                }
                else if (rune == Runes.Bow)
                {
                    tmpBow--;
                }

                if (tmpBow < 0 || tmpShield < 0 || tmpSpear < 0)
                {
                    bad = true;
                    break;
                }
            }

            if (bad)
            {
                indices.Add(i);
            }
            i += 1;
        }

        List<CardTypes> newDeck = new();

        int ind = 0;
        foreach (CardTypes ct in DeckManager.deck)
        {
            bool add_card = true;
            foreach (int idx in indices)
            {
                if (ind == idx)
                {
                    add_card = false;
                    break;
                }
            }
            if (add_card)
            {
                newDeck.Add(ct);
            }
            ind += 1;
        }
        DeckManager.deck = newDeck;

        string runeString = "";
        foreach (Runes rune in DeckManager.runes)
        {
            if (rune == Runes.Spear)
            {
                runeString += "Sp ";
            }
            else if (rune == Runes.Shield)
            {
                runeString += "Sh ";
            }
            else if (rune == Runes.Bow)
            {
                runeString += "Bo ";
            }
            else
            {
                runeString += "?? ";
            }
        }

        //SaveSystem.SaveRunes(DeckManager.runes);
        ShowDeck();
    }

    public bool CheckPage(int pageNumber)
    {
        var namesCount = Enum.GetNames(typeof(CardTypes)).Length - GetForbiddenCards().Count;
        if (pageNumber < 0)
        {
            return false;
        }

        if (pageNumber * rows * columns >= namesCount)
        {
            return false;
        }

        return true;
    }

    public List<CardTypes> GetPage(int pageNumber)
    {
        var namesCount = Enum.GetNames(typeof(CardTypes)).Length;
        List<CardTypes> cardList = new List<CardTypes>();
        int startNumber = pageNumber * rows * columns;
        int end = rows * columns;
        List<CardTypes> reservedList = GetForbiddenCards();
        for (int i = 0; i < end && startNumber + i < namesCount; ++i)
        {
            if (reservedList.Contains((CardTypes)(startNumber + i)))
            {
                end += 1;
                continue;
            }
            else
            {
                cardList.Add((CardTypes)(startNumber + i));
            }
        }
        return cardList;
    }

    public void ShowCards(List<CardTypes> cardTypes)
    {
        for (int i = 0; i < rows; ++i)
        {
            for (int j = 0; j < columns; ++j)
            {
                int index = i * columns + j;
                if (index >= cardTypes.Count)
                {
                    break;
                }

                GameObject cardObject = GenerateCard(cardTypes[index]);
                CardManager card = cardObject.GetComponent<CardManager>();
                card.SetDisplay();
                card.transform.position = new Vector3(xOffset + j * cardSpaceX, yOffset + (rows - i - 1) * cardSpaceY, 0f);
                card.transform.localScale = new Vector3(cardScale, cardScale, 1f);
                currentCards.Add(card);
            }
        }
    }

    public GameObject GenerateCard(CardTypes cardType)
    {
        GameObject newCard = Instantiate(cardPrefab);
        CardGenerator.CustomizeCard(newCard.GetComponent<CardManager>(), cardType);
        return newCard;
    }

    public void LoadNextPage()
    {
        if (CheckPage(currentPage + 1))
        {
            currentPage += 1;
            foreach (CardManager card in currentCards)
            {
                Destroy(card.gameObject);
            }
            currentCards = new List<CardManager>();
            ShowCards(GetPage(currentPage));
        }
    }

    public void LoadPrevPage()
    {
        if (CheckPage(currentPage - 1))
        {
            currentPage -= 1;
            foreach (CardManager card in currentCards)
            {
                Destroy(card.gameObject);
            }
            currentCards = new List<CardManager>();
            ShowCards(GetPage(currentPage));
        }
    }

    public void BackButton()
    {
        SaveSystem.SaveRunes(DeckManager.runes);
        SaveSystem.SaveDeck(DeckManager.deck);
        if (DeckManager.GetDeckSize() == DeckManager.minDeckSize)
        {
            SceneManager.LoadScene("MainMenu");
        }
    }

    public void ShowDeck()
    {
        DeckManager.SortDeck();
        foreach (GameObject obj in cardList)
        {
            Destroy(obj);
        }

        cardList = new List<GameObject>();

        float curPosY = deckStartY;
        CardReprManager prevCard = null;
        foreach (CardTypes type in DeckManager.deck)
        {
            if (prevCard == null || prevCard.type != type)
            {
                GameObject card = GenerateCard(type);
                CardReprManager cardReprObj = Instantiate(cardRepr).GetComponent<CardReprManager>();
                cardReprObj.SetName(card.GetComponent<CardManager>().GetName());
                //card.GetComponent<CardManager>().DestroyCard();
                Destroy(card);

                cardReprObj.gameObject.transform.position = new Vector3(deckStartX, curPosY, 0f);

                prevCard = cardReprObj;

                cardReprObj.type = type;
                cardReprObj.numberOfCopies = 1;
                cardReprObj.SetVisualNumber();

                curPosY -= deckCardSpace;

                cardList.Add(cardReprObj.gameObject);
                cardReprObj.collectionObject = this;
            }
            else
            {
                prevCard.numberOfCopies += 1;
                prevCard.SetVisualNumber();
            }
        }
    }
}
