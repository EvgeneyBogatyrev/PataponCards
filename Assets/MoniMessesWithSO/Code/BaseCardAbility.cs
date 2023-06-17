using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseCardAbility : MonoBehaviour
{
    public virtual IEnumerator OnPlayEvent()
    {
        yield return null;
    }

    public virtual IEnumerator OnDeathEvent()
    {
        yield return null;
    }

    public virtual IEnumerator EndTurnEvent()
    {
        yield return null;
    }
}
