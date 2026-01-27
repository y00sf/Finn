using UnityEngine;
using UnityEngine.UI;

public class Circle : MonoBehaviour
{
    [SerializeField] private RectTransform mainCircle;
    [SerializeField] private RectTransform approachRing;
    [SerializeField] private float startScale = 3f;
    [SerializeField] private float shrinkSpeed = 1f;
    [SerializeField] private float hitTolerance = 0.35f;

    [Header("Color")]
    [SerializeField] private Image approachRingImage;
    [SerializeField] private Color startColor = Color.red;
    [SerializeField] private Color endColor = Color.green;

    void Start()
    {
        approachRing.localScale = Vector3.one * startScale;

        if (approachRingImage == null && approachRing != null)
        {
            approachRingImage = approachRing.GetComponent<Image>();
        }

        if (approachRingImage != null)
        {
            approachRingImage.color = startColor;
        }
    }

    void Update()
    {
        approachRing.localScale -= Vector3.one * shrinkSpeed * Time.deltaTime;
        UpdateApproachRingColor();

        if (approachRing.localScale.x <= mainCircle.localScale.x * 0.7f)
        {
            Destroy(gameObject);
        }
    }

    private void UpdateApproachRingColor()
    {
        if (approachRingImage == null || mainCircle == null) return;

        float approachSize = approachRing.localScale.x;
        float targetSize = mainCircle.localScale.x;
        float totalRange = startScale - targetSize;
        float currentAboveTarget = Mathf.Max(approachSize - targetSize, 0f);

        float progress = totalRange > 0f ? 1f - (currentAboveTarget / totalRange) : 1f;
        progress = Mathf.Clamp01(progress);

        approachRingImage.color = Color.Lerp(startColor, endColor, progress);
        
        if (Mathf.Abs(approachSize - targetSize) <= hitTolerance)
        {
            approachRingImage.color = Color.white;
        }
    }

    public float GetHitDistance()
    {
        return Mathf.Abs(approachRing.localScale.x - mainCircle.localScale.x);
    }

    public bool CheckHit()
    {
        if (GetHitDistance() <= hitTolerance)
        {
            Destroy(gameObject);
            return true;
        }
        return false;
    }
}