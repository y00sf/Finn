using System;
using ModularMotion;
using UnityEngine;

public class Sign : MonoBehaviour
{
    [SerializeField] private GameObject Signbox;
    [SerializeField] private UIMotion uiMotion1;
    [SerializeField] private UIMotion uiMotion2;
    [SerializeField] private Camera mainCam;
    [SerializeField] private Transform worldAnchor;
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 2f, 0f);
    [SerializeField] private Vector2 screenOffset = new Vector2(0f, 40f);

    private RectTransform signboxRect;
    private Canvas signboxCanvas;
    private Renderer targetRenderer;

    void Awake()
    {
        CacheReferences();

        if (Signbox != null)
        {
            Signbox.SetActive(false);
        }
    }

    private void LateUpdate()
    {
        if (!CacheReferences() || !Signbox.activeSelf)
        {
            return;
        }

        Vector3 worldTarget = GetWorldTarget();
        Vector3 screenTarget = mainCam.WorldToScreenPoint(worldTarget);
        screenTarget.x += screenOffset.x;
        screenTarget.y += screenOffset.y;

        if (screenTarget.z < 0f)
        {
            return;
        }

        RectTransform canvasRect = signboxCanvas.rootCanvas.transform as RectTransform;
        Camera uiCamera = signboxCanvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : signboxCanvas.worldCamera ?? mainCam;

        if (canvasRect != null &&
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenTarget, uiCamera, out Vector2 localPoint))
        {
            signboxRect.position = canvasRect.TransformPoint(localPoint);
        }
    }

    private bool CacheReferences()
    {
        if (Signbox == null)
        {
            Canvas canvas = GetComponentInChildren<Canvas>(true);
            if (canvas != null)
            {
                Signbox = canvas.gameObject;
            }
        }

        if (Signbox != null && signboxRect == null)
        {
            signboxRect = Signbox.GetComponent<RectTransform>();
        }

        if (Signbox != null && signboxCanvas == null)
        {
            signboxCanvas = Signbox.GetComponentInParent<Canvas>();
        }

        if (targetRenderer == null)
        {
            targetRenderer = GetComponentInChildren<Renderer>();
        }

        if (mainCam == null)
        {
            mainCam = Camera.main;
        }

        return Signbox != null && signboxRect != null && signboxCanvas != null && mainCam != null;
    }

    private Vector3 GetWorldTarget()
    {
        if (worldAnchor != null)
        {
            return worldAnchor.position + worldOffset;
        }

        if (targetRenderer != null)
        {
            Bounds bounds = targetRenderer.bounds;
            Vector3 topCenter = new Vector3(bounds.center.x, bounds.max.y, bounds.center.z);
            return topCenter + worldOffset;
        }

        return transform.position + worldOffset;
    }

    public void ShowSign()
    {
        if (!CacheReferences())
        {
            return;
        }

        Signbox.SetActive(true);
        uiMotion1?.PlayAll();
        uiMotion2?.PlayAll();
    }

    public void HideSign()
    {
        uiMotion1?.ResetMotion();
        uiMotion2?.ResetMotion();

        if (Signbox == null)
        {
            return;
        }

        
        Signbox.SetActive(false);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            HideSign();
        }
    }
}
