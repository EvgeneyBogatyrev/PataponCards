using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShakeScreen : MonoBehaviour
{
    public bool start = false;
    public float duration = 0.5f;
    public float basicStrength = 2f;
    public AnimationCurve curve;


    public void shakeTheScreen(float strength_)
    {
        start = true;
        basicStrength = strength_;
    }
    
    void Update()
    {
        if (start)
        {
            start = false;
            StartCoroutine(Shaking());
        }
    }

    IEnumerator Shaking()
    {   
        Vector3 startPosition = transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float strength = basicStrength * curve.Evaluate(elapsedTime / duration);
            transform.position = startPosition + strength * Random.insideUnitSphere;
            yield return null;
        }

        transform.position = startPosition;
    }
}
