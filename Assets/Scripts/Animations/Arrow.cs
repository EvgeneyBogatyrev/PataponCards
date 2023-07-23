using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow
{
    private GameObject arrowHeadPrefab;
    private GameObject arrowBallPrefab;
    private GameObject arrowHead;
    private List<GameObject> trail = new List<GameObject>();
    private Vector3 startPosition;
    private Vector3 endPosition;
    private float ballsStep = 1f;
    private float zLevel = -4f;
    private float speed = 2f;

    private float lastUpdatedMax = 1f;
    private float lastUpdated = 1f;


    public Arrow (Vector3 startPosition)
    {
        arrowHeadPrefab = Resources.Load<GameObject>("Prefabs/ArrowHead");
        arrowBallPrefab = Resources.Load<GameObject>("Prefabs/ArrowBall");

        this.startPosition = new Vector3(startPosition.x, startPosition.y, zLevel);
        arrowHead = GameObject.Instantiate(arrowHeadPrefab, startPosition, Quaternion.identity);

        UpdatePosition();
    }

    public Arrow (Vector3 startPosition, Vector3 endPosition)
    {
        arrowHeadPrefab = Resources.Load<GameObject>("Prefabs/ArrowHead");
        arrowBallPrefab = Resources.Load<GameObject>("Prefabs/ArrowBall");

        this.startPosition = new Vector3(startPosition.x, startPosition.y, zLevel);
        arrowHead = GameObject.Instantiate(arrowHeadPrefab, startPosition, Quaternion.identity);

        this.endPosition = endPosition;

        UpdatePosition();
    }

    private IEnumerable Counter()
    {
        while (lastUpdated > 0)
        {
            yield return new WaitForSeconds(1f);
        }
        
        DestroyArrow();
        yield return null;
    }


    public void DestroyArrow()
    {
        GameObject.Destroy(arrowHead);

        DestroyBalls();
    }

    public void DestroyBalls()
    {
        // OPTIMIZE!!!
        foreach (GameObject ball in trail)
        {
            GameObject.Destroy(ball);
        }
        trail = new List<GameObject>();
    }
    
    private float Linearity(float arg, float boundary=Mathf.PI / 2f)
    {
        return (arg % boundary) / boundary;
    }

    public void UpdatePosition()
    {
        lastUpdated = lastUpdatedMax;
        DestroyBalls();
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition = new Vector3(mousePosition.x, mousePosition.y, zLevel);
        Vector3 curPosition = this.startPosition;
        curPosition -= (mousePosition - curPosition).normalized * ballsStep * Linearity(-Time.time * speed);
        while ((curPosition - mousePosition).magnitude > ballsStep)
        {
            GameObject ball = GameObject.Instantiate(arrowBallPrefab, curPosition, Quaternion.identity);
            trail.Add(ball);
            curPosition += (mousePosition - curPosition).normalized * ballsStep;
        }

        arrowHead.transform.position = mousePosition - (mousePosition - curPosition).normalized * 0.5f;
        arrowHead.transform.rotation = Rotation(startPosition, mousePosition);
    }

    public void UpdatePosition(Vector3 startPos)
    {
        lastUpdated = lastUpdatedMax;
        DestroyBalls();
        Vector3 mousePosition = endPosition;
        mousePosition = new Vector3(mousePosition.x, mousePosition.y, zLevel);
        Vector3 curPosition = startPos;
        curPosition -= (mousePosition - curPosition).normalized * ballsStep * Linearity(-Time.time * speed);
        while ((curPosition - mousePosition).magnitude > ballsStep)
        {
            GameObject ball = GameObject.Instantiate(arrowBallPrefab, curPosition, Quaternion.identity);
            trail.Add(ball);
            curPosition += (mousePosition - curPosition).normalized * ballsStep;
        }

        arrowHead.transform.position = mousePosition - (mousePosition - curPosition).normalized * 0.5f;
        arrowHead.transform.rotation = Rotation(startPos, endPosition);
    }

    private Quaternion Rotation(Vector3 from, Vector3 to)
    {
        Vector3 directionVector = (to - from);
        
        Vector2 direction = new Vector2(directionVector.x, directionVector.y).normalized;

        float angle = Vector2.Angle(direction, new Vector2(Mathf.Sign(to.y - from.y), 0f));

        return Quaternion.Euler(0.0f, 0.0f, angle + 90f * Mathf.Sign(to.y - from.y));
        
    }


}
