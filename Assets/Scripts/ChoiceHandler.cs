using UnityEngine;

public class ChoiceHandler : MonoBehaviour
{
   public static event System.Action<int> OnChoiceMade;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

   public void OnChoice0Clicked()
    {
        HandleChoiceSelected(0);
    }
   public void OnChoice1Clicked()
    {
        HandleChoiceSelected(1);
    }
   public void OnChoice2Clicked()
    {
        HandleChoiceSelected(2);
    }

    private void HandleChoiceSelected(int index)
    {
     if (index < 0 || index > 2)
     {
       Debug.Log($"Invalid Choice #{index}");
       return;
       
      
     }
     Debug.Log($"[ChoiceHandler] Player selected choice #{index}");
     OnChoiceMade?.Invoke(index);
    }
}
