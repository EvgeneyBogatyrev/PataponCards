using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Spell/attack targeting arrow. Previously built from a chain of tiny ArrowBall sprites
// destroyed and reinstantiated fresh every single frame (expensive, and visually just a string
// of dark reddish dots using Unity's built-in default sprites) - now a single LineRenderer shaft
// with a procedurally-generated dashed texture that scrolls to read as "flowing toward the
// target," plus the same ArrowHead sprite at the tip, recolored to the game's actual gold/amber
// theme (UITheme.primary/primaryHover) instead of the old flat pure red.
public class Arrow
{
    private static readonly Color ShaftColor = new Color(0.86f, 0.71f, 0.30f, 0.85f);
    private static readonly Color HeadColor = new Color(0.95f, 0.80f, 0.35f, 1f);

    private GameObject arrowHeadPrefab;
    private GameObject arrowHead;
    private GameObject shaftObject;
    private LineRenderer shaft;

    private Vector3 startPosition;
    private Vector3 endPosition;
    private float zLevel = -4f;

    // World units per dash+gap repeat, and how fast the dash pattern scrolls toward the target -
    // tuned by eye, not tied to any other constant.
    private const float DashWorldSize = 0.35f;
    private const float FlowSpeed = 1.5f;

    public Arrow(Vector3 startPosition)
    {
        arrowHeadPrefab = Resources.Load<GameObject>("Prefabs/ArrowHead");
        this.startPosition = new Vector3(startPosition.x, startPosition.y, zLevel);
        arrowHead = GameObject.Instantiate(arrowHeadPrefab, startPosition, Quaternion.identity);
        RecolorHead();
        CreateShaft();

        UpdatePosition();
    }

    public Arrow(Vector3 startPosition, Vector3 endPosition)
    {
        arrowHeadPrefab = Resources.Load<GameObject>("Prefabs/ArrowHead");
        this.startPosition = new Vector3(startPosition.x, startPosition.y, zLevel);
        arrowHead = GameObject.Instantiate(arrowHeadPrefab, startPosition, Quaternion.identity);
        RecolorHead();
        CreateShaft();

        this.endPosition = endPosition;

        UpdatePosition();
    }

    private void RecolorHead()
    {
        SpriteRenderer headRenderer = arrowHead.GetComponent<SpriteRenderer>();
        if (headRenderer != null)
        {
            headRenderer.color = HeadColor;
        }
    }

    private void CreateShaft()
    {
        shaftObject = new GameObject("ArrowShaft");
        shaft = shaftObject.AddComponent<LineRenderer>();
        shaft.material = new Material(Shader.Find("Sprites/Default")) { mainTexture = DashTexture() };
        shaft.textureMode = LineTextureMode.Tile;
        shaft.numCapVertices = 4;
        shaft.numCornerVertices = 0;
        shaft.positionCount = 2;
        shaft.useWorldSpace = true;
        shaft.startColor = ShaftColor;
        shaft.endColor = ShaftColor;
        shaft.startWidth = 0.16f;
        shaft.endWidth = 0.24f;
        shaft.sortingOrder = -1;
    }

    // A small repeating opaque-then-transparent strip, generated once at runtime rather than
    // shipped as an art asset - scrolling its UV offset each frame (see UpdatePosition) is what
    // makes the shaft read as flowing toward the target instead of a static bar.
    private static Texture2D _dashTexture;
    private static Texture2D DashTexture()
    {
        if (_dashTexture == null)
        {
            const int width = 32;
            _dashTexture = new Texture2D(width, 1, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Bilinear
            };
            for (int x = 0; x < width; ++x)
            {
                bool solid = x < width * 0.6f;
                _dashTexture.SetPixel(x, 0, solid ? Color.white : new Color(1f, 1f, 1f, 0f));
            }
            _dashTexture.Apply();
        }
        return _dashTexture;
    }

    public void DestroyArrow()
    {
        GameObject.Destroy(arrowHead);
        GameObject.Destroy(shaftObject);
    }

    public void UpdatePosition()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition = new Vector3(mousePosition.x, mousePosition.y, zLevel);
        DrawShaftAndHead(startPosition, mousePosition);
    }

    public void UpdatePosition(Vector3 startPos)
    {
        Vector3 target = new Vector3(endPosition.x, endPosition.y, zLevel);
        DrawShaftAndHead(startPos, target);
    }

    private void DrawShaftAndHead(Vector3 from, Vector3 to)
    {
        shaft.SetPosition(0, from);
        shaft.SetPosition(1, to);

        float length = Vector3.Distance(from, to);
        float repeats = Mathf.Max(1f, length / DashWorldSize);
        shaft.material.mainTextureScale = new Vector2(repeats, 1f);
        shaft.material.mainTextureOffset = new Vector2(-Time.time * FlowSpeed, 0f);

        Vector3 direction = (to - from).normalized;
        arrowHead.transform.position = to - direction * 0.5f;
        arrowHead.transform.rotation = Rotation(from, to);
    }

    private Quaternion Rotation(Vector3 from, Vector3 to)
    {
        Vector3 directionVector = (to - from);

        Vector2 direction = new Vector2(directionVector.x, directionVector.y).normalized;

        float angle = Vector2.Angle(direction, new Vector2(Mathf.Sign(to.y - from.y), 0f));

        return Quaternion.Euler(0.0f, 0.0f, angle + 90f * Mathf.Sign(to.y - from.y));

    }


}
