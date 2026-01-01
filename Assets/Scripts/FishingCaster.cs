using UnityEngine;
using UnityEngine.EventSystems;

public class FishingCaster : MonoBehaviour
{
    public Camera mainCamera;
    public LayerMask clickLayerMask;
    public float rayLength = 100f;

    private void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return; 
            }
            PerformRaycast();
        }
    }

    private void PerformRaycast()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;
        
        if (Physics.Raycast(ray, out hitInfo, rayLength, clickLayerMask))
        {
         
            Debug.DrawLine(ray.origin, hitInfo.point, Color.green, 1f);
            
            FishingSpot spot = hitInfo.collider.GetComponent<FishingSpot>();

            if (spot != null)
            {
               
                spot.Interact(this.transform);
            }
        }
    }
}