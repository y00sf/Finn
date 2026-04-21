using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class ImageMatchLayout : MonoBehaviour
{
    [SerializeField] private RectTransform layoutGroupRect;
    [SerializeField] private RectTransform imageRect;

    void Update()
    {
        if (layoutGroupRect == null || imageRect == null) return;

        imageRect.sizeDelta = new Vector2(
            layoutGroupRect.rect.width,
            imageRect.sizeDelta.y 
        );
    }
}