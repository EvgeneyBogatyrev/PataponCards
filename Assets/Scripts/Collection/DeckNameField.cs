using UnityEngine;
using TMPro;

// A physical (non-uGUI) editable text field for the current deck's name, shown only in the
// Collection scene - matches every other physical button/toggle here (SpriteRenderer +
// OnMouseOver/Input-driven, requires a Collider for OnMouseOver/OnMouseExit to fire), rather than
// a Canvas TMP_InputField. Click to start editing: raw keystrokes are captured via
// Input.inputString, which Unity already delivers pre-filtered to printable characters plus
// '\b' (backspace) and '\n'/'\r' (enter/return) each frame - no Canvas/EventSystem needed for
// that, unlike TMP_InputField. Enter, clicking elsewhere, or losing focus commits; Escape cancels
// back to the last saved name.
public class DeckNameField : MonoBehaviour
{
    public GameObject label;

    private bool mouseOver = false;
    private bool editing = false;
    private string editBuffer = "";
    private string savedName = SaveSystem.NewDeckName;

    private void Start()
    {
        savedName = SaveSystem.LoadDeckName(DeckLoadManager.deckIndex);
        SetLabelText(savedName);
    }

    private void Update()
    {
        if (!editing)
        {
            if (mouseOver && Input.GetMouseButtonDown(0))
            {
                BeginEditing();
            }
            return;
        }

        foreach (char c in Input.inputString)
        {
            if (c == '\b')
            {
                if (editBuffer.Length > 0)
                {
                    editBuffer = editBuffer.Substring(0, editBuffer.Length - 1);
                }
            }
            else if (c == '\n' || c == '\r')
            {
                CommitEditing();
                return;
            }
            else if (!char.IsControl(c) && TextValidation.IsLatinChar(c) && editBuffer.Length < SaveSystem.MaxDeckNameLength)
            {
                editBuffer += c;
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelEditing();
            return;
        }

        // A click anywhere else while editing commits, same as a normal text field losing focus -
        // guarded so the same click that opened editing (still mouseOver that frame) doesn't
        // immediately close it again.
        if (Input.GetMouseButtonDown(0) && !mouseOver)
        {
            CommitEditing();
            return;
        }

        SetLabelText(editBuffer + "_");
    }

    private void BeginEditing()
    {
        editing = true;
        // Starting from an untouched placeholder name (an empty slot's "New deck", or the seeded
        // starter deck's still-unrenamed "Default deck") clears it first, rather than making the
        // player delete it themselves before typing their own name.
        editBuffer = (savedName == SaveSystem.NewDeckName || savedName == SaveSystem.DefaultDeckName) ? "" : savedName;
        SetLabelText(editBuffer + "_");
    }

    private void CommitEditing()
    {
        editing = false;
        SaveSystem.SaveDeckName(editBuffer, DeckLoadManager.deckIndex);
        savedName = SaveSystem.LoadDeckName(DeckLoadManager.deckIndex);
        SetLabelText(savedName);
    }

    private void CancelEditing()
    {
        editing = false;
        SetLabelText(savedName);
    }

    // Called by CollectionControl before leaving the scene, so an in-progress edit the player
    // never clicked away from (e.g. they went straight from typing to the Back button) still saves
    // instead of silently reverting.
    public void CommitIfEditing()
    {
        if (editing)
        {
            CommitEditing();
        }
    }

    private void SetLabelText(string text)
    {
        if (label != null)
        {
            label.GetComponent<TextMeshPro>().text = text;
        }
    }

    private void OnMouseOver()
    {
        mouseOver = true;
    }
    private void OnMouseExit()
    {
        mouseOver = false;
    }
}
