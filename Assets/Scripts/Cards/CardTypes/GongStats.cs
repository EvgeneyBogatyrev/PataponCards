using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GongStats : MonoBehaviour
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();

        const int yumiponDamage = 1;

        stats.power = 4;
        stats.description = "<b>Hexproof</b>.\n<b>On attack</b>: Deal " + yumiponDamage.ToString() + " damage to all enemy units.";
        stats.name = "Gong the Hawkeye";
        stats.runes.Add(Runes.Bow);
        stats.runes.Add(Runes.Shield);

        stats.hexproof = true;

        stats.nameSize = 5;

        static IEnumerator YumiponEndTurn(int index, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
            gameController.actionIsHappening = true;

            AnimationManager animationManager = GameObject.Find("GameController").GetComponent<AnimationManager>();
            List<SpearManager> spearArray = new List<SpearManager>();

            foreach (BoardManager.Slot slot in enemySlots)
            {
                if (!slot.GetFree())
                {
                    //slot.GetConnectedMinion().GetDamage(yumiponDamage);
                    SpearManager spear = animationManager.CreateObject(AnimationManager.Animations.Spear, friendlySlots[index].GetPosition()).GetComponent<SpearManager>();
                    spear.SetSlotToGo(slot);
                    spearArray.Add(spear);
                    if (enemySlots[index].GetFriendly())
                    {
                        spear.isEnemy = true;
                    }
                }
            }

            bool arrowsExists = true;

            while (arrowsExists)
            {
                arrowsExists = false;

                foreach (SpearManager spear in spearArray)
                {
                    if (!spear.reachDestination)
                    {
                        arrowsExists = true;
                    }
                    else if (!spear.exhausted)
                    {
                        spear.DestroySelf();
                        spear.GetSlotToGo().GetConnectedMinion().ReceiveDamage(yumiponDamage);
                        spear.exhausted = true;
                    }
                }
                yield return new WaitForSeconds(0.1f);
            }


            gameController.actionIsHappening = false;
            yield return null;
        }
        static IEnumerator YumiponOnAttack(List<int> targets, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            int thisIndex = targets[0];
            BoardManager.Slot thisSlot;
            if (thisIndex > 0)
            {
                thisIndex -= 1;
                thisSlot = friendlySlots[thisIndex];
            }
            else
            {
                thisIndex = -1 * thisIndex - 1;
                thisSlot = friendlySlots[thisIndex];
            }

            MinionManager minion = thisSlot.GetConnectedMinion();
            if (minion != null)
            {
                GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
                gameController.actionIsHappening = true;

                AnimationManager animationManager = GameObject.Find("GameController").GetComponent<AnimationManager>();
                Vector3 startPosition = new Vector3(-10f, enemySlots[thisIndex].GetPosition().y, enemySlots[thisIndex].GetPosition().z);
                SpearManager tornado = animationManager.CreateObject(AnimationManager.Animations.Spear, startPosition).GetComponent<SpearManager>();
                tornado.gameObject.transform.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Images/gong_tornado");
                tornado.SetSlotToGo(enemySlots[0]);
                tornado.speed = 15f;
                tornado.constantSpeed = true;

                List <SpearManager> ghostly = new();
                for (int i = 0; i < 3; ++i)
                {
                    yield return new WaitForSeconds(0.03f);
                    SpearManager ghost = animationManager.CreateObject(AnimationManager.Animations.Spear, startPosition).GetComponent<SpearManager>();
                    ghost.gameObject.transform.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Images/gong_tornado");  
                    var tmp = ghost.gameObject.transform.GetComponent<SpriteRenderer>().color;
                    tmp.a = 0.7f - i * 0.2f;
                    ghost.gameObject.transform.GetComponent<SpriteRenderer>().color = tmp;
                    ghost.SetSlotToGo(enemySlots[0]);
                    ghost.speed = 15f;
                    ghost.rotate = false;
                    ghost.constantSpeed = true;
                    ghostly.Add(ghost);
                }

                int idx = 0;
                while (idx < 7)
                {
                    if (tornado.reachDestination)
                    {
                        tornado.reachDestination = false;
                        BoardManager.Slot _slot = enemySlots[idx];    
                        if (!_slot.GetFree())
                        {
                            _slot.GetConnectedMinion().transform.position = new Vector3(_slot.GetConnectedMinion().transform.position.x, _slot.GetConnectedMinion().transform.position.y + 1f, _slot.GetConnectedMinion().transform.position.z);
                            _slot.GetConnectedMinion().ReceiveDamage(yumiponDamage);
                        }
                        if (idx < 6)
                        {
                            tornado.SetSlotToGo(enemySlots[idx + 1]);
                        
                            foreach (SpearManager ghostlyTornado in ghostly)
                            {
                                ghostlyTornado.reachDestination = false;
                                ghostlyTornado.SetSlotToGo(enemySlots[idx + 1]);
                            }
                            idx += 1;
                        }
                        else
                        {
                            foreach (SpearManager ghostlyTornado in ghostly)
                            {
                                ghostlyTornado.DestroySelf();
                            }
                            tornado.DestroySelf();
                            break;
                        }
                    }
                    else
                    {
                        yield return new WaitForSeconds(0.2f);
                    }
                }

                minion.onAttackActionProgress = false;
                gameController.actionIsHappening = false;
            }
            
            yield return null;
        }

        stats.onAttackEvent = YumiponOnAttack;

        stats.imagePath = "gong_the_hawkeye";
        return stats;
    }
}
