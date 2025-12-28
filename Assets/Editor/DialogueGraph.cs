using System.Collections.Generic;
using UnityEngine;

public class DialogueGraph
{
    Dictionary<Question, List<Question>> adjacencyList;
    HashSet<Question> allNodes;

    public DialogueGraph(Question[] questions)
    {
        adjacencyList = new Dictionary<Question, List<Question>>();
        allNodes = new HashSet<Question>();

        if (questions == null || questions.Length == 0)
        {
            return;
        }

        for (int i = 0; i < questions.Length; i++)
        {
            Question currentQuestion = questions[i];
            allNodes.Add(currentQuestion);

            List<Question> currentNeighbors = new List<Question>();

            if (currentQuestion.choices != null)
            {
                foreach (var choice in currentQuestion.choices)
                {
                    Question target = choice.nextDialogue;
                    if (target != null)
                    {
                        currentNeighbors.Add(target);
                    }
                }
            }

            if (!adjacencyList.ContainsKey(currentQuestion))
            {
                adjacencyList.Add(currentQuestion, currentNeighbors);
            }
        }
    }

    public List<Question> GetNeighbors(Question node)
    {
        if (node == null)
        {
            return new List<Question>();
        }

        if (adjacencyList.ContainsKey(node))
        {
            return adjacencyList[node];
        }

        return new List<Question>();
    }

    public HashSet<Question> FindEntryPoints()
    {
        HashSet<Question> hasIncomingEdges = new HashSet<Question>();
        foreach (var neighborList in adjacencyList.Values)
        {
            foreach (var question in neighborList)
            {
                hasIncomingEdges.Add(question);
            }
        }

        HashSet<Question> entryPoints = new HashSet<Question>(allNodes);
        entryPoints.ExceptWith(hasIncomingEdges);
        
        return entryPoints;
    }

    public HashSet<Question> GetReachableNodes(Question start)
    {
        if (start == null)
        {
           return new HashSet<Question>();
        }

        HashSet<Question> visited = new HashSet<Question>();
        DFS(start, visited);
        return visited;
    }

    private void DFS(Question current, HashSet<Question> visited)
    {
        visited.Add(current);
        List<Question> neighbors = GetNeighbors(current);
        
        foreach (Question neighbor in neighbors)
        {
            if (!visited.Contains(neighbor))
            {
                DFS(neighbor, visited);
            }
        }
    }

    public HashSet<Question> GetAllNodes()
    {
        return allNodes;
    }
}