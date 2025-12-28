using System.Collections.Generic;
using UnityEngine;


public static class DialogueTreeLayout
{
    public class NodeInfo
    {
        public Question Question;
        public float SubtreeWidth;
        public Vector2 Position;
        public List<NodeInfo> Children = new List<NodeInfo>();
    }

  
    public static Dictionary<Question, Rect> GenerateTreeLayout(
        Question[] questions,
        float nodeWidth = 180f,
        float nodeHeight = 60f,
        float horizontalSpacing = 40f,
        float verticalSpacing = 40f)
    {
        
        HashSet<Question> childrenSet = new HashSet<Question>();
        foreach (var q in questions)
        {
            if (q.choices == null) continue;
            foreach (var choice in q.choices)
            {
                if (choice.nextDialogue != null)
                    childrenSet.Add(choice.nextDialogue);
            }
        }

        Question root = null;
        foreach (var q in questions)
        {
            if (!childrenSet.Contains(q))
            {
                root = q;
                break;
            }
        }

        if (root == null)
        {
            Debug.LogWarning("DialogueTreeLayout: No root node found. Using first node as root.");
            root = questions[0];
        }

       
        NodeInfo rootInfo = BuildNodeInfo(root);

        
        CalculateSubtreeWidth(rootInfo, nodeWidth, horizontalSpacing);

        
        Dictionary<Question, Rect> result = new Dictionary<Question, Rect>();
        AssignPositions(rootInfo, 0f, 0f, nodeWidth, nodeHeight, horizontalSpacing, verticalSpacing, result);

        return result;
    }

    private static NodeInfo BuildNodeInfo(Question q)
    {
        NodeInfo info = new NodeInfo();
        info.Question = q;
        if (q.choices != null)
        {
            foreach (var choice in q.choices)
            {
                if (choice.nextDialogue != null)
                    info.Children.Add(BuildNodeInfo(choice.nextDialogue));
            }
        }
        return info;
    }

    private static float CalculateSubtreeWidth(NodeInfo node, float nodeWidth, float horizontalSpacing)
    {
        if (node.Children.Count == 0)
        {
            node.SubtreeWidth = nodeWidth;
        }
        else
        {
            float total = 0f;
            for (int i = 0; i < node.Children.Count; i++)
            {
                total += CalculateSubtreeWidth(node.Children[i], nodeWidth, horizontalSpacing);
            }
            total += horizontalSpacing * (node.Children.Count - 1);
            node.SubtreeWidth = Mathf.Max(total, nodeWidth);
        }
        return node.SubtreeWidth;
    }

    private static void AssignPositions(NodeInfo node, float startX, float startY, float nodeWidth, float nodeHeight,
        float horizontalSpacing, float verticalSpacing, Dictionary<Question, Rect> result)
    {
        if (node.Children.Count == 0)
        {
            
            node.Position = new Vector2(startX, startY);
            result[node.Question] = new Rect(node.Position, new Vector2(nodeWidth, nodeHeight));
        }
        else
        {
            
            float x = startX;
            foreach (var child in node.Children)
            {
                AssignPositions(child, x, startY + nodeHeight + verticalSpacing, nodeWidth, nodeHeight, horizontalSpacing, verticalSpacing, result);
                x += child.SubtreeWidth + horizontalSpacing;
            }

            
            float firstChildX = node.Children[0].Position.x;
            float lastChildX = node.Children[node.Children.Count - 1].Position.x;
            float parentX = (firstChildX + lastChildX) / 2f;
            node.Position = new Vector2(parentX, startY);
            result[node.Question] = new Rect(node.Position, new Vector2(nodeWidth, nodeHeight));
        }
    }
}