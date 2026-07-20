using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

// Auto-reskins every currently-open scene's uGUI with the sprites baked by UISpriteGenerator,
// so restyling ~10 menu scenes doesn't mean hand-dragging sprites onto dozens of buttons (and
// doesn't mean me hand-editing scene YAML, which has corrupted scenes before on this project).
// Open a scene in the Editor, run Tools > UI > Apply Theme To Open Scenes, review, save.
public static class UIThemeApplier
{
    [MenuItem("Tools/UI/Apply Theme To Open Scenes")]
    public static void ApplyToOpenScenes()
    {
        UITheme theme = FindTheme();
        if (theme == null || !HasGeneratedSprites(theme))
        {
            return;
        }

        int buttons = 0, panels = 0, inputFields = 0;
        for (int i = 0; i < EditorSceneManager.sceneCount; i++)
        {
            Scene scene = EditorSceneManager.GetSceneAt(i);
            if (!scene.isLoaded)
            {
                continue;
            }
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                buttons += ApplyButtons(root, theme);
                panels += ApplyPlainImages(root, theme);
                inputFields += ApplyInputFields(root, theme);
            }
        }
        Debug.Log("UIThemeApplier: restyled " + buttons + " buttons, " + panels + " panels/images, " + inputFields + " input fields across open scenes.");
    }

    // Scenes-only ApplyToOpenScenes can't reach a prefab asset sitting in the Project window
    // (e.g. FriendRow) - select one or more prefabs there first, then run this instead.
    [MenuItem("Tools/UI/Apply Theme To Selected Prefabs")]
    public static void ApplyToSelectedPrefabs()
    {
        UITheme theme = FindTheme();
        if (theme == null || !HasGeneratedSprites(theme))
        {
            return;
        }

        UnityEngine.Object[] selection = Selection.GetFiltered(typeof(GameObject), SelectionMode.Assets);
        if (selection.Length == 0)
        {
            Debug.LogWarning("UIThemeApplier: select one or more prefab assets in the Project window first.");
            return;
        }

        int buttons = 0, panels = 0, inputFields = 0, prefabCount = 0;
        foreach (UnityEngine.Object obj in selection)
        {
            string path = AssetDatabase.GetAssetPath(obj);
            if (string.IsNullOrEmpty(path) || PrefabUtility.GetPrefabAssetType(obj) == PrefabAssetType.NotAPrefab)
            {
                continue;
            }

            GameObject contentsRoot = PrefabUtility.LoadPrefabContents(path);
            buttons += ApplyButtons(contentsRoot, theme);
            panels += ApplyPlainImages(contentsRoot, theme);
            inputFields += ApplyInputFields(contentsRoot, theme);
            PrefabUtility.SaveAsPrefabAsset(contentsRoot, path);
            PrefabUtility.UnloadPrefabContents(contentsRoot);
            prefabCount++;
        }

        Debug.Log("UIThemeApplier: restyled " + buttons + " buttons, " + panels + " panels/images, " + inputFields + " input fields across " + prefabCount + " selected prefab(s).");
    }

    private static UITheme FindTheme()
    {
        string[] guids = AssetDatabase.FindAssets("t:UITheme");
        if (guids.Length == 0)
        {
            Debug.LogWarning("UIThemeApplier: no UITheme asset found. Create one via Assets > Create > PataponCards > UI Theme first.");
            return null;
        }
        return AssetDatabase.LoadAssetAtPath<UITheme>(AssetDatabase.GUIDToAssetPath(guids[0]));
    }

    private static bool HasGeneratedSprites(UITheme theme)
    {
        if (theme.buttonNormal == null || theme.panel == null || theme.inputField == null)
        {
            Debug.LogWarning("UIThemeApplier: UITheme has no generated sprites yet. Run Tools > UI > Generate Theme Sprites first.");
            return false;
        }
        return true;
    }

    private static int ApplyButtons(GameObject root, UITheme theme)
    {
        int count = 0;
        foreach (Button button in root.GetComponentsInChildren<Button>(true))
        {
            Image image = button.targetGraphic as Image ?? button.GetComponent<Image>();
            if (image == null)
            {
                continue;
            }

            image.sprite = theme.buttonNormal;
            image.type = Image.Type.Sliced;
            image.color = Color.white;

            button.transition = Selectable.Transition.SpriteSwap;
            button.spriteState = new SpriteState
            {
                highlightedSprite = theme.buttonHover,
                pressedSprite = theme.buttonPressed,
                selectedSprite = theme.buttonHover,
                disabledSprite = theme.buttonNormal,
            };

            EditorUtility.SetDirty(button);
            EditorUtility.SetDirty(image);
            count++;
        }
        return count;
    }

    private static int ApplyPlainImages(GameObject root, UITheme theme)
    {
        int count = 0;
        foreach (Image image in root.GetComponentsInChildren<Image>(true))
        {
            if (image.GetComponent<Button>() != null || image.GetComponent<TMP_InputField>() != null)
            {
                continue; // handled by ApplyButtons/ApplyInputFields
            }
            // Only reskin plain backdrop-style images (default UI sprite or no sprite at all) -
            // leave custom art (icons, card art, logos) untouched so this can't silently clobber
            // hand-placed visuals it wasn't meant to touch.
            if (image.sprite != null && image.sprite.name != "UISprite" && image.sprite.name != "Background")
            {
                continue;
            }
            image.sprite = theme.panel;
            image.type = Image.Type.Sliced;
            image.color = Color.white;
            EditorUtility.SetDirty(image);
            count++;
        }
        return count;
    }

    private static int ApplyInputFields(GameObject root, UITheme theme)
    {
        int count = 0;
        foreach (TMP_InputField field in root.GetComponentsInChildren<TMP_InputField>(true))
        {
            Image image = field.GetComponent<Image>();
            if (image != null)
            {
                image.sprite = theme.inputField;
                image.type = Image.Type.Sliced;
                image.color = Color.white;
                EditorUtility.SetDirty(image);
            }

            // Input fields use a light background (theme.inputFieldBackground) unlike the rest of
            // the dark theme, so their text needs the dark-on-light pair, not textPrimary/Muted.
            // Also turn off vertex gradient, which (if enabled) overrides .color entirely with
            // its own gradient corner colors.
            if (field.textComponent != null)
            {
                field.textComponent.enableVertexGradient = false;
                field.textComponent.color = theme.textOnLight;
                EditorUtility.SetDirty(field.textComponent);
            }
            if (field.placeholder is TMP_Text placeholderText)
            {
                placeholderText.enableVertexGradient = false;
                placeholderText.color = theme.textOnLightMuted;
                EditorUtility.SetDirty(placeholderText);
            }

            count++;
        }
        return count;
    }
}
