using System.IO;
using UnityEditor;
using UnityEngine;

// Bakes the game's whole UI skin (buttons, panels, input fields, card-slot placeholders) as
// procedurally-drawn rounded-rectangle PNGs, 9-sliced for uGUI Image / world-space SpriteRenderer
// alike. This is the "code-only" art path: nobody hand-draws these, this script does, once, ahead
// of time - so runtime code just references plain baked Sprite assets with zero per-frame cost.
// Rerun via Tools > UI > Generate Theme Sprites whenever UITheme's palette/shape values change.
public static class UISpriteGenerator
{
    private const string OutputFolder = "Assets/Generated/UI";

    [MenuItem("Tools/UI/Generate Theme Sprites")]
    public static void Generate()
    {
        UITheme theme = FindOrWarnTheme();
        if (theme == null)
        {
            return;
        }

        if (!AssetDatabase.IsValidFolder("Assets/Generated"))
        {
            AssetDatabase.CreateFolder("Assets", "Generated");
        }
        if (!AssetDatabase.IsValidFolder(OutputFolder))
        {
            AssetDatabase.CreateFolder("Assets/Generated", "UI");
        }

        theme.buttonNormal = BakeRoundedRect(theme, "button_normal", theme.primary);
        theme.buttonHover = BakeRoundedRect(theme, "button_hover", theme.primaryHover);
        theme.buttonPressed = BakeRoundedRect(theme, "button_pressed", theme.primaryPressed);
        theme.buttonDanger = BakeRoundedRect(theme, "button_danger", theme.danger);
        theme.panel = BakeRoundedRect(theme, "panel", theme.surface);
        theme.inputField = BakeRoundedRect(theme, "input_field", theme.inputFieldBackground, outlineColor: theme.primary, outlinePx: 2);
        theme.cardSlot = BakeRoundedRect(theme, "card_slot", theme.surface, outlineColor: theme.primary, outlinePx: 3);

        EditorUtility.SetDirty(theme);
        AssetDatabase.SaveAssets();
        Debug.Log("UISpriteGenerator: baked 7 sprites into " + OutputFolder + " and assigned them on " + theme.name + ".");
    }

    private static UITheme FindOrWarnTheme()
    {
        string[] guids = AssetDatabase.FindAssets("t:UITheme");
        if (guids.Length == 0)
        {
            Debug.LogWarning("UISpriteGenerator: no UITheme asset found. Create one via Assets > Create > PataponCards > UI Theme first, then rerun this.");
            return null;
        }
        return AssetDatabase.LoadAssetAtPath<UITheme>(AssetDatabase.GUIDToAssetPath(guids[0]));
    }

    private static Sprite BakeRoundedRect(UITheme theme, string name, Color fill, Color? outlineColor = null, int outlinePx = 0)
    {
        int size = theme.spriteSizePx;
        int radius = theme.cornerRadiusPx;
        int border = theme.spriteBorderPx;

        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
        };

        Color clear = new Color(fill.r, fill.g, fill.b, 0f);
        Color[] pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float coverage = RoundedRectCoverage(x, y, size, radius);
                Color pixel = clear;
                if (coverage > 0f)
                {
                    pixel = fill;
                    pixel.a *= coverage;
                    if (outlineColor.HasValue && outlinePx > 0)
                    {
                        // Blend toward the outline color in the ring between the outer edge and
                        // an edge inset by outlinePx - fully outline color mid-ring, fading to
                        // fill color as we move further inward.
                        float innerCoverage = RoundedRectCoverage(x, y, size, radius, inset: outlinePx);
                        float ringWeight = coverage * (1f - innerCoverage);
                        pixel = Color.Lerp(pixel, outlineColor.Value, ringWeight);
                        pixel.a = Mathf.Max(pixel.a, outlineColor.Value.a * ringWeight);
                    }
                }
                pixels[y * size + x] = pixel;
            }
        }
        tex.SetPixels(pixels);
        tex.Apply();

        string pngPath = OutputFolder + "/" + name + ".png";
        File.WriteAllBytes(pngPath, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(pngPath, ImportAssetOptions.ForceUpdate);

        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(pngPath);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spriteBorder = new Vector4(border, border, border, border);
        importer.spritePixelsPerUnit = 100f;
        importer.filterMode = FilterMode.Bilinear;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        // Required for 9-slicing to render correctly at all - Unity's default "Tight" mesh trims
        // to the sprite's visible/alpha shape (fine for a plain sprite), which produces broken/
        // invisible geometry once something 9-slices it (WorldButtonSkin sets SpriteRenderer's
        // drawMode to Sliced for the world-space buttons). uGUI Image never hit this because it
        // generates its own quad geometry regardless of the sprite's mesh type.
        // Mesh type isn't a direct TextureImporter property - it lives in the settings struct.
        TextureImporterSettings settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect;
        importer.SetTextureSettings(settings);
        EditorUtility.SetDirty(importer);
        importer.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Sprite>(pngPath);
    }

    // Anti-aliased coverage (0..1) of a centered rounded rect at pixel (x,y) in a size x size
    // texture with the given corner radius, optionally inset by `inset` pixels on every side
    // (used to carve out the ring for an outline).
    private static float RoundedRectCoverage(int x, int y, int size, int radius, int inset = 0)
    {
        float half = size / 2f;
        float px = x + 0.5f - half;
        float py = y + 0.5f - half;
        float extent = half - inset;
        float r = Mathf.Max(0f, radius - inset);

        float dx = Mathf.Max(Mathf.Abs(px) - (extent - r), 0f);
        float dy = Mathf.Max(Mathf.Abs(py) - (extent - r), 0f);
        float dist = Mathf.Sqrt(dx * dx + dy * dy) - r;

        // 1px-wide smoothstep band around the edge for anti-aliasing.
        return 1f - Mathf.Clamp01(dist + 0.5f);
    }
}
