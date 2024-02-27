using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


//Rewrite this entire piece of sheesh
public static class CardGenerator
{
    public static CardManager.CardStats GetCardStats(CardTypes cardType)
    {
        static bool EmptyCheckSpellTargets(List<int> targets, List<BoardManager.Slot> slots1, List<BoardManager.Slot> slots2) { return true; }
        static bool EmptyCheckSpellTarget(int target, List<BoardManager.Slot> slots1, List<BoardManager.Slot> slots2) { return true; }

        CardManager.CardStats stats = CardTypeToStats.GetCardStats(cardType);
        
        if (stats.pacifism)
        {
            stats.canAttack = false;
            stats.canDealDamage = false;
            stats.limitedVision = true;
        }

        // Those checks will always return true
        // For spells with no limits
        if (stats.checkSpellTarget == null) stats.checkSpellTarget = EmptyCheckSpellTarget;
        if (stats.checkSpellTargets == null) stats.checkSpellTargets = EmptyCheckSpellTargets;

        return stats;
    }

    public static void CustomizeCard(CardManager card, CardTypes cardType)
    {
        card.SetCardType(cardType);
        CardManager.CardStats stats = GetCardStats(cardType);
        
        card.SetCardStats(stats);
        card.SetPower(stats.power);
        card.SetDescription(stats.description);
        card.SetNameSize(stats.nameSize);
        card.SetDescriptionSize(stats.descriptionSize);
        card.SetName(stats.name);
        card.imageObject.GetComponent<SpriteRenderer>().sprite = stats.GetSprite();

        if (stats.isSpell)
        {
            card.powerObject.SetActive(false);
            card.powerSquare.SetActive(false);
            card.heartObject.SetActive(false);
        }        

        int spearCount = 0;
        int shieldCount = 0;
        int bowCount = 0;

        foreach (Runes rune in stats.runes)
        {
            if (rune == Runes.Spear)
            {
                spearCount += 1;
            }
            else if (rune == Runes.Shield)
            {
                shieldCount += 1;
            }
            else if (rune == Runes.Bow)
            {
                bowCount += 1;
            }
        }
    
        MeshRenderer meshRenderer = card.nameOutline.GetComponent<MeshRenderer>();
        var materialsCopy = meshRenderer.materials;

        if (spearCount == 0 && shieldCount == 0 && bowCount == 0)
        {
            materialsCopy[0] = card.neutralMaterial;
        }
        else if (spearCount > 0 && shieldCount == 0 && bowCount == 0)
        {
            materialsCopy[0] = card.spearMaterial;
        }
        else if (spearCount == 0 && shieldCount > 0 && bowCount == 0)
        {
            materialsCopy[0] = card.shieldMaterial;
        } 
        else if (spearCount == 0 && shieldCount == 0 && bowCount > 0)
        {
            materialsCopy[0] = card.bowMaterial;
        }
        else
        {
            materialsCopy[0] = card.multiclassMaterial;
        }

        meshRenderer.materials = materialsCopy;

        if (stats.isStatic || stats.canAttack == false)
        {
            card.powerSquare.SetActive(false);
        }
        else
        {
            card.heartObject.SetActive(false);
        }

        
        while (stats.runes.Count < 3)
        {
            stats.runes.Add(Runes.Neutral);
        }

        MeshRenderer _meshRenderer;
        foreach (var (key, value) in Enumerable.Zip(stats.runes, card.runeObjects, (key, value) => (key, value)))
        {
            switch (key)
            {
                case Runes.Neutral:
                    value.SetActive(false);
                    break;

                case Runes.Spear:
                    _meshRenderer = value.GetComponent<MeshRenderer>();
                    var _materialsCopy = _meshRenderer.materials;
                    _materialsCopy[0] = card.spearMaterial;
                    _meshRenderer.materials = _materialsCopy;
                    break;

                case Runes.Shield:
                    _meshRenderer = value.GetComponent<MeshRenderer>();
                    var __materialsCopy = _meshRenderer.materials;
                    __materialsCopy[0] = card.shieldMaterial;
                    _meshRenderer.materials = __materialsCopy;
                    break;

                case Runes.Bow:
                    _meshRenderer = value.GetComponent<MeshRenderer>();
                    var ___materialsCopy = _meshRenderer.materials;
                    ___materialsCopy[0] = card.bowMaterial;
                    _meshRenderer.materials = ___materialsCopy;
                    break;
            }    
        }

    }
}

