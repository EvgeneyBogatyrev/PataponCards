using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

// Renders every collectable card exactly as CardGenerator.CustomizeCard builds it in-game
// (art, name, description, power, rune coloring) to its own PNG, one per CardTypes entry from
// SaveSystem.GetCollectableCards() - the same "real, ownable card" list used everywhere else
// (packs, deck building), so tokens/spell-option cards (TokenTatepon, Motiti_option1, etc.) are
// skipped. Runs entirely in Edit mode - CustomizeCard has no scene dependencies (no
// GameObject.Find of Board/Hand/GameController), and CardState.hilightOver is the same
// no-scene-context-needed display state already used for every hover/info-popup preview
// elsewhere in the game, so nothing here requires Play Mode.
public static class CardPngExporter
{
    private const string OutputFolder = "Assets/Generated/CardExports";
    private const string CardPrefabPath = "Assets/Prefabs/Card.prefab";

    // Orthographic camera size = card's own rendered height/width plus this fraction of padding
    // on each side, computed per-card from its actual renderer bounds rather than a hardcoded
    // guess - keeps every export framed consistently even if a card's art/description box is a
    // slightly different size from another's.
    private const float PaddingFraction = 0.08f;
    private const int TextureResolution = 1024;

    [MenuItem("Tools/Cards/Export All Cards as PNG")]
    public static void ExportAll()
    {
        GameObject cardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CardPrefabPath);
        if (cardPrefab == null)
        {
            Debug.LogError("CardPngExporter: couldn't load " + CardPrefabPath);
            return;
        }

        if (!AssetDatabase.IsValidFolder("Assets/Generated"))
        {
            AssetDatabase.CreateFolder("Assets", "Generated");
        }
        if (!AssetDatabase.IsValidFolder(OutputFolder))
        {
            AssetDatabase.CreateFolder("Assets/Generated", "CardExports");
        }

        List<CardTypes> cardTypes = SaveSystem.GetCollectableCards();
        GameObject cameraObject = new GameObject("CardPngExporter_Camera");
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.clearFlags = CameraClearFlags.SolidColor;
        // Alpha 0 - the card's own opaque backObject/frame sprite still renders normally, this
        // just keeps the padding AROUND the card transparent instead of some arbitrary fill color.
        camera.backgroundColor = new Color(0f, 0f, 0f, 0f);
        camera.cullingMask = ~0;

        RenderTexture renderTexture = new RenderTexture(TextureResolution, TextureResolution, 24, RenderTextureFormat.ARGB32);
        camera.targetTexture = renderTexture;

        try
        {
            int done = 0;
            foreach (CardTypes cardType in cardTypes)
            {
                ExportOne(cardPrefab, camera, renderTexture, cardType);
                done++;
                EditorUtility.DisplayProgressBar("Exporting cards", cardType.ToString(), (float)done / cardTypes.Count);
            }
            Debug.Log("CardPngExporter: exported " + done + " cards to " + OutputFolder);
        }
        finally
        {
            EditorUtility.ClearProgressBar();
            camera.targetTexture = null;
            Object.DestroyImmediate(renderTexture);
            Object.DestroyImmediate(cameraObject);
            AssetDatabase.Refresh();
        }
    }

    private static void ExportOne(GameObject cardPrefab, Camera camera, RenderTexture renderTexture, CardTypes cardType)
    {
        GameObject cardObject = Object.Instantiate(cardPrefab, Vector3.zero, Quaternion.identity);
        CardManager card = cardObject.GetComponent<CardManager>();

        // Editor-instantiated outside Play Mode never runs Start(), which is where these
        // normally get force-hidden - set explicitly instead of relying on it.
        if (card.numberOfCardsObject != null) card.numberOfCardsObject.SetActive(false);
        if (card.lockObject != null) card.lockObject.SetActive(false);
        if (card.grayTiltObject != null) card.grayTiltObject.SetActive(false);

        CardGenerator.CustomizeCard(card, cardType);
        card.SetCardState(CardManager.CardState.hilightOver);

        Bounds bounds = ComputeBounds(cardObject);
        FrameCamera(camera, bounds);

        RenderTexture.active = renderTexture;
        camera.Render();

        Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);
        texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture.Apply();

        byte[] png = texture.EncodeToPNG();
        string path = OutputFolder + "/" + cardType + ".png";
        File.WriteAllBytes(path, png);

        Object.DestroyImmediate(texture);
        RenderTexture.active = null;
        Object.DestroyImmediate(cardObject);
    }

    private static Bounds ComputeBounds(GameObject cardObject)
    {
        Renderer[] renderers = cardObject.GetComponentsInChildren<Renderer>();
        Bounds bounds = renderers.Length > 0 ? renderers[0].bounds : new Bounds(cardObject.transform.position, Vector3.one);
        foreach (Renderer r in renderers)
        {
            bounds.Encapsulate(r.bounds);
        }
        return bounds;
    }

    private static void FrameCamera(Camera camera, Bounds bounds)
    {
        float halfExtent = Mathf.Max(bounds.extents.x, bounds.extents.y);
        camera.orthographicSize = halfExtent * (1f + PaddingFraction);
        camera.transform.position = new Vector3(bounds.center.x, bounds.center.y, bounds.min.z - 10f);
        camera.transform.rotation = Quaternion.identity;
        camera.nearClipPlane = 0.1f;
        camera.farClipPlane = bounds.size.z + 20f;
    }
}
