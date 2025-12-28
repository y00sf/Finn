using TMPro;
using UnityEngine;

public class FishingMiniGame : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private RectTransform pointer;
    [SerializeField] private RectTransform Target;
    [SerializeField] private TextMeshProUGUI counterText;
    [SerializeField] private TextMeshProUGUI healthText;
    [Header("Game Setttigns")]
    [SerializeField] private float speed = 200f;
    [SerializeField] private float speedIncrease = 50f;
    [SerializeField] private int counterCount;
    [SerializeField] private float hitTolerance = 15f;
    [SerializeField] private int health;
    
    
    
    [SerializeField] private bool isClockwise = true;
    
    void Start()
    {
        UpdateCounterUI();
        ChangeTargetRot();
    }

    // Update is called once per frame
    void Update()
    {
        

       
        float directionMultiplier = isClockwise ? -1f : 1f;
        
       
        pointer.Rotate(Vector3.forward * directionMultiplier * speed * Time.deltaTime);

       
        if (Input.GetMouseButtonDown(0)) 
        {
            CheckHit();
        }
    }

    void CheckHit()
    {
        float pointerAngle = pointer.eulerAngles.z;
        float targetAngle = Target.eulerAngles.z;

    
        float angleDifference = Mathf.DeltaAngle(pointerAngle, targetAngle);

        
        if (Mathf.Abs(angleDifference) <= hitTolerance)
        {
            OnSuccess();
        }
        else
        {
            OnFail();
        }
    }

    private void UpdateCounterUI()
    {
        counterText.text = counterCount.ToString();
    }

    private void ChangeTargetRot()
    {
        
        float randomAngle = Random.Range(0f, 360f);
        
       
        Target.rotation = Quaternion.Euler(0, 0, randomAngle);
    }

    private void OnFail()
    {
        health--;
        healthText.text = health.ToString();
        if (health <= 0)
        {
            Debug.Log("Game Over");
        }
    }

    private void OnSuccess()
    {
        counterCount--;
        UpdateCounterUI();

        if (counterCount <= 0)
        {
            Debug.Log("Win");
            return;
        }
        isClockwise = !isClockwise;
        
        speed += speedIncrease;
        
        ChangeTargetRot();
    }
}
