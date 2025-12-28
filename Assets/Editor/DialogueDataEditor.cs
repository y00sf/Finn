using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;


[CustomEditor(typeof(Question))]
public class DialogueDataEditor : Editor
{
    SerializedProperty Question;
    private bool showSpeaker = true;  
    private bool showDialogue = true;
    private bool showChoices = true;
    
    private ReorderableList choicesList;
    
    private GUIStyle _headerStyle;
    private GUIStyle HeaderStyle
    {
        get
        {
            if (_headerStyle == null)
            {
                _headerStyle = new GUIStyle();
                _headerStyle.fontSize = 20;
                _headerStyle.fontStyle = FontStyle.Bold;
                _headerStyle.normal.textColor = Color.white;
               
            }
            return _headerStyle;
        }
    }
    
    private GUIStyle _foldoutStyle;
    private GUIStyle FoldoutStyle
    {
        get
        {
            if (_foldoutStyle == null)
            {
                _foldoutStyle = new GUIStyle(EditorStyles.foldout);
                _foldoutStyle.fontStyle = FontStyle.Bold;
                _foldoutStyle.fontSize = 16;
                _foldoutStyle.normal.textColor = Color.white;
            }
            return _foldoutStyle;
        }
    }
   
    
    void OnEnable()
    {
         Question = serializedObject.FindProperty("Question");
         
         SerializedProperty choicesProp = serializedObject.FindProperty("choices");
         
         choicesList = new ReorderableList(serializedObject, choicesProp, true, true, true, true);
         
         choicesList.drawHeaderCallback = DrawChoicesHeader;
         choicesList.drawElementCallback = DrawChoiceElement;
         choicesList.elementHeightCallback = GetChoiceElementHeight;
         choicesList.onAddCallback = OnAddChoice;
         choicesList.onRemoveCallback = OnRemoveChoice;
         choicesList.drawElementBackgroundCallback = DrawElementBackground;
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        Question question = (Question)target;
        string assetPath = AssetDatabase.GetAssetPath(question);
        string fileName = Path.GetFileName(assetPath);
        
        
        int totalChars = CalculateTotalCharacters(question, fileName);
        
       
        DrawAssetHeader(fileName, totalChars);

       
        DrawSpeakerSection();
        
        DrawDialogueSection();
        
        DrawChoicesSection();
        
        serializedObject.ApplyModifiedProperties();
        
        
    }

