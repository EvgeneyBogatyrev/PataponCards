using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class DeckLoadManager
{
    public static string roomToGo = "Collection";
    public static int deckIndex = 0;
}

public class DecksController : MonoBehaviour
{
    private int maxDecks = 6;

    public GameObject deckSlotPrefab;

    public float startPositionX = -1f; 
    public float startPositionY = -1f; 

    public float shiftX = 1f;
    public float shiftY = 1f;


    public IEnumerator Start()
    {
        float curX = startPositionX;
        float curY = startPositionY;
        for (int i = 0; i < maxDecks; ++i)
        {
            List<Runes> runes = SaveSystem.LoadRunes(i);
            while (SaveSystem.loading)
            {
                Debug.Log("Loading");
                yield return new WaitForSeconds(0.1f);
            }
            if (runes.Count == 0)
            {
                if (DeckLoadManager.roomToGo == "Collection")
                {
                    GameObject _deckSlot = Instantiate(deckSlotPrefab, new Vector3(curX, curY, 0f), Quaternion.identity);
                    _deckSlot.GetComponent<DeckSlotManager>().Customize(null, i, "New Deck");
                }
                break;
            }
            
            GameObject deckSlot = Instantiate(deckSlotPrefab, new Vector3(curX, curY, 0f), Quaternion.identity);
            deckSlot.GetComponent<DeckSlotManager>().Customize(runes, i);
            
            curX += shiftX;
            //curY += shiftY;

            if (i % 2 == 1)
            {
                curX = startPositionX;
                curY -= shiftY;
            }
        }

        yield return null;
    }

}
