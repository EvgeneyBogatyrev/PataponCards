using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    public enum Animations 
    {
        Spear
    }

    public GameObject spearPrefab;

    public GameObject CreateObject(Animations type, Vector3 position)
    {
        GameObject gameObject = null;
        switch (type)
        {
            case Animations.Spear:
                gameObject = Instantiate(spearPrefab);
                gameObject.transform.position = position;
                break;

            default:
                Debug.Log("Wrong animation, idiot");
                break;
        }
        return gameObject;
    }
}