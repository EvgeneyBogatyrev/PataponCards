using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SpamtonAbility : BaseCardAbility
{
    public override IEnumerator OnPlayEvent()
    {
        Debug.Log("BIG SHOT!");
        yield return null;
    }

    public override IEnumerator OnDeathEvent()
    {
        Debug.Log("spam is dead. no spam.");
        yield return null;
    }

    public override IEnumerator EndTurnEvent()
    {
        Debug.Log("until next time");
        yield return null;
    }
}

