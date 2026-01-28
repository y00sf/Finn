using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class Eyes : MonoBehaviour
{
    [SerializeField] private float blinkDuration = 0.15f;
    [SerializeField] private float maxwaitBetweenBlinks = 2f;
    [SerializeField] private Vector3 defaultScale;

    [SerializeField] private Transform eyeL;
    [SerializeField] private Transform eyeR;

    private void Start()
    {
        defaultScale = eyeL.localScale;
        StartCoroutine(BlinkLoop());
    }

    IEnumerator BlinkLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(1f, maxwaitBetweenBlinks));
            yield return StartCoroutine(Blink());
            yield return StartCoroutine(Unblink());
        }
    }

    IEnumerator Blink()
    {
        float time = 0f;
        Vector3 start = eyeL.localScale;
        Vector3 end = new Vector3(defaultScale.x, 0f, defaultScale.z);

        while (time < blinkDuration)
        {
            time += Time.deltaTime;
            float t = time / blinkDuration;
            eyeL.localScale = Vector3.Lerp(start, end, t);
            eyeR.localScale = Vector3.Lerp(start, end, t);
            yield return null;
        }
    }

    IEnumerator Unblink()
    {
        float time = 0f;
        Vector3 start = eyeL.localScale;
        Vector3 end = defaultScale;

        while (time < blinkDuration)
        {
            time += Time.deltaTime;
            float t = time / blinkDuration;
            eyeL.localScale = Vector3.Lerp(start, end, t);
            eyeR.localScale = Vector3.Lerp(start, end, t);
            yield return null;
        }
    }
}