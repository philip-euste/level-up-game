using UnityEngine;

public sealed class CharacterSelectionPreview : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    public static CharacterSelectionPreview Create(
        string objectName,
        Vector3 position,
        bool facesLeft,
        Material previewMaterial,
        float scale)
    {
        GameObject previewObject = new GameObject(objectName);
        previewObject.transform.position = position;
        previewObject.transform.localScale = Vector3.one * scale;

        SpriteRenderer renderer = previewObject.AddComponent<SpriteRenderer>();
        renderer.sharedMaterial = previewMaterial;

        Animator previewAnimator = previewObject.AddComponent<Animator>();
        CharacterSelectionPreview preview =
            previewObject.AddComponent<CharacterSelectionPreview>();
        preview.spriteRenderer = renderer;
        preview.animator = previewAnimator;
        preview.spriteRenderer.flipX = facesLeft;
        return preview;
    }

    public void Display(Sprite sprite, RuntimeAnimatorController controller)
    {
        if (spriteRenderer == null || animator == null)
        {
            Debug.LogError("Character preview is missing its renderer or animator.");
            return;
        }

        bool controllerChanged = animator.runtimeAnimatorController != controller;
        spriteRenderer.sprite = sprite;
        spriteRenderer.enabled = sprite != null || controller != null;

        if (controllerChanged)
        {
            animator.runtimeAnimatorController = controller;
        }

        animator.enabled = controller != null;
        if (controller != null && controllerChanged)
        {
            animator.Rebind();
            animator.Update(0f);
        }
    }
}
