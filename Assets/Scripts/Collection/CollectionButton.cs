using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CollectionButton : MonoBehaviour
{
    public UITheme uiTheme;
    private CollectionControl collection;
    private SpriteRenderer background;

    public bool mouseOver = false;

    public int status = -1;

    private void Start()
    {
        collection = GameObject.Find("Collection").GetComponent<CollectionControl>();
        background = GetComponent<SpriteRenderer>();
    }
    private void Update()
    {
        if (mouseOver && Input.GetMouseButtonDown(0))
        {
            switch (status)
            {
                case 0:
                    collection.LoadNextPage();
                    break;

                case 1:
                    collection.LoadPrevPage();
                    break;

                case 2:
                    collection.BackButton();
                    break;

                case 3:
                    // Delete-deck - opens a Yes/Cancel confirmation instead of deleting on the
                    // spot (used to require a press-and-hold instead).
                    collection.RequestDeckDeleteConfirmation();
                    break;

                default:
                    break;
            }
        }

        // status 3 is the delete-deck button - skin it as a danger action.
        WorldButtonSkin.Apply(background, uiTheme, danger: status == 3, hovered: mouseOver, pressed: mouseOver && Input.GetMouseButton(0));
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
