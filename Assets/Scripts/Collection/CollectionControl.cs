using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using TMPro;

public class CollectionControl : MonoBehaviour
{
    private bool DEBUG = false;
    public int rows = 2;
    public int columns = 3;
    public float cardScale = 0.5f;
    public GameObject cardPrefab;
    public GameObject cardRepr;
    public GameObject cardReprSmaller;
    public GameObject cardReprSmallest;
    public GameObject cardNumberText;
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

    // Off by default: only cards that fit the deck's current runes are shown (the pre-Phase-3
    // behavior). Wire a UGUI Toggle's OnValueChanged(bool) to SetShowAllCards to let players see
    // the whole collection (rune-blocked cards included, tinted/locked) instead.
    private bool showAllCards = false;

    private List<GameObject> cardList;
    private int cardShowLimit = 10;
    private int start_idx = 0;
    private int deck_max = -1;

    public GameObject leftArrow;
    public GameObject rightArrow;

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
            //CardTypes.Moribu,
            //CardTypes.Grenburr,
            CardTypes.Wondabarappa,
            //CardTypes.Venomist,
            CardTypes.KibaForm,
            CardTypes.BirdForm,
            CardTypes.Catapult_option1,
            CardTypes.Catapult_option2,
            CardTypes.BabattaSwarm,
            CardTypes.LightningBolt,
            CardTypes.MeteorRain,
            CardTypes.SleepingDust,
            CardTypes.Megapon,
            //CardTypes.SparringPartner,
            //CardTypes.AvengingScout,
            //CardTypes.NaturalEnemy,
            CardTypes.Fatique,
        };

        return reservedList;
    }

    // Why a card can't be added to the deck currently being edited - drives CardManager's lock
    // icon (either reason) and gray backObject tint (BlockedByRunes only) instead of hiding the
    // card outright, so the player can see their whole collection and understand why it's unusable.
    public enum CardPlaceability { Placeable, BlockedByRunes, BlockedByLimit }

    public CardPlaceability GetPlaceability(CardTypes type)
    {
        CardManager.CardStats stats = CardTypeToStats.GetCardStats(type);
        if (!DEBUG && (bowDevotion < stats.GetDevotion(Runes.Bow)
            || shieldDevotion < stats.GetDevotion(Runes.Shield)
            || spearDevotion < stats.GetDevotion(Runes.Spear)
        ))
        {
            return CardPlaceability.BlockedByRunes;
        }

        if (!DeckManager.CheckCardNumber(type))
        {
            return CardPlaceability.BlockedByLimit;
        }

        int owned = DeckManager.collection.TryGetValue(type, out int ownedCount) ? ownedCount : 0;
        if (DeckManager.GetCardQty(type) >= owned)
        {
            return CardPlaceability.BlockedByLimit;
        }

        return CardPlaceability.Placeable;
    }

    private IEnumerator Start()
    {
        CursorController.cursorState = CursorController.CursorStates.Free;
        DeckManager.deck = SaveSystem.LoadDeck(DeckLoadManager.deckIndex);
        DeckManager.runes = SaveSystem.LoadRunes(DeckLoadManager.deckIndex);

        if (DeckManager.runes.Count == 0)
        {
            DeckManager.deck = SaveSystem.GetDefaultDeck();
            DeckManager.runes = new List<Runes>(){Runes.Spear, Runes.Shield, Runes.Bow};
        }

        yield return new WaitForSeconds(0.01f);
        
        if (DeckManager.runes.Count == 0)
        {
            DeckManager.runes = new List<Runes>{Runes.Spear, Runes.Spear, Runes.Spear};
        }


        cardList = new List<GameObject>();
        currentCards = new List<CardManager>();
        List<CardTypes> cardTypes = GetPage(0);
        currentPage = 0;
        ShowCards(cardTypes);
        

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

        UpdateRunes();
        ShowDeck();
        yield return null;
    }

    public void Update()
    {
        cardNumberText.GetComponent<TextMeshPro>().text = DeckManager.deck.Count.ToString() + "/24";
    }

    public void ButtonUp()
    {
        if (start_idx > 0)
        {
            start_idx -= 1;
        }
        ShowDeck();
    }

    public void ButtonDown()
    {
        if (start_idx + cardShowLimit <= deck_max - 1)
        {
            start_idx += 1;
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

        // A rune change reshuffles the whole sort order (fits-current-runes-first), so unlike
        // toggling Show All Cards (see SetShowAllCards) this is a full rebuild back to page 0
        // rather than an in-place diff - the "same page" concept doesn't really apply when the
        // ordering itself just changed.
        RebuildFromPage(0);
    }

    private List<CardTypes> GetCollectableCards()
    {
        string[] allCards = Enum.GetNames(typeof(CardTypes));
        List<CardTypes> relevantCards = new();
        List<CardTypes> reservedList = GetForbiddenCards();
        foreach (string stringType in allCards)
        {
            CardTypes type = (CardTypes)Enum.Parse(typeof(CardTypes), stringType);
            if (!reservedList.Contains(type))
            {
                relevantCards.Add(type);
            }
        }
        return relevantCards;
    }

    public bool CheckPage(int pageNumber)
    {
        var namesCount = GetBrowsableCards().Count;
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

    private int GetUniqueCardsNumber()
    {
        int count = 0;
        CardTypes prevCardType = CardTypes.Hatapon;
        foreach (CardTypes type in DeckManager.deck)
        {
            if (prevCardType != type)
            {
                count++;
            }
            prevCardType = type;
        }
        return count;
    }


    // Cards that fit the deck's current runes come first; cards that don't are pushed after them,
    // grouped by their own rune requirement (same neutral/spear/shield/bow/multiclass grouping
    // CardGenerator already uses for the name-outline material) so browsing the rest of the
    // collection isn't a random jumble. Card type order is the tiebreaker within each group.
    private List<CardTypes> SortForBrowsing(List<CardTypes> cards)
    {
        List<CardTypes> sorted = new List<CardTypes>(cards);
        sorted.Sort((a, b) =>
        {
            bool aBlocked = GetPlaceability(a) == CardPlaceability.BlockedByRunes;
            bool bBlocked = GetPlaceability(b) == CardPlaceability.BlockedByRunes;
            if (aBlocked != bBlocked)
            {
                return aBlocked ? 1 : -1;
            }

            int runeCompare = RuneCategoryOrder(a).CompareTo(RuneCategoryOrder(b));
            if (runeCompare != 0)
            {
                return runeCompare;
            }

            return ((int)a).CompareTo((int)b);
        });
        return sorted;
    }

    // Neutral < Spear < Shield < Bow < multiclass - mirrors CardGenerator.CustomizeCard's
    // neutral/spear/shield/bow/multiclass material selection.
    private int RuneCategoryOrder(CardTypes type)
    {
        CardManager.CardStats stats = CardTypeToStats.GetCardStats(type);
        int spearCount = 0, shieldCount = 0, bowCount = 0;
        foreach (Runes rune in stats.runes)
        {
            if (rune == Runes.Spear) spearCount++;
            else if (rune == Runes.Shield) shieldCount++;
            else if (rune == Runes.Bow) bowCount++;
        }

        if (spearCount == 0 && shieldCount == 0 && bowCount == 0) return 0; // neutral
        if (spearCount > 0 && shieldCount == 0 && bowCount == 0) return 1; // spear
        if (spearCount == 0 && shieldCount > 0 && bowCount == 0) return 2; // shield
        if (spearCount == 0 && shieldCount == 0 && bowCount > 0) return 3; // bow
        return 4; // multiclass
    }

    public List<CardTypes> GetPage(int pageNumber)
    {
        List<CardTypes> relevantCards = GetBrowsableCards();
        int namesCount = relevantCards.Count;

        List<CardTypes> cardList = new List<CardTypes>();
        int startNumber = pageNumber * rows * columns;
        int end = rows * columns;

        for (int i = 0; i < end && startNumber + i < namesCount; ++i)
        {
            cardList.Add(relevantCards[startNumber + i]);
        }
        return cardList;
    }

    // Sorted browsable set for the current showAllCards state - filtered down to rune-fitting
    // cards only when it's off (the pre-Phase-3 behavior), or the whole collection when on.
    private List<CardTypes> GetBrowsableCards()
    {
        List<CardTypes> relevantCards = new List<CardTypes>(DeckManager.collection.Keys);
        if (!showAllCards)
        {
            relevantCards.RemoveAll(type => GetPlaceability(type) == CardPlaceability.BlockedByRunes);
        }
        return SortForBrowsing(relevantCards);
    }

    private Vector3 GetGridPosition(int index)
    {
        int i = index / columns;
        int j = index % columns;
        return new Vector3(xOffset + j * cardSpaceX, yOffset + (rows - i - 1) * cardSpaceY, 0f);
    }

    public void ShowCards(List<CardTypes> cardTypes)
    {
        int slotCount = Mathf.Min(cardTypes.Count, rows * columns);
        for (int index = 0; index < slotCount; ++index)
        {
            GameObject cardObject = GenerateCard(cardTypes[index]);
            CardManager card = cardObject.GetComponent<CardManager>();
            card.SetDisplay();
            card.transform.position = GetGridPosition(index);
            card.transform.localScale = new Vector3(cardScale, cardScale, 1f);
            card.SetNumberOfCopies(DeckManager.collection[cardTypes[index]]);
            // Lock icon + rune-blocked overlay refresh every frame on their own (see
            // CardManager's CardState.display case) - no explicit call needed here.
            currentCards.Add(card);
        }
    }

    // Wire a UGUI Toggle's OnValueChanged(bool) to this. Deliberately does not destroy/regenerate
    // the whole page (see RefreshVisibleCardsInPlace) - only the cards whose visibility actually
    // changes are added or removed. currentPage is kept as-is unless unchecking made it fall past
    // the new last page, in which case it's clamped down to that last page (never up - checking
    // never needs to move off the current page, only unchecking can shrink past it).
    public void SetShowAllCards(bool _value)
    {
        Debug.Log("SetShowAllCards called: current=" + showAllCards + " new=" + _value
            + " collectionCount=" + (DeckManager.collection != null ? DeckManager.collection.Count.ToString() : "null(collection)"));
        if (showAllCards == _value)
        {
            return;
        }
        showAllCards = _value;

        Debug.Log("SetShowAllCards: browsableCount(filtered=" + (!showAllCards) + ")=" + GetBrowsableCards().Count
            + " currentPage=" + currentPage);

        int lastValidPage = GetLastValidPage();
        if (currentPage > lastValidPage)
        {
            currentPage = lastValidPage;
        }

        RefreshVisibleCardsInPlace();
    }

    private int GetLastValidPage()
    {
        int count = GetBrowsableCards().Count;
        if (count == 0)
        {
            return 0;
        }
        return (count - 1) / (rows * columns);
    }

    private void RefreshVisibleCardsInPlace()
    {
        List<CardTypes> newTypes = GetPage(currentPage);
        int slotCount = Mathf.Max(currentCards.Count, newTypes.Count);
        List<CardManager> updatedCards = new List<CardManager>();

        for (int index = 0; index < slotCount; ++index)
        {
            CardManager existing = index < currentCards.Count ? currentCards[index] : null;
            CardTypes? newType = index < newTypes.Count ? newTypes[index] : (CardTypes?)null;

            if (existing != null && newType.HasValue && existing.GetCardType() == newType.Value)
            {
                updatedCards.Add(existing);
                continue;
            }

            if (existing != null)
            {
                Destroy(existing.gameObject);
            }

            if (newType.HasValue)
            {
                GameObject cardObject = GenerateCard(newType.Value);
                CardManager card = cardObject.GetComponent<CardManager>();
                card.SetDisplay();
                card.transform.position = GetGridPosition(index);
                card.transform.localScale = new Vector3(cardScale, cardScale, 1f);
                card.SetNumberOfCopies(DeckManager.collection[newType.Value]);
                updatedCards.Add(card);
            }
        }

        currentCards = updatedCards;
        CheckButtons(currentPage);
    }

    public GameObject GenerateCard(CardTypes cardType)
    {
        GameObject newCard = Instantiate(cardPrefab);
        CardGenerator.CustomizeCard(newCard.GetComponent<CardManager>(), cardType);
        newCard.transform.localScale = new Vector3(cardScale, cardScale, 1f);
        return newCard;
    }

    // Full destroy-and-regenerate, resetting to a specific page - used whenever the page's
    // content/ordering genuinely needs a fresh start (navigating pages, a rune change reshuffling
    // sort order). Contrast with RefreshVisibleCardsInPlace, which only adds/removes what changed
    // without moving currentPage (used by the Show All Cards toggle).
    private void RebuildFromPage(int pageNumber)
    {
        foreach (CardManager card in currentCards)
        {
            Destroy(card.gameObject);
        }
        currentCards = new List<CardManager>();
        currentPage = pageNumber;
        ShowCards(GetPage(currentPage));
        CheckButtons(currentPage);
    }

    public void LoadNextPage()
    {
        if (CheckPage(currentPage + 1))
        {
            RebuildFromPage(currentPage + 1);
        }
    }

    private void CheckButtons(int pageNumber)
    {
        if (CheckPage(pageNumber - 1))
        {
            leftArrow.SetActive(true);
        }
        else
        {
            leftArrow.GetComponent<CollectionButton>().mouseOver = false;
            leftArrow.SetActive(false);
        }
        if (CheckPage(pageNumber + 1))
        {
            rightArrow.SetActive(true);
        }
        else
        {
            rightArrow.GetComponent<CollectionButton>().mouseOver = false;
            rightArrow.SetActive(false);
        }
    }

    public void LoadPrevPage()
    {
        if (CheckPage(currentPage - 1))
        {
            RebuildFromPage(currentPage - 1);
        }
    }

    public void BackButton()
    {
        if (true || DeckManager.deck.Count == DeckManager.minDeckSize)
        {
            SaveSystem.SaveDeckAndRunes(DeckManager.deck, DeckManager.runes, DeckLoadManager.deckIndex);
            SceneManager.LoadScene("MainMenu");
        }
    }

    // Click opens a Yes/Cancel confirmation instead of deleting immediately - see
    // CollectionButton.cs (status 3), which used to require a press-and-hold instead.
    public bool pendingDeckDeleteConfirmation = false;

    public void RequestDeckDeleteConfirmation()
    {
        pendingDeckDeleteConfirmation = true;
    }

    public void CancelDeckDelete()
    {
        pendingDeckDeleteConfirmation = false;
    }

    public void DeleteButton()
    {
        pendingDeckDeleteConfirmation = false;
        SaveSystem.DeleteDeck(DeckLoadManager.deckIndex);
        SceneManager.LoadScene("MainMenu");
    }

    private void OnGUI()
    {
        if (pendingDeckDeleteConfirmation)
        {
            ConfirmationBanner.Draw(new Color(0.6f, 0.1f, 0.1f, 0.9f), "Delete this deck? This can't be undone.",
                "Yes, delete deck", "Cancel", DeleteButton, CancelDeckDelete);
        }
    }

    public void ShowDeck()
    {
        DeckManager.SortDeck();
        foreach (GameObject obj in cardList)
        {
            CardReprManager cardReprManager = obj.GetComponent<CardReprManager>();
            if (cardReprManager.previewedCard != null)
            {
                Destroy(cardReprManager.previewedCard.gameObject);
            }
            Destroy(obj);
        }

        cardList = new List<GameObject>();

        int numberOfCards = GetUniqueCardsNumber();
        int cardsInRow = 8;

        int numberOfRows = (numberOfCards / cardsInRow) + (numberOfCards % cardsInRow > 0 ? 1 : 0);

        GameObject requiredPrefab;
        int charactersLimit = 100;
        float shift = 0f;
        if (numberOfRows == 1)
        {
            requiredPrefab = cardRepr;
        }
        else if (numberOfRows == 2)
        {
            requiredPrefab = cardReprSmaller;
            charactersLimit = 7;
            shift = 1.5f;
        }
        else
        {
            requiredPrefab = cardReprSmallest;
            charactersLimit = 4;
            shift = 1.2f;
        }

        float curPosY = deckStartY;
        CardReprManager prevCard = null;
        CardTypes prevCard_type = CardTypes.Hatapon;
        
        int row = 0;
        int index = 0;
        foreach (CardTypes type in DeckManager.deck)
        {
            if (prevCard == null || prevCard_type != type)
            {
                prevCard_type = type;
            
                GameObject card = GenerateCard(type);
                CardReprManager cardReprObj = Instantiate(requiredPrefab).GetComponent<CardReprManager>();
                string cardName = card.GetComponent<CardManager>().GetName();

                if (cardName.Length > charactersLimit)
                {
                    cardName = cardName.Substring(0, charactersLimit);
                    cardName += "...";
                }

                cardReprObj.SetName(cardName);
                //card.GetComponent<CardManager>().DestroyCard();
                Destroy(card);

                cardReprObj.gameObject.transform.position = new Vector3(deckStartX + shift * row, curPosY, 0f);

                prevCard = cardReprObj;

                cardReprObj.type = type;
                cardReprObj.numberOfCopies = 1;
                cardReprObj.SetVisualNumber();
                cardReprObj.index = index;
                cardReprObj.SetRowIndex(index);
                index += 1;

                curPosY -= deckCardSpace;

                cardList.Add(cardReprObj.gameObject);
                cardReprObj.collectionObject = this;

                if (index >= cardsInRow)
                {
                    index = 0;
                    curPosY = deckStartY;
                    row += 1;
                }
                
            }
            else
            {
                if (prevCard != null)
                {
                    prevCard.numberOfCopies += 1;
                    prevCard.SetVisualNumber();
                }
            }
        }
    }
}
