using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(Choice))]
public class ChoicePropertyDrawer : PropertyDrawer
{
    private const float SPACING = 5f;
    private const float NEXT_DIALOGUE_WIDTH = 70f;
    private const float AUDIO_WIDTH = 50f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        float line = EditorGUIUtility.singleLineHeight;
        float textWidth = position.width - AUDIO_WIDTH - NEXT_DIALOGUE_WIDTH - (SPACING * 2);
       
        Rect choiceLabelRect = new Rect(
            position.x,
            position.y,
            textWidth,
            line
        );
        
        Rect audioLabelRect = new Rect(
            choiceLabelRect.xMax + SPACING,
            position.y,
            AUDIO_WIDTH,
            line
        );

        Rect nextLabelRect = new Rect(
            position.xMax - NEXT_DIALOGUE_WIDTH,
            position.y,
            NEXT_DIALOGUE_WIDTH,
            line
        );

        GUIStyle miniStyle = new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.UpperLeft,
            normal = { textColor = Color.white }
        };
        
        GUIStyle centerStyle = new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.UpperCenter
        };

        GUIStyle warningStyle = new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.UpperCenter,
            normal = { textColor = Color.red }
        };

        EditorGUI.LabelField(choiceLabelRect, "Choice Text", miniStyle);
        EditorGUI.LabelField(audioLabelRect, "Audio", centerStyle);

        SerializedProperty nextDialogueProp = property.FindPropertyRelative("nextDialogue");
        if (nextDialogueProp.objectReferenceValue == null)
        {
            EditorGUI.LabelField(nextLabelRect, "⚠ Missing", warningStyle);
        }
        else
        {
            EditorGUI.LabelField(nextLabelRect, "Next", miniStyle);
        }


       
        Rect secondLine = new Rect(
            position.x,
            position.y + line + 2f,
            position.width,
            line
        );

        Rect textRect = new Rect(
            secondLine.x,
            secondLine.y,
            textWidth,
            line
        );
        
        Rect audioClipRect = new Rect(
            textRect.xMax + SPACING,
            secondLine.y,
            AUDIO_WIDTH,
            line
        );

        Rect nextDialogueRect = new Rect(
            secondLine.xMax - NEXT_DIALOGUE_WIDTH,
            secondLine.y,
            NEXT_DIALOGUE_WIDTH,
            line
        );
        

        SerializedProperty textProp = property.FindPropertyRelative("text");
        SerializedProperty audioProp = property.FindPropertyRelative("choiceAudio");

        
        if (audioProp != null)
        {
            EditorGUI.PropertyField(audioClipRect, audioProp, GUIContent.none);
        }
        else
        {
            EditorGUI.LabelField(audioClipRect, "Error");
        }
        
        EditorGUI.PropertyField(textRect, textProp, GUIContent.none);
        EditorGUI.PropertyField(nextDialogueRect, nextDialogueProp, GUIContent.none);
        EditorGUI.PropertyField(audioClipRect, audioProp, GUIContent.none);

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float line = EditorGUIUtility.singleLineHeight;
        return (line * 2) + 5f; 
    }
}
