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

    private static CursorStates _cursorState;

    // Locked to EnemyTurn for the whole match while spectating - a spectator has no local side
    // of its own to act with, and CursorStates.Free/Select/Hold/ChooseOption already gate the
    // large majority of click-to-play/attack/target logic scattered throughout
    // CardManager/MinionManager, so pinning this one place makes all of that inert without
    // having to touch every individual call site. (EnemyTurn, not some other state, because a
    // few read-only hover/preview checks explicitly allow Free OR EnemyTurn - locking to
    // EnemyTurn keeps those working for a spectator instead of also breaking hover previews.)
    public static CursorStates cursorState
    {
        get { return _cursorState; }
        set { _cursorState = InfoSaver.isSpectator ? CursorStates.EnemyTurn : value; }
    }
}
