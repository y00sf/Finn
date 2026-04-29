using UnityEngine;

[System.Serializable]
public struct BubblePoint
{
    public RectTransform image;
    public Vector3 offset;
}
public class IntercationUIFollow : MonoBehaviour
{
    public Transform player;
    public float lerpSpeed = 10f;
    public BubblePoint[] bubbles;
    public Camera mainCam;

    void Start()
    {
      
    }

    void LateUpdate()
    {
        if (player == null || mainCam == null) return;

        foreach (var b in bubbles)
        {
            if (b.image != null)
            {
                Vector3 worldTarget = player.position + b.offset;
                Vector3 screenTarget = mainCam.WorldToScreenPoint(worldTarget);
                b.image.position = Vector3.Lerp(b.image.position, screenTarget, lerpSpeed * Time.deltaTime);
                
            }
        }
    }

   
}
