using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class PathFinder : MonoBehaviour
{
    public List<Vector2> PathToTarget;
    public List<Node> CheckedNodes = new List<Node>();
    public List<Node> FreeNodes = new List<Node>();
    List<Node> WaitingNodes = new List<Node>();
    //public GameObject Target;
    public LayerMask SolidLayer;

    private void Start()
    {

    }

    void Update()
    {
        //PathToTarget = GetPath(Target.transform.position);
    }

    public List<Vector2> GetPath(Vector2 start, Vector2 target)
    {
        PathToTarget = new List<Vector2>();
        CheckedNodes = new List<Node>();
        WaitingNodes = new List<Node>();

        Vector2 StartPosition = new Vector2(Mathf.Round(start.x), Mathf.Round(start.y));
        Vector2 TargetPosition = new Vector2(Mathf.Round(target.x), Mathf.Round(target.y));
        
        if(StartPosition == TargetPosition) return PathToTarget;

        Node startNode = new Node(0, StartPosition, TargetPosition, null);
        CheckedNodes.Add(startNode);
        WaitingNodes.AddRange(GetNeighbourNodes(startNode));
        while(WaitingNodes.Count > 0)
        {
            Node nodeToCheck = WaitingNodes.Where(x => x.F == WaitingNodes.Min(y => y.F)).FirstOrDefault();

            if (nodeToCheck.Position == TargetPosition)
            {
                return CalculatePathFromNode(nodeToCheck);
            }

            var walkable = !Physics2D.OverlapCircle(nodeToCheck.Position, 0.1f, SolidLayer);
            if(!walkable)
            {
                WaitingNodes.Remove(nodeToCheck);
                CheckedNodes.Add(nodeToCheck);
            }
            else if(walkable)
            {
                WaitingNodes.Remove(nodeToCheck);
                if(!CheckedNodes.Where(x => x.Position == nodeToCheck.Position).Any()) {
                    CheckedNodes.Add(nodeToCheck);
                    WaitingNodes.AddRange(GetNeighbourNodes(nodeToCheck));
                } 
            }
        }
        FreeNodes = CheckedNodes;
        Debug.Log("путь не обнаружен");
        return PathToTarget;
    }

    public List<Vector2> CalculatePathFromNode(Node node)
    {
        var path = new List<Vector2>();
        Node currentNode = node;

        while(currentNode.PreviousNode != null)
        {
            path.Add(new Vector2(currentNode.Position.x, currentNode.Position.y));
            currentNode = currentNode.PreviousNode;
        }

        return path;
    }

    List<Node> GetNeighbourNodes (Node node)
    {
        var Neighbours = new List<Node>();

        Neighbours.Add(new Node(node.G + 1, new Vector2(
            node.Position.x-1, node.Position.y), 
            node.TargetPosition, 
            node));
        Neighbours.Add(new Node(node.G + 1, new Vector2(
            node.Position.x+1, node.Position.y),
            node.TargetPosition,
            node));
        Neighbours.Add(new Node(node.G + 1, new Vector2(
            node.Position.x, node.Position.y-1),
            node.TargetPosition,
            node));
        Neighbours.Add(new Node(node.G + 1, new Vector2(
            node.Position.x, node.Position.y+1),
            node.TargetPosition,
            node));
        return Neighbours;
    }

    //void OnDrawGizmos()
    //{
    //    foreach (var item in CheckedNodes)
    //    {
    //        Gizmos.color = Color.yellow;
    //        Gizmos.DrawSphere(new Vector2(item.Position.x, item.Position.y), 0.1f);
    //    }
    //    if (PathToTarget != null)
    //    foreach (var item in PathToTarget)
    //    {
    //        Gizmos.color = Color.red;
    //        Gizmos.DrawSphere(new Vector2(item.x, item.y), 0.2f);
    //    }
    //}

}

public class Node 
{
    public Vector2 Position;
    public Vector2 TargetPosition;
    public Node PreviousNode;
    public int F; // F=G+H
    public int G; // расстояние от старта до ноды
    public int H; // расстояние от ноды до цели

    public Node(int g, Vector2 nodePosition, Vector2 targetPosition, Node previousNode)
    {
        Position = nodePosition;
        TargetPosition = targetPosition;
        PreviousNode = previousNode;
        G = g;
        H = (int)Mathf.Abs(targetPosition.x - Position.x) + (int)Mathf.Abs(targetPosition.y - Position.y);
        F = G + H;
    }
}