using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    public Button[] dialogueButtons;

    [Header("World Space Settings")]
    public float heightOffset = 2.0f; 
    public GameObject defaultSpeaker; 

    public void DisplayDialogue(Question question)
    {
        if (question == null) return;

       
        MoveCanvasToSpeaker(question.SpeakerName);
        
        
        if(speakerNameText != null) speakerNameText.text = question.SpeakerName;
        if(dialogueText != null) dialogueText.text = question.questionText;

      
        for (int i = 0; i < dialogueButtons.Length; i++)
        {
            if (i < question.choices.Count)
            {
                dialogueButtons[i].gameObject.SetActive(true);
                dialogueButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = question.choices[i].text;
            }
            else
            {
                dialogueButtons[i].gameObject.SetActive(false);
            }
        }
    }

    private void MoveCanvasToSpeaker(string nameToFind)
    {
        GameObject speakerObj = GameObject.Find(nameToFind);

        if (speakerObj != null)
        {
            
            transform.position = speakerObj.transform.position + Vector3.up * heightOffset;
        }
        else
        {
            if (defaultSpeaker != null)
            {
                transform.position = defaultSpeaker.transform.position + Vector3.up * heightOffset;
            }
            Debug.LogWarning($"Could not find a GameObject named '{nameToFind}'. Ensure Scene Object matches Dialogue Speaker Name.");
        }
    }
}
