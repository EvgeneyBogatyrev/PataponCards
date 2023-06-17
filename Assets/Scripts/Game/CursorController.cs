using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorController
{
    public enum CursorStates
    {
        Free,
        Hold,
        Select,
        EnemyTurn,
        ChooseOption,
    }

    public static CursorStates cursorState;
}
