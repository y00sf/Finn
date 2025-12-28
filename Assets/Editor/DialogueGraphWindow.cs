using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class DialogueGraphWindow : EditorWindow
{
    private int nodeWidth = 250;
    private int nodeHeight = 100;
    private int answerWidth = 180;
    private int answerHeight = 50;
    private int horizontalSpacing = 60;
    private int verticalSpacing = 80;
    private Vector2 scrollPosition = Vector2.zero;

    private float zoom = 1f;
    private const float zoomMin = 0.5f;
    private const float zoomMax = 2f;

    private Question[] questionArray;
    private Dictionary<Question, Rect> questionRects = new Dictionary<Question, Rect>();
    private Dictionary<string, Rect> answerRects = new Dictionary<string, Rect>();
    private Dictionary<string, Choice> answerChoices = new Dictionary<string, Choice>();
    private Dictionary<string, Question> answerParents = new Dictionary<string, Question>();

    private GUIStyle _textStyle;
    private GUIStyle TextStyle
    {
        get
        {
            if (_textStyle == null)
            {
                _textStyle = new GUIStyle();
                _textStyle.fontSize = 12;
                _textStyle.fontStyle = FontStyle.Normal;
                _textStyle.normal.textColor = Color.white;
                _textStyle.wordWrap = true;
                _textStyle.alignment = TextAnchor.UpperLeft;
            }
            return _textStyle;
        }
    }

    private GUIStyle _speakerStyle;
    private GUIStyle SpeakerStyle
    {
        get
        {
            if (_speakerStyle == null)
            {
                _speakerStyle = new GUIStyle();
                _speakerStyle.fontSize = 14;
                _speakerStyle.fontStyle = FontStyle.Bold;
                _speakerStyle.normal.textColor = Color.white;
                _speakerStyle.alignment = TextAnchor.UpperLeft;
            }
            return _speakerStyle;
        }
    }

    [MenuItem("Window/Dialogue Graph")]
    public static void ShowWindow()
    {
        DialogueGraphWindow window = (DialogueGraphWindow)EditorWindow.GetWindow(typeof(DialogueGraphWindow));
        window.titleContent = new GUIContent("Dialogue Graph");
        window.minSize = new Vector2(800, 600);
    }

    private void OnEnable()
    {
        LoadQuestions();
    }

    private void LoadQuestions()
    {
        string[] guids = AssetDatabase.FindAssets("t:Question");
        if (guids == null || guids.Length == 0)
        {
            questionArray = Array.Empty<Question>();
            Debug.Log("DialogueGraphWindow: no Question assets found.");
            return;
        }

        questionArray = new Question[guids.Length];
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            if (string.IsNullOrEmpty(path)) continue;

            Question q = AssetDatabase.LoadAssetAtPath<Question>(path);
            if (q == null)
            {
                Debug.LogWarning($"DialogueGraphWindow: failed to load Question at path {path}.");
            }
            questionArray[i] = q;
        }
    }

    private void OnGUI()
    {
        Rect windowRect = new Rect(0f, 0f, position.width, position.height);
        EditorGUI.DrawRect(windowRect, new Color(0.15f, 0.15f, 0.15f, 0.9f));

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        DrawGrid();
        DrawToolStrip();

        if (questionArray == null || questionArray.Length == 0)
        {
            EditorGUILayout.LabelField("No questions to display.");
            EditorGUILayout.EndScrollView();
            return;
        }

        GenerateQuestionAnswerLayout();
        DrawConnections();
        DrawNodes();
        HandleNodeSelection();

        EditorGUILayout.EndScrollView();
    }

    private class NodeLayout
    {
        public Question Question;
        public float SubtreeWidth;
        public Vector2 Position;
        public List<AnswerLayout> Answers = new List<AnswerLayout>();
    }

    private class AnswerLayout
    {
        public Choice Choice;
        public int Index;
        public float SubtreeWidth;
        public Vector2 Position;
        public NodeLayout NextQuestion;
    }

    private void GenerateQuestionAnswerLayout()
    {
        questionRects.Clear();
        answerRects.Clear();
        answerChoices.Clear();
        answerParents.Clear();

        HashSet<Question> childrenSet = new HashSet<Question>();
        foreach (var q in questionArray)
        {
            if (q.choices == null) continue;
            foreach (var choice in q.choices)
            {
                if (choice.nextDialogue != null)
                    childrenSet.Add(choice.nextDialogue);
            }
        }

        Question root = null;
        foreach (var q in questionArray)
        {
            if (!childrenSet.Contains(q))
            {
                root = q;
                break;
            }
        }

        if (root == null && questionArray.Length > 0)
        {
            root = questionArray[0];
        }

        if (root != null)
        {
            NodeLayout rootLayout = BuildNodeLayout(root, new HashSet<Question>());
            CalculateSubtreeWidths(rootLayout);
            AssignPositions(rootLayout, 0f, 0f);
        }
    }

    private NodeLayout BuildNodeLayout(Question q, HashSet<Question> visited)
    {
        if (visited.Contains(q)) return null;
        visited.Add(q);

        NodeLayout layout = new NodeLayout { Question = q };

        if (q.choices != null)
        {
            for (int i = 0; i < q.choices.Count; i++)
            {
                AnswerLayout answerLayout = new AnswerLayout
                {
                    Choice = q.choices[i],
                    Index = i
                };

                if (q.choices[i].nextDialogue != null)
                {
                    answerLayout.NextQuestion = BuildNodeLayout(q.choices[i].nextDialogue, visited);
                }

                layout.Answers.Add(answerLayout);
            }
        }

        return layout;
    }

    private float CalculateSubtreeWidths(NodeLayout node)
    {
        if (node == null) return 0f;

        if (node.Answers.Count == 0)
        {
            node.SubtreeWidth = nodeWidth;
            return node.SubtreeWidth;
        }

        float totalWidth = 0f;
        foreach (var answer in node.Answers)
        {
            if (answer.NextQuestion != null)
            {
                answer.SubtreeWidth = CalculateSubtreeWidths(answer.NextQuestion);
            }
            else
            {
                answer.SubtreeWidth = answerWidth;
            }
            totalWidth += answer.SubtreeWidth;
        }

        totalWidth += horizontalSpacing * (node.Answers.Count - 1);
        node.SubtreeWidth = Mathf.Max(totalWidth, nodeWidth);

        return node.SubtreeWidth;
    }

    private void AssignPositions(NodeLayout node, float startX, float startY)
    {
        if (node == null) return;

        float centerX = startX + node.SubtreeWidth / 2f - nodeWidth / 2f;
        node.Position = new Vector2(centerX, startY);
        questionRects[node.Question] = new Rect(node.Position, new Vector2(nodeWidth, nodeHeight));

        if (node.Answers.Count > 0)
        {
            float answersY = startY + nodeHeight + verticalSpacing;
            float currentX = startX;

            foreach (var answer in node.Answers)
            {
                float answerCenterX = currentX + answer.SubtreeWidth / 2f - answerWidth / 2f;
                answer.Position = new Vector2(answerCenterX, answersY);

                string answerKey = AssetDatabase.GetAssetPath(node.Question) + "_" + answer.Index;
                answerRects[answerKey] = new Rect(answer.Position, new Vector2(answerWidth, answerHeight));
                answerChoices[answerKey] = answer.Choice;
                answerParents[answerKey] = node.Question;

                if (answer.NextQuestion != null)
                {
                    float nextY = answersY + answerHeight + verticalSpacing;
                    AssignPositions(answer.NextQuestion, currentX, nextY);
                }

                currentX += answer.SubtreeWidth + horizontalSpacing;
            }
        }
    }

    private void DrawConnections()
    {
        Handles.BeginGUI();

        foreach (var kvp in questionRects)
        {
            Question q = kvp.Key;
            Rect qRect = kvp.Value;
            string qPath = AssetDatabase.GetAssetPath(q);

            if (q.choices != null)
            {
                for (int i = 0; i < q.choices.Count; i++)
                {
                    string answerKey = qPath + "_" + i;
                    if (!answerRects.ContainsKey(answerKey)) continue;

                    Rect aRect = answerRects[answerKey];
                    Vector2 start = new Vector2(qRect.x + qRect.width / 2, qRect.y + qRect.height);
                    Vector2 end = new Vector2(aRect.x + aRect.width / 2, aRect.y);

                    Handles.color = Color.green;
                    Handles.DrawBezier(start, end, start + Vector2.up * 30, end + Vector2.down * 30, Color.green, null, 2f);

                    if (q.choices[i].nextDialogue != null && questionRects.ContainsKey(q.choices[i].nextDialogue))
                    {
                        Rect nextRect = questionRects[q.choices[i].nextDialogue];
                        Vector2 aStart = new Vector2(aRect.x + aRect.width / 2, aRect.y + aRect.height);
                        Vector2 aEnd = new Vector2(nextRect.x + nextRect.width / 2, nextRect.y);

                        Handles.color = Color.blue;
                        Handles.DrawBezier(aStart, aEnd, aStart + Vector2.up * 30, aEnd + Vector2.down * 30, Color.blue, null, 2f);
                    }
                }
            }
        }

        Handles.EndGUI();
    }

    private void DrawNodes()
    {
        foreach (var kvp in questionRects)
        {
            DrawQuestionNode(kvp.Key, kvp.Value);
        }

        foreach (var kvp in answerRects)
        {
            string key = kvp.Key;
            Rect rect = kvp.Value;
            Choice choice = answerChoices[key];
            Question parent = answerParents[key];
            DrawAnswerNode(choice, rect, parent);
        }
    }

    private void DrawQuestionNode(Question q, Rect rect)
    {
        Rect outline = new Rect(rect.x, rect.y, rect.width, 7f);

        if (q == Selection.activeObject)
        {
            Rect highlighter = new Rect(rect.x - 2, rect.y - 2, rect.width + 4, rect.height + 4);
            EditorGUI.DrawRect(highlighter, Color.white);
        }

        EditorGUI.DrawRect(rect, new Color(0.25f, 0.25f, 0.25f, 1f));

        bool missingSpeaker = string.IsNullOrEmpty(q.SpeakerName);
        bool missingText = string.IsNullOrEmpty(q.questionText);
        bool missingChoices = (q.choices == null || q.choices.Count == 0);

        if (missingSpeaker && missingText && missingChoices)
        {
            EditorGUI.DrawRect(outline, Color.red);
        }
        else if (missingSpeaker || missingText || missingChoices)
        {
            EditorGUI.DrawRect(outline, new Color(1f, 0.64f, 0f));
        }
        else
        {
            EditorGUI.DrawRect(outline, Color.green);
        }

        // Draw speaker name
        Rect speakerRect = new Rect(rect.x + 10, rect.y + 12, rect.width - 20, 20);
        string speakerText = !string.IsNullOrEmpty(q.SpeakerName) ? q.SpeakerName : "<No Speaker>";
        EditorGUI.LabelField(speakerRect, speakerText, SpeakerStyle);

        // Draw dialogue text
        Rect dialogueRect = new Rect(rect.x + 10, rect.y + 32, rect.width - 20, rect.height - 42);
        string dialogueText = !string.IsNullOrEmpty(q.questionText) ? q.questionText : "<No Dialogue>";
        EditorGUI.LabelField(dialogueRect, dialogueText, TextStyle);
    }

    private void DrawAnswerNode(Choice choice, Rect rect, Question parent)
    {
        Rect outline = new Rect(rect.x, rect.y, rect.width, 7f);

        bool isSelected = false;
        foreach (var kvp in answerRects)
        {
            if (kvp.Value == rect && answerParents[kvp.Key] == Selection.activeObject)
            {
                isSelected = true;
                break;
            }
        }

        if (isSelected)
        {
            Rect highlighter = new Rect(rect.x - 2, rect.y - 2, rect.width + 4, rect.height + 4);
            EditorGUI.DrawRect(highlighter, Color.white);
        }

        EditorGUI.DrawRect(rect, new Color(0.25f, 0.25f, 0.25f, 1f));

        bool missingText = string.IsNullOrEmpty(choice.text);
        bool missingNext = choice.nextDialogue == null;

        if (missingText && missingNext)
        {
            EditorGUI.DrawRect(outline, Color.red);
        }
        else if (missingText || missingNext)
        {
            EditorGUI.DrawRect(outline, new Color(1f, 0.64f, 0f));
        }
        else
        {
            EditorGUI.DrawRect(outline, Color.green);
        }

        var centerText = new Rect(rect.x + 10, rect.y + 10, rect.width - 20, rect.height - 20);
        EditorGUI.LabelField(centerText, choice.text, TextStyle);
    }

    private void HandleNodeSelection()
    {
        Event e = Event.current;
        if (e.type == EventType.MouseDown)
        {
            Vector2 mousePos = e.mousePosition;

            foreach (var kvp in questionRects)
            {
                if (kvp.Value.Contains(mousePos))
                {
                    Selection.activeObject = kvp.Key;
                    e.Use();
                    return;
                }
            }

            foreach (var kvp in answerRects)
            {
                if (kvp.Value.Contains(mousePos))
                {
                    Selection.activeObject = answerParents[kvp.Key];
                    e.Use();
                    return;
                }
            }

            Selection.activeObject = null;
        }
    }

    private void DrawGrid(float gridSpacing = 20f, float gridOpacity = 0.1f)
    {
        Color originalColor = Handles.color;
        Handles.color = new Color(1f, 1f, 1f, gridOpacity);

        float width = position.width;
        float height = position.height;

        for (float x = 0; x < width; x += gridSpacing)
            Handles.DrawLine(new Vector3(x, 0, 0), new Vector3(x, height, 0));

        for (float y = 0; y < height; y += gridSpacing)
            Handles.DrawLine(new Vector3(0, y, 0), new Vector3(width, y, 0));

        Handles.color = originalColor;
    }

    void DrawToolStrip()
    {
        GUILayout.BeginHorizontal(EditorStyles.toolbar);

        GUILayout.Label("Zoom", GUILayout.Width(40));
        zoom = GUILayout.HorizontalSlider(zoom, zoomMin, zoomMax, GUILayout.Width(150));
        GUILayout.Label($"{zoom:0.00}x", GUILayout.Width(40));

        GUILayout.EndHorizontal();
    }

    private void OnValidate()
    {
        Repaint();
    }
    void SearchByName(string text)
    {
        for (int i = 0; i < questionArray.Length; i++)
        {
            if (questionArray[i].SpeakerName == text)
            {
                Debug.Log(questionArray[i].questionText);
                Selection.activeObject = questionArray[i];
            }
        }
    }
}