using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpearManager : MonoBehaviour
{
    private BoardManager.Slot slotToGo;
    private float threshold = 1f;
    private float zLevel = -0.1f;
    public bool reachDestination = false;
    public bool exhausted = false;
    private bool rotationSet = false;

    public bool isEnemy = false;

    public void SetSlotToGo(BoardManager.Slot slot)
    {
        slotToGo = slot;
    }


    public BoardManager.Slot GetSlotToGo()
    {
        return slotToGo;
    }

    public void DestroySelf()
    {
        Destroy(transform.gameObject);
    }

    private float Distance(Vector3 from, Vector3 to)
    {
        Vector2 from2D = new Vector2(from.x, from.y);
        Vector2 to2D = new Vector2(to.x, to.y);
        return (from2D - to2D).magnitude;
    }

    private Quaternion Rotation(Vector3 from, Vector3 to)
    {
        Vector3 directionVector = (to - from);
        
        Vector2 direction = new Vector2(directionVector.x, directionVector.y).normalized;

        float angle = Vector2.Angle(direction, new Vector2(1f, 0f));

        if (isEnemy)
        {
            angle *= -1;
        }

        return Quaternion.Euler(0.0f, 0.0f, angle);
        
    }

    void Update()
    {
        Vector3 targetPosition = slotToGo.GetSlotObject().transform.position;
        transform.position += (targetPosition - transform.position).normalized *  Time.deltaTime * 30f;
        transform.position = new Vector3(transform.position.x, transform.position.y, zLevel);

        if (!rotationSet)
        {
            transform.rotation = Rotation(transform.position, slotToGo.GetSlotObject().transform.position);
            rotationSet = true;
        }

        if (Distance(targetPosition, transform.position) < threshold)
        {
            reachDestination = true;
        }
    }

    
}
