using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{

    public LayerMask obstacleLayer;
    public List<Vector2> mAvailableDirections { get; private set; }
    private static int sNumNodes = 0;

    private void Start()
    {

        sNumNodes++;
        gameObject.name = $"Node {sNumNodes}";
        
        mAvailableDirections = new List<Vector2>();

        GetAvailableDirections(Vector2.up);
        GetAvailableDirections(Vector2.down);
        GetAvailableDirections(Vector2.left);
        GetAvailableDirections(Vector2.right);
    }

    private void GetAvailableDirections(Vector2 direction)
    {
        RaycastHit2D hit = Physics2D.BoxCast(transform.position, Vector2.one * 0.5f, 0f, direction, 1.0f, obstacleLayer);

        if (hit.collider == null)
        {
            mAvailableDirections.Add(direction);
        }
    }
}