    private void DrawDialogueSection()
    {
        Rect rect = EditorGUILayout.GetControlRect();
        EditorGUI.DrawRect(rect,new Color(0.18f, 0.18f, 0.18f, 1f));
        EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, 2), new Color(0.3f, 0.3f, 0.3f));
        EditorGUI.DrawRect(new Rect(rect.x, rect.y + rect.height - 2, rect.width, 2), new Color(0.3f, 0.3f, 0.3f));
        showDialogue = EditorGUI.Foldout(rect, showDialogue, "Dialogue", true, FoldoutStyle);
        
        EditorGUILayout.Space();
        SerializedProperty dialogueTextProp = serializedObject.FindProperty("questionText");
        if (dialogueTextProp.stringValue == string.Empty)
        {
            EditorGUILayout.Space(); 
            EditorGUILayout.HelpBox("questionText is empty!", MessageType.Error);
            EditorGUILayout.Space(); 
        }

        if (showDialogue)
        {
            EditorGUILayout.PropertyField(dialogueTextProp);
        }
        

        EditorGUILayout.Space();
    }

    private void DrawChoicesSection()
    {
        showChoices = EditorGUILayout.Foldout(showChoices, "Choices", FoldoutStyle);
        EditorGUILayout.Space();
        SerializedProperty choicesProp = serializedObject.FindProperty("choices");

        if (showChoices)
        {
            choicesList.DoLayoutList();
            
        }
        for (int i = 0; i < choicesProp.arraySize; i++)
        {
            SerializedProperty element = choicesProp.GetArrayElementAtIndex(i).FindPropertyRelative("text");
            if (string.IsNullOrEmpty(element.stringValue))
            {
                EditorGUILayout.Space(); 
                
                EditorGUILayout.HelpBox($"Answer {i} is empty!", MessageType.Warning);
                EditorGUILayout.Space(); 
            }
        }
    }

    private void DrawSpeakerSection()
    {
        showSpeaker = EditorGUILayout.Foldout(showSpeaker, "Speaker", FoldoutStyle);
        EditorGUILayout.Space();
        SerializedProperty speakerNameProp = serializedObject.FindProperty("SpeakerName");
        if (speakerNameProp.stringValue == string.Empty)
        {
            EditorGUILayout.Space(); 
            EditorGUILayout.HelpBox("Speaker name is empty!", MessageType.Error);
            EditorGUILayout.Space(); 
        }
        if (showSpeaker)
        {
            EditorGUILayout.PropertyField(speakerNameProp);
        }

        
      
        EditorGUILayout.Space(); 
    }

    private void DrawAssetHeader(string fileName, int totalChars)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
    
        EditorGUILayout.LabelField("DIALOGUE DATA", HeaderStyle);
        EditorGUILayout.LabelField($"File: {fileName}", EditorStyles.miniLabel);
        EditorGUILayout.LabelField($"Characters: {totalChars}", EditorStyles.miniLabel);
    
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();

    }

    private int CalculateTotalCharacters(Question question, string fileName)
    {
        // 1. Safety Check: If the scriptable object is missing, stop immediately.
        if (question == null) return 0;

        int totalChars = 0;

        // 2. Add File Name Length (Check if string exists first)
        if (!string.IsNullOrEmpty(fileName))
        {
            totalChars += fileName.Length;
        }

        // 3. Add Question Text Length
        // THIS IS THE FIX: We check IsNullOrEmpty BEFORE accessing .Length
        if (!string.IsNullOrEmpty(question.questionText))
        {
            totalChars += question.questionText.Length;
        }

        // 4. Add Choices Length
        if (question.choices != null)
        {
            foreach (Choice answer in question.choices)
            {
                // THIS IS THE COMPILER FIX: 
                // We do NOT check "if (answer != null)" because Choice is a struct.
                // We only check if the text string inside it is valid.
                if (!string.IsNullOrEmpty(answer.text))
                {
                    totalChars += answer.text.Length;
                }
            }
        }
    
        return totalChars;
    }

    private void DrawHorizontalLine()
    {
        
        Rect rect = EditorGUILayout.GetControlRect();
    
        EditorGUI.DrawRect(rect, Color.white);
        EditorGUI.DrawRect(rect, Color.gray);
    }
    
    private void DrawChoicesHeader(Rect rect)
    {
       
        int count = choicesList.count;
        EditorGUI.LabelField(rect, $"Choices ({count})", EditorStyles.boldLabel);
    }
    
    private void DrawChoiceElement(Rect rect, int index, bool isActive, bool isFocused)
    {
        
        
        if (isActive)
        {
            EditorGUI.DrawRect(rect, new Color(0.3f, 0.5f, 0.8f, 0.3f));  // Light blue tint
        }
    
        
        SerializedProperty element = choicesList.serializedProperty.GetArrayElementAtIndex(index);
       
        EditorGUI.PropertyField(rect, element, GUIContent.none);
    }
    
    private void OnAddChoice(ReorderableList list)
    {
        
        int newIndex = list.serializedProperty.arraySize;
        list.serializedProperty.arraySize++;
    
      
        SerializedProperty newElement = list.serializedProperty.GetArrayElementAtIndex(newIndex);
    
        
        SerializedProperty textProp = newElement.FindPropertyRelative("text");
        SerializedProperty nextProp = newElement.FindPropertyRelative("nextDialogue");
    
        textProp.stringValue = "New Choice";
        nextProp.objectReferenceValue = null;
    
       
        list.serializedProperty.serializedObject.ApplyModifiedProperties();
    }
    
    private void OnRemoveChoice(ReorderableList list)
    {
        SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(list.index);
        SerializedProperty textProp = element.FindPropertyRelative("text");
    
        string choiceText = textProp.stringValue;
    
        if (EditorUtility.DisplayDialog(
                "Delete Choice",
                $"Delete choice \"{choiceText}\"?",
                "Delete",
                "Cancel"))
        {
            ReorderableList.defaultBehaviours.DoRemoveButton(list);
        }
    }
    
    private void DrawElementBackground(Rect rect, int index, bool isActive, bool isFocused)
    {
      
        var arrayProp = choicesList.serializedProperty;
        if (arrayProp == null || index < 0 || index >= arrayProp.arraySize)
            return;

      
        Rect bg = new Rect(rect.x + 2, rect.y + 2, rect.width - 4, rect.height - 4);

       
        SerializedProperty element = arrayProp.GetArrayElementAtIndex(index);
        SerializedProperty textProp = element?.FindPropertyRelative("text");
        SerializedProperty nextProp = element?.FindPropertyRelative("nextDialogue");

        bool hasError = false;
        if (textProp != null && string.IsNullOrEmpty(textProp.stringValue) || nextProp.objectReferenceValue == null)
        {
            hasError = true;
        }
           
        

        if (hasError)
        {
            int steps = 12;
            float sliceW = bg.width / steps;
            for (int i = 0; i < steps; i++)
            {
                float t = (float)i / (steps - 1);
                Color c = Color.Lerp(Color.red, Color.black, t) * new Color(1f, 1f, 1f, 0.22f); // keep alpha low
                Rect slice = new Rect(bg.x + i * sliceW, bg.y, sliceW + 0.5f, bg.height);
                EditorGUI.DrawRect(slice, c);
            }
            return;
        }
        if (isFocused)
        {
            EditorGUI.DrawRect(rect, new Color(0.2f, 0.4f, 0.7f, 0.2f));
        }
        else if (index % 2 == 0)
        {
            EditorGUI.DrawRect(rect, new Color(0, 0, 0, 0.1f));  // Zebra striping
        }
    }
    
    private float GetChoiceElementHeight(int index)
    {
        // Your choice drawer is 2 lines tall + spacing
        float line = EditorGUIUtility.singleLineHeight;
        float spacing = 2f;
    
        return (line * 2) + (spacing * 2);
    
        // Plus extra spacing between elements
        return (line * 2) + (spacing * 3);  // Add gap between choices
    }
}
