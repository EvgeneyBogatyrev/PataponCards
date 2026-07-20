using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Bot
{
    public class BotMove
    {
        public CardTypes playCard = CardTypes.Hatapon;
        public int cellNumber = -1;
        public int attackCell = -1;
        public int moveCell = -1;
        public bool exchange = false;
        public List<int> deck = null;
        public bool endTurn = false;
    }

    public bool botActive = false;
    public int botLevel = 2;

    private bool deckSend = false;
    public bool cardPlayed = false;

    public List<CardTypes> botDeck = null;

    private void InitDeck()
    {
        if (botDeck != null)
        {
            return;
        }
        if (botLevel == 3)
        {
            // "Smart Bot" - a midrange curve of vanilla stat-sticks and simple triggered-ability
            // creatures (no on-play targeting, so it stays compatible with GetNextMove's
            // single-target PlayCard message - see ChooseCardToPlaySmart for why this level, and
            // only this level, actually reasons about which of these to play rather than picking
            // randomly).
            botDeck = new List<CardTypes>()
            {
                CardTypes.Kuwagattan,
                CardTypes.Kuwagattan,
                CardTypes.Guardira,
                CardTypes.Guardira,
                CardTypes.Dekapon,
                CardTypes.Dekapon,
                CardTypes.ZigotonTroops,
                CardTypes.ZigotonTroops,
                CardTypes.ZigotonTroops,
                CardTypes.Moforumo,
                CardTypes.Moforumo,
                CardTypes.GongTheHawkeye,
                CardTypes.GongTheHawkeye,
                CardTypes.GongTheHawkeye,
                CardTypes.Tatepon,
                CardTypes.Tatepon,
                CardTypes.Tatepon,
                CardTypes.PanThePakapon,
                CardTypes.MyamsarHero,
                CardTypes.MyamsarHero,
                CardTypes.MyamsarHero,
                CardTypes.Yaripon,
                CardTypes.Yaripon,
                CardTypes.TraitorBoulder,
            };
        }
        else if (botLevel == 2)
        {
            botDeck = new List<CardTypes>()
            {
                CardTypes.Tatepon,
                CardTypes.Tatepon,
                CardTypes.Tatepon,
                CardTypes.MyamsarHero,
                CardTypes.MyamsarHero,
                CardTypes.MyamsarHero,
                CardTypes.Dekapon,
                CardTypes.Dekapon,
                CardTypes.Kuwagattan,
                CardTypes.Kuwagattan,
                CardTypes.Moforumo,
                CardTypes.Piekron,
                CardTypes.Piekron,
                CardTypes.Piekron,
                CardTypes.Yaripon,
                CardTypes.Yaripon,
                CardTypes.Yumipon,
                CardTypes.Yumipon,
                CardTypes.Yumipon,
                CardTypes.GongTheHawkeye,
                CardTypes.GongTheHawkeye,
                CardTypes.GongTheHawkeye,
                CardTypes.FuckingIdiot,
                CardTypes.FuckingIdiot,
            };
        }
        else if (botLevel == 1)
        {
            botDeck = new List<CardTypes>()
            {
                CardTypes.Tatepon,
                CardTypes.Tatepon,
                CardTypes.Tatepon,
                CardTypes.TargetDummy,
                CardTypes.TargetDummy,
                CardTypes.TargetDummy,
                CardTypes.Guardira,
                CardTypes.Guardira,
                CardTypes.Dekapon,
                CardTypes.Moforumo,
                CardTypes.PanThePakapon,
                CardTypes.ZigotonTroops,
                CardTypes.ZigotonTroops,
                CardTypes.Yaripon,
                CardTypes.Yaripon,
                CardTypes.Yaripon,
                CardTypes.Yumipon,
                CardTypes.Yumipon,
                CardTypes.Yumipon,
                CardTypes.FuckingIdiot,
                CardTypes.FuckingIdiot,
                CardTypes.MyamsarHero,
                CardTypes.MyamsarHero,
                CardTypes.GongTheHawkeye,
            };
        }
        else if (botLevel <= 0)
        {
            botDeck = new List<CardTypes>()
            {
                CardTypes.Tatepon,
                CardTypes.Tatepon,
                CardTypes.Tatepon,
                CardTypes.TargetDummy,
                CardTypes.TargetDummy,
                CardTypes.TargetDummy,
                CardTypes.Guardira,
                CardTypes.Guardira,
                CardTypes.Guardira,
                CardTypes.TraitorBoulder,
                CardTypes.TraitorBoulder,
                CardTypes.PanThePakapon,
                CardTypes.PanThePakapon,
                CardTypes.PanThePakapon,
                CardTypes.Yaripon,
                CardTypes.Yaripon,
                CardTypes.Yaripon,
                CardTypes.Yumipon,
                CardTypes.Yumipon,
                CardTypes.FuckingIdiot,
                CardTypes.FuckingIdiot,
                CardTypes.FuckingIdiot,
                CardTypes.MyamsarHero,
                CardTypes.MyamsarHero,
            };
        }
    }

    public List<CardTypes> botHand = new List<CardTypes>();

    private bool CheckLifelink(List<BoardManager.Slot> slots)
    {
        foreach (BoardManager.Slot _slot in slots)
        {
            if (!_slot.GetFree() && _slot.GetConnectedMinion().GetCardStats().hasShield)
            {
                return true;
            }
        }
        return false;
    }

    // "Don't overextend" check - if the bot already has enough unblocked power positioned to
    // threaten the enemy Hatapon directly, and nothing forces it to deal with something else
    // first (a Lifelink unit blocks all Hatapon damage regardless of how much power is
    // threatening it, so that always has to be dealt with before Hatapon damage matters at all),
    // there's no benefit to developing more board - it just attacks with what it already has each
    // turn instead of dumping its whole hand for no reason.
    private bool HasEnoughAttackersForHatapon(List<BoardManager.Slot> playerSlots, List<BoardManager.Slot> botSlots)
    {
        if (CheckLifelink(playerSlots))
        {
            return false;
        }

        BoardManager.Slot hataponSlot = FindSlotWithCard(playerSlots, CardTypes.Hatapon);
        if (hataponSlot == null)
        {
            return false;
        }

        int hataponLife = hataponSlot.GetConnectedMinion().GetPower();
        int threateningPower = 0;
        foreach (BoardManager.Slot slot in botSlots)
        {
            if (slot.GetFree())
            {
                continue;
            }
            MinionManager minion = slot.GetConnectedMinion();
            if (!minion.GetCardStats().canDealDamage || minion.GetCardStats().isStatic)
            {
                continue;
            }
            if (Mathf.Abs(slot.GetIndex() - hataponSlot.GetIndex()) <= 1)
            {
                threateningPower += minion.GetPower();
            }
        }

        return threateningPower >= hataponLife;
    }

    private BoardManager.Slot FindSlotWithCard(List<BoardManager.Slot> slots, CardTypes type)
    {
        foreach(BoardManager.Slot _slot in slots)
        {
            if (!_slot.GetFree() && _slot.GetConnectedMinion().GetCardType() == type)
            {
                return _slot;
            }
        }
        return null;
    }

    private BoardManager.Slot RateSlotsToPlayCard(List<BoardManager.Slot> playerSlots, List<BoardManager.Slot> botSlots, List<BoardManager.Slot> selectedSlots, CardTypes cardToPlay)
    {
        BoardManager.Slot selectedSlot = null;
        BoardManager.Slot targetSlot = null;

        if (botLevel >= 2 && (cardToPlay == CardTypes.Yumipon || cardToPlay == CardTypes.FuckingIdiot))
        {
            foreach (BoardManager.Slot _slot in selectedSlots)
            {
                int idx = _slot.GetIndex();
                if (selectedSlot == null || (playerSlots[idx].GetFree() && (idx == 6 || playerSlots[idx + 1].GetFree()) && (idx == 0 || playerSlots[idx - 1].GetFree())))
                {
                    selectedSlot = _slot;
                }
            }
            return selectedSlot;
        }

        if (CheckLifelink(playerSlots) && botLevel >= 1)
        {
            foreach(BoardManager.Slot _slot in playerSlots)
            {
                if (!_slot.GetFree() && _slot.GetConnectedMinion().GetCardStats().hasShield)
                {
                    targetSlot = _slot;
                }
            }
        }
        else
        {
            /*foreach(BoardManager.Slot _slot in playerSlots)
            {
                if (!_slot.GetFree() && _slot.GetConnectedMinion().GetCardStats().endTurnEvent != null)
                {
                    targetSlot = _slot;
                }
            }
            */
            if (targetSlot == null)
            {
                targetSlot = FindSlotWithCard(playerSlots, CardTypes.Hatapon);
            }
        }
        
        if (targetSlot == null)
        {
            return selectedSlots[0];
        }
        foreach (BoardManager.Slot _slot in selectedSlots)
        {
            if (selectedSlot == null || Math.Abs(selectedSlot.GetIndex() - targetSlot.GetIndex()) > Math.Abs(_slot.GetIndex() - targetSlot.GetIndex()))
            {
                selectedSlot = _slot;
            }
        }
        return selectedSlot;
        
    }

    public float EvaluateAttackUnit(MinionManager enemyMinion, MinionManager myMinion, bool enemyLifelink)
    {
        float score = 0f;

        if (enemyMinion.GetCardStats().hasShield && botLevel >= 1)
        {
            score += 0.15f;
        }
        if (enemyMinion.GetCardStats().endTurnEvent != null && botLevel >= 2)
        {
            score += 0.35f;
        }
        if (enemyMinion.GetCardStats().hasOnDeath && botLevel >= 2)
        {
            score -= 10f;
        }

        if (myMinion.GetCardType() == CardTypes.Yaripon || myMinion.GetCardType() == CardTypes.Yumipon)
        {
            score -= 10f;
        }

        if (enemyMinion.GetCardType() == CardTypes.Hatapon)
        {
            if (!enemyLifelink)
            {
                if (myMinion.GetPower() > enemyMinion.GetPower())
                {
                    return 100f;
                }
                return score + myMinion.GetPower() / (enemyMinion.GetPower() - myMinion.GetPower() + 0.1f);
            }
            else
            {
                return score - 100f;
            }
        }
        else if (!enemyMinion.GetCardStats().canDealDamage)
        {
            return score - (enemyMinion.GetPower() - myMinion.GetPower() + 1) / 20f;
        }
        
        return score - (enemyMinion.GetPower() - myMinion.GetPower()) / 5f;
    }

    public float EvaluateMoveUnit(BoardManager.Slot slotFrom, BoardManager.Slot slotTo, List<BoardManager.Slot> playerSlots)
    {
        if (slotFrom.GetConnectedMinion().GetCardType() == CardTypes.Hatapon)
        {
            float value = 0f;
            for (int i = slotTo.GetIndex() - 1; i <= slotTo.GetIndex() + 1; ++i)
            {
                if (i >= 0 && i <= 6 && !playerSlots[i].GetFree())
                {
                    if (playerSlots[i].GetConnectedMinion().GetCardStats().canDealDamage && botLevel >= 1)
                    {
                        value -= playerSlots[i].GetConnectedMinion().GetPower();
                    }
                }
            }
            value += 0.01f * Mathf.Abs(3 - slotTo.GetIndex());
            Debug.Log($"value of {slotTo.GetIndex()}: {value}");
            return value;
        }

        BoardManager.Slot hataponSlot = null;
        if (CheckLifelink(playerSlots))
        {
            foreach(BoardManager.Slot _slot in playerSlots)
            {
                if (!_slot.GetFree() && _slot.GetConnectedMinion().GetCardStats().hasShield)
                {
                    hataponSlot = _slot;
                }
            }
        }
        else
        {
            hataponSlot = FindSlotWithCard(playerSlots, CardTypes.Hatapon);
        }
        if (hataponSlot == null)
        {
            return 0f;
        }
        return -Mathf.Abs(slotTo.GetIndex() - hataponSlot.GetIndex());

    }

    private int RateAttackTargets(List<BoardManager.Slot> playerSlots, List<BoardManager.Slot> botSlots, List<int> moveTargets, BoardManager.Slot mySlot)
    {
        int bestIndex = -1;
        float bestEval = 0f;

        bool lifelink = CheckLifelink(playerSlots);

        foreach (int target in moveTargets)
        {
            float curEval = EvaluateAttackUnit(playerSlots[target - 1].GetConnectedMinion(), mySlot.GetConnectedMinion(), lifelink);
            Debug.Log($"Cost of {target}: {curEval}");
            if (bestIndex == -1 || curEval > bestEval)
            {
                bestEval = curEval;
                bestIndex = target;
            }
        }

        if (bestEval < -5f)
        {
            return -1;
        }
        
        return bestIndex;
    
    }

    private int RateMoveTargets(List<BoardManager.Slot> playerSlots, List<BoardManager.Slot> botSlots, List<int> moveTargets, BoardManager.Slot mySlot)
    {
        int bestIndex = -1;
        float bestEval = 0f;

        foreach (int target in moveTargets)
        {
            float curEval = EvaluateMoveUnit(mySlot, botSlots[target - 1], playerSlots);
            if (bestIndex == -1 || curEval > bestEval)
            {
                bestEval = curEval;
                bestIndex = target;
            }
            Debug.Log(curEval);
        }
        
        return bestIndex;
    
    }

    public BotMove GetNextMove(List<BoardManager.Slot> playerSlots, List<BoardManager.Slot> botSlots)
    {
        if (!botActive || GameController.eventQueue.Count != 0)
        {
            return null;
        }

        InitDeck();

        HandManager handManager = GameObject.Find("Hand").GetComponent<HandManager>();
        for (int i = 0; i < handManager.GetNumberOfOpponentsCards() - botHand.Count; ++i)
        {
            CardTypes newCard = DrawCard();
            if (newCard == CardTypes.Hatapon)
            {
                break;
            }
            botHand.Add(newCard);
        }
        int numberOfCadsInBotHand = botHand.Count;
        for (int i = 0; i < numberOfCadsInBotHand - handManager.GetNumberOfOpponentsCards(); ++i)
        {
            botHand.RemoveAt(0);
        }

        if (!deckSend)
        {
            deckSend = true;
            BotMove deckMove = new BotMove();
            deckMove.deck = DeckManager.GetEncodedDeck(runes:true);
            return deckMove;
        }

        Debug.Log($"Bot level={botLevel}");
        if (botLevel < 0)
        {
            // End Turn
            BotMove endTurnMove = new BotMove();
            endTurnMove.endTurn = true;
            botActive = false;
            return endTurnMove;
        }

        //Attack
        foreach (BoardManager.Slot slot in botSlots)
        {
            if (!slot.GetFree())
            {
                int slotIdx = slot.GetIndex();
                MinionManager minion = slot.GetConnectedMinion();
                if (minion.GetCanAttackBot())
                {
                    List<int> attackPositions = new List<int>();
                    if (!playerSlots[slotIdx].GetFree() && !playerSlots[slotIdx].GetConnectedMinion().GetCardStats().flying)
                    {
                        attackPositions.Add(slotIdx + 1);
                    }
                    if (slotIdx > 0 && !playerSlots[slotIdx - 1].GetFree() && !playerSlots[slotIdx - 1].GetConnectedMinion().GetCardStats().flying)
                    {
                        attackPositions.Add(slotIdx);
                    }
                    if (slotIdx < 6 && !playerSlots[slotIdx + 1].GetFree() && !playerSlots[slotIdx + 1].GetConnectedMinion().GetCardStats().flying)
                    {
                        attackPositions.Add(slotIdx + 2);
                    }
                    if (attackPositions.Count > 0)
                    {
                        int attackTarget = RateAttackTargets(playerSlots, botSlots, attackPositions, slot);
                        if (attackTarget != -1)
                        {
                            BotMove __move = new BotMove();
                            __move.attackCell = attackTarget;
                            __move.cellNumber = slotIdx + 1;
                            return __move;
                        }
                    }
                }
            }
        }

        //Exchange
        foreach (BoardManager.Slot slot in botSlots)
        {
            if (!slot.GetFree() && slot.GetConnectedMinion().GetCardType() == CardTypes.Hatapon)
            {
                int slotIdx = slot.GetIndex();
                MinionManager minion = slot.GetConnectedMinion();
                if (minion.GetCanMoveBot())
                {
                    List<int> movePositions = new List<int>();
                    if (slotIdx > 0 && !botSlots[slotIdx - 1].GetFree() && botSlots[slotIdx - 1].GetConnectedMinion().GetCanMoveBot())
                    {
                        movePositions.Add(slotIdx);
                    }
                    if (slotIdx < 6 && !botSlots[slotIdx + 1].GetFree() && botSlots[slotIdx + 1].GetConnectedMinion().GetCanMoveBot())
                    {
                        movePositions.Add(slotIdx + 2);
                    }
                    if (movePositions.Count > 0)
                    {
                        int currentMoveIdx = slotIdx + 1;
                        movePositions.Add(currentMoveIdx);

                        int moveTarget = RateMoveTargets(playerSlots, botSlots, movePositions, slot);
                        if (moveTarget != currentMoveIdx)
                        {
                            BotMove __move = new BotMove();
                            __move.moveCell = moveTarget;
                            __move.cellNumber = slotIdx + 1;
                            __move.exchange = true;
                            return __move;
                        }
                    }
                }
            }
        }

        //Move
        foreach (BoardManager.Slot slot in botSlots)
        {
            if (!slot.GetFree())
            {
                int slotIdx = slot.GetIndex();
                MinionManager minion = slot.GetConnectedMinion();
                if (minion.GetCanMoveBot())
                {
                    List<int> movePositions = new List<int>();
                    if (slotIdx > 0 && botSlots[slotIdx - 1].GetFree())
                    {
                        movePositions.Add(slotIdx);
                    }
                    if (slotIdx < 6 && botSlots[slotIdx + 1].GetFree())
                    {
                        movePositions.Add(slotIdx + 2);
                    }
                    if (movePositions.Count > 0)
                    {
                        int currentMoveIdx = slotIdx + 1;
                        movePositions.Add(currentMoveIdx);

                        int moveTarget = RateMoveTargets(playerSlots, botSlots, movePositions, slot);
                        if (moveTarget != currentMoveIdx)
                        {
                            BotMove __move = new BotMove();
                            __move.moveCell = moveTarget;
                            __move.cellNumber = slotIdx + 1;
                            return __move;
                        }
                    }
                }
            }
        }

        // Play card
        if (!cardPlayed && handManager.GetNumberOfOpponentsCards() > 0 && botHand.Count > 0
            && !HasEnoughAttackersForHatapon(playerSlots, botSlots))
        {
            List<BoardManager.Slot> freeSlots = new List<BoardManager.Slot>();
            foreach (BoardManager.Slot slot in botSlots)
            {
                if (slot.GetFree())
                {
                    freeSlots.Add(slot);
                }
            }

            // Mirrors CardManager.PlayCard's own firstTurnToPlay gate for the human player
            // (cards like Kuwagattan/Dekapon are unplayable before their own set turn number) -
            // the bot's move-picking never went through that method (it's server/message-driven,
            // not the local click-to-play flow), so nothing was stopping it from dropping a
            // turn-3-only bomb on turn 1 until now.
            GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
            int botTurnNumber = gameController.gameState.enemyTurnNumber;
            List<CardTypes> playableHand = botHand.FindAll(card => CardTypeToStats.GetCardStats(card).firstTurnToPlay < botTurnNumber);

            if (freeSlots.Count > 0 && playableHand.Count > 0)
            {
                // Levels below 3 just grab a random card from hand - only Smart Bot (level 3)
                // actually weighs which one is worth playing (see ChooseCardToPlaySmart).
                CardTypes plyedCard = botLevel >= 3
                    ? ChooseCardToPlaySmart(playableHand)
                    : playableHand[UnityEngine.Random.Range(0, playableHand.Count)];
                BoardManager.Slot slotToPlay = RateSlotsToPlayCard(playerSlots, botSlots, freeSlots, plyedCard);//freeSlots[UnityEngine.Random.Range(0, freeSlots.Count)];
                botHand.Remove(plyedCard);
                BotMove playCardMove = new BotMove();
                playCardMove.playCard = plyedCard;
                playCardMove.cellNumber = slotToPlay.GetIndex() + 1;
                cardPlayed = true;
                return playCardMove;
            }
        }

        // End Turn
        BotMove move = new BotMove();
        move.endTurn = true;
        botActive = false;
        return move;
    }

    // Smart Bot's (botLevel 3) card-selection heuristic - the one piece every other bot level
    // leaves to UnityEngine.Random, which is the main reason they read as "random" rather than
    // "not very good." This deliberately doesn't look ahead or plan multi-turn combos (that's the
    // "not perfect, no complex strategy" half of the brief) - it just greedily values each card in
    // hand right now and plays the best-looking one, the same way a beginner human player would
    // eyeball their hand rather than calculate.
    private CardTypes ChooseCardToPlaySmart(List<CardTypes> hand)
    {
        CardTypes bestCard = hand[0];
        float bestScore = float.MinValue;
        foreach (CardTypes card in hand)
        {
            float score = ScoreCardForPlay(card);
            if (score > bestScore)
            {
                bestScore = score;
                bestCard = card;
            }
        }
        return bestCard;
    }

    private float ScoreCardForPlay(CardTypes card)
    {
        CardManager.CardStats stats = CardTypeToStats.GetCardStats(card);
        float score = stats.power;

        if (stats.hasShield) score += 2f;
        if (stats.hasGreatshield) score += 3f;
        if (stats.hasHaste) score += 1.5f;
        if (stats.flying) score += 1.5f;
        if (stats.hexproof) score += 1.5f;
        if (stats.hasOnDeath) score += 1f;
        if (stats.onAttackEvent != null) score += 1.5f;
        if (stats.endTurnEvent != null || stats.startTurnEvent != null) score += 1f;
        if (stats.pacifism) score -= 3f;
        if (!stats.canAttack) score -= 2f;

        // Small jitter so cards that score identically don't always resolve in the exact same
        // order every time - keeps ties from feeling robotic without making the overall choice
        // actually random.
        score += UnityEngine.Random.Range(-0.05f, 0.05f);

        return score;
    }

    private CardTypes DrawCard()
    {
        if (botDeck.Count == 0)
        {
            return CardTypes.Hatapon;
        }

        CardTypes returnValue = botDeck[UnityEngine.Random.Range(0, botDeck.Count - 1)];
        botDeck.Remove(returnValue);
        return returnValue;
    }
}