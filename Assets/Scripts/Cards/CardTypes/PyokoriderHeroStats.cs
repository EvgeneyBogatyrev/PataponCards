using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PyokoriderHeroStats
{
    public static CardManager.CardStats GetStats()
    {
        CardManager.CardStats stats = new CardManager.CardStats();
        const int pyokoDamage = 3;

        stats.power = 3;
        stats.description = "<b>End of turn</b>: Deal damage equal to this unit's power to the right-most enemy unit.";
        stats.name = "Pyokorider";
        stats.runes.Add(Runes.Spear);
        stats.runes.Add(Runes.Spear);
        stats.runes.Add(Runes.Spear);

        stats.nameSize = 4;


        static IEnumerator PyokoriderHeroEndTurn(int index, List<BoardManager.Slot> enemySlots, List<BoardManager.Slot> friendlySlots)
        {
            GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
            gameController.actionIsHappening = true;
            MinionManager thisMinion = friendlySlots[index].GetConnectedMinion();

            MinionManager connectedMinion = null;
            foreach (BoardManager.Slot slot in enemySlots)
            {
                if (slot.GetConnectedMinion() != null)
                {
                    connectedMinion = slot.GetConnectedMinion();
                }
            }
            
            if (connectedMinion != null)
            {

                if (connectedMinion != null)
                {
                    AnimationManager animationManager = GameObject.Find("GameController").GetComponent<AnimationManager>();
                    
                    // Main projectile
                    SpearManager spear = animationManager.CreateObject(AnimationManager.Animations.Spear, friendlySlots[index].GetPosition()).GetComponent<SpearManager>();
                    spear.gameObject.transform.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Images/pyokorider_hero");
                    spear.SetSlotToGo(connectedMinion.GetSlot());
                    spear.speed = 20f;
                    spear.rotate = false;
                    spear.constantSpeed = true;

                    List <SpearManager> ghostly = new();
                    for (int i = 0; i < 3; ++i)
                    {
                        yield return new WaitForSeconds(0.03f);
                        SpearManager ghost = animationManager.CreateObject(AnimationManager.Animations.Spear, friendlySlots[index].GetPosition()).GetComponent<SpearManager>();
                        ghost.gameObject.transform.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Images/pyokorider_hero");  
                        var tmp = ghost.gameObject.transform.GetComponent<SpriteRenderer>().color;
                        tmp.a = 0.7f - i * 0.2f;
                        ghost.gameObject.transform.GetComponent<SpriteRenderer>().color = tmp;
                        ghost.SetSlotToGo(connectedMinion.GetSlot());
                        ghost.speed = 20f;
                        ghost.rotate = false;
                        ghost.constantSpeed = true;
                        ghostly.Add(ghost);
                    }

                    while (!spear.reachDestination)
                    {
                        yield return new WaitForSeconds(0.1f);
                    }

                    connectedMinion.ReceiveDamage(thisMinion.GetPower());

                    while (!spear.outOfScreen)
                    {
                        yield return new WaitForSeconds(0.1f);
                    }

                    spear.DestroySelf(); 
                    foreach (SpearManager ghost in ghostly)
                    {
                        ghost.DestroySelf();
                    }                   
                    
                }
                gameController.actionIsHappening = false;
            }
            //GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
            //gameController.actionIsHappening = false;
            yield return null;
        }
        stats.endTurnEvent = PyokoriderHeroEndTurn;

        stats.imagePath = "pyokorider_hero";
        stats.artistName = "Official render";

        return stats;
    }
}
