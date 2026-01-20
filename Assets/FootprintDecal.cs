using UnityEngine;

public class FootprintDecal : MonoBehaviour
{
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private float fadeDuration = 2f;
    
    private Material mat;
    private Color startColor;
    private float timer;

    void Start()
    {
        Renderer rend = GetComponent<Renderer>();
        mat = rend.material; 
        startColor = mat.color;
        
        
        Destroy(gameObject, lifetime + fadeDuration);
    }

    void Update()
    {
        timer += Time.deltaTime;
        
        if (timer > lifetime)
        {
            float fadeProgress = (timer - lifetime) / fadeDuration;
            Color newColor = startColor;
            newColor.a = Mathf.Lerp(startColor.a, 0f, fadeProgress);
            mat.color = newColor;
        }
    }
}