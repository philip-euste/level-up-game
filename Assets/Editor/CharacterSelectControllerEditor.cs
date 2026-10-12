#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CharacterSelectController))]
public sealed class CharacterSelectControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawDefaultInspector();
        EditorGUILayout.HelpBox(
            "Select a preview position to move its character in Scene view. " +
            "These previews are editor-only and are not added to the scene.",
            MessageType.Info);
        serializedObject.ApplyModifiedProperties();
    }
}

[InitializeOnLoad]
public static class CharacterSelectScenePreview
{
    private const float PreviewDepth = 0f;

    static CharacterSelectScenePreview()
    {
        SceneView.duringSceneGui -= DrawPreviews;
        SceneView.duringSceneGui += DrawPreviews;
    }

    private static void DrawPreviews(SceneView sceneView)
    {
        if (Application.isPlaying)
        {
            return;
        }

        CharacterSelectController[] controllers =
            Resources.FindObjectsOfTypeAll<CharacterSelectController>();
        foreach (CharacterSelectController controller in controllers)
        {
            if (controller == null ||
                EditorUtility.IsPersistent(controller) ||
                !controller.gameObject.scene.IsValid() ||
                controller.gameObject.hideFlags != HideFlags.None)
            {
                continue;
            }

            DrawControllerPreviews(controller);
        }
    }

    private static void DrawControllerPreviews(CharacterSelectController controller)
    {
        SerializedObject data = new SerializedObject(controller);
        data.Update();

        SerializedProperty roster = data.FindProperty("characterRoster");
        if (roster == null || roster.arraySize == 0)
        {
            Handles.Label(controller.transform.position, "Character roster is empty");
            return;
        }

        SerializedProperty firstEntry = roster.GetArrayElementAtIndex(0);
        SerializedProperty spriteProperty = firstEntry.FindPropertyRelative("previewSprite");
        Sprite sprite = spriteProperty != null
            ? spriteProperty.objectReferenceValue as Sprite
            : null;

        SerializedProperty layout = data.FindProperty("layout");
        SerializedProperty scaleProperty = layout != null
            ? layout.FindPropertyRelative("previewScale")
            : null;
        float scale = scaleProperty != null ? scaleProperty.floatValue : 0.65f;

        DrawPreview(controller, data, sprite, scale, "Player 1", "player1PreviewPosition", false);
        DrawPreview(controller, data, sprite, scale, "Player 2", "player2PreviewPosition", true);
    }

    private static void DrawPreview(
        CharacterSelectController controller,
        SerializedObject data,
        Sprite sprite,
        float scale,
        string label,
        string positionField,
        bool flipX)
    {
        SerializedProperty positionProperty = data.FindProperty(positionField);
        if (positionProperty == null)
        {
            return;
        }

        Vector3 position = positionProperty.vector3Value;
        Handles.Label(position + Vector3.up * 3.4f, label + " Preview");

        EditorGUI.BeginChangeCheck();
        Vector3 movedPosition = Handles.PositionHandle(position, Quaternion.identity);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(controller, "Move " + label + " Character Preview");
            positionProperty.vector3Value = movedPosition;
            data.ApplyModifiedProperties();
            EditorUtility.SetDirty(controller);
            position = movedPosition;
        }

        if (sprite == null || sprite.texture == null || Event.current.type != EventType.Repaint)
        {
            Handles.Label(position, "No preview sprite assigned");
            return;
        }

        Vector2 spriteSize = sprite.bounds.size * Mathf.Max(0f, scale);
        if (spriteSize.x <= 0f || spriteSize.y <= 0f)
        {
            return;
        }

        Vector3 bottomLeftWorld = position + new Vector3(
            -spriteSize.x * 0.5f, -spriteSize.y * 0.5f, PreviewDepth);
        Vector3 topRightWorld = position + new Vector3(
            spriteSize.x * 0.5f, spriteSize.y * 0.5f, PreviewDepth);
        Vector2 bottomLeftGUI = HandleUtility.WorldToGUIPoint(bottomLeftWorld);
        Vector2 topRightGUI = HandleUtility.WorldToGUIPoint(topRightWorld);

        Rect previewRect = Rect.MinMaxRect(
            Mathf.Min(bottomLeftGUI.x, topRightGUI.x),
            Mathf.Min(bottomLeftGUI.y, topRightGUI.y),
            Mathf.Max(bottomLeftGUI.x, topRightGUI.x),
            Mathf.Max(bottomLeftGUI.y, topRightGUI.y));

        Rect textureRect = sprite.textureRect;
        Rect textureUVs = new Rect(
            textureRect.x / sprite.texture.width,
            textureRect.y / sprite.texture.height,
            textureRect.width / sprite.texture.width,
            textureRect.height / sprite.texture.height);

        if (flipX)
        {
            textureUVs.x += textureUVs.width;
            textureUVs.width = -textureUVs.width;
        }

        Handles.BeginGUI();
        GUI.DrawTextureWithTexCoords(previewRect, sprite.texture, textureUVs, true);
        Handles.EndGUI();
    }
}
#endif
