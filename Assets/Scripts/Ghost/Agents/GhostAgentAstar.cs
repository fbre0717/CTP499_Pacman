using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;

public class GhostAgentAstar : Agent
{
    private GhostMovement mMovement;
    private Vector3 mGhostStartLocation;

    public Transform pacman;
    private Vector3 mPacmanStartLocation;

    public Transform nodes;
    private List<Transform> mNodeList;

    private List<Transform> mPath = new List<Transform>();
    public bool visPath = true;

    private Vector2 bestDirection = Vector2.zero;

    public LayerMask obstacleLayer;

    private Vector3 mGhostPosPrev = Vector3.zero;
    private int mAction = -1;
    private float[] mAvailableDirOneHot = { 0.0f, 0.0f, 0.0f, 0.0f };

    private int agentStepCount;

    private void Awake()
    {
        BehaviorParameters bp = GetComponent<BehaviorParameters>();

        bp.BrainParameters.VectorObservationSize = 10;
        bp.BrainParameters.NumStackedVectorObservations = 1;
        bp.BrainParameters.ActionSpec = ActionSpec.MakeDiscrete(4);

        MaxStep = 32 * 9;

        mMovement = GetComponent<GhostMovement>();
        mMovement.OnNewNodeArrived += OnPostActuation;

        mGhostStartLocation = transform.localPosition;
        mPacmanStartLocation = pacman.transform.localPosition;
        agentStepCount = 0;

        mGhostPosPrev = Vector3.zero;
        mAction = -1;
        mAvailableDirOneHot = new float[] { 0.0f, 0.0f, 0.0f, 0.0f };

    }

    private void Start()
    {
        mNodeList = new List<Transform>();
        foreach (Transform node in nodes)
        {
            mNodeList.Add(node);
        }
        Assert.IsTrue(mNodeList != null && mNodeList.Count > 0, "Node is not specified");
    }

    private void OnDrawGizmos()
    {
        if (mPath == null || mPath.Count == 0 || visPath == false) return;

        Gizmos.color = Color.red;

        for (int i = 0; i < mPath.Count; i++)
        {
            Vector3 worldPos = mPath[i].position;
            Gizmos.DrawSphere(worldPos, 0.1f);

            if (i < mPath.Count - 1)
            {
                Vector3 nextWorldPos = mPath[i + 1].position;
                Gizmos.DrawLine(worldPos, nextWorldPos);
            }
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut){    
        int bestIdx = 0;
        Vector3[] directions = new Vector3[]
        {
            Vector3.up,
            Vector3.down,
            Vector3.right,
            Vector3.left
        };
        
        //////////////////////////////////////////////////////////////////////
        /// <TODO> Task 4-2 (2 points)
        /// Implement the heuristic function for moving the ghost along the A* path
        /// 
        /// Implementation steps:
        /// 1. Compute bestDirection using the nodes in mPath
        ///    a) If (agentStepCount + 1) is less than the number of nodes in mPath, compute bestDirection using the current node and the next node in mPath
        ///    b) Otherwise, set bestDirection using the final two nodes in mPath list (Exception handling)
        /// 2. Initialize maxDot (max dot product value) as -infinity
        /// 3. For each direction in directions array, compute the dot product between bestDirection and the direction
        /// 4. If the dot product is greater than maxDot, update maxDot and bestIdx
        /// 
        /// Hint:
        /// Use System.Array.IndexOf() to get the index of an element in an array
        
        
        /// ////////////////////////////////////////////////////////////////////
        
        ActionSegment<int> DiscreteActions = actionsOut.DiscreteActions;
        DiscreteActions[0] = bestIdx;
        mAction = bestIdx;

    }

    public override void OnEpisodeBegin()
    {
        //////////////////////////////////////////////////////////////////////
        /// <TODO> Task 4-0
        /// Copy the code you've implemented in Task 2-1 & 3-1
        /// 
        /// Note:
        /// For the code from Task 3-1, remove the conditional statement that checks taskMode
        

        //////////////////////////////////////////////////////////////////////
        
        if (Vector2.Distance(pacman.transform.localPosition, transform.localPosition) < 2.0f)
        {
            transform.localPosition = mGhostStartLocation;
            pacman.transform.localPosition = mPacmanStartLocation;
        }
        mGhostPosPrev = transform.localPosition;

        //////////////////////////////////////////////////////////////////////
        /// <TODO> Task 4-1 (1 point)
        /// A* pathfinding to find a path from the ghost to pacman
        /// 
        /// 1. Get currentNode using GetClosestNode() for the ghost's current local position
        /// 2. Get targetNode using GetClosestNode() for pacman's current local position
        /// 3. Find A* path from currentNode to targetNode using FindPath(), and store it in mPath
        

        //////////////////////////////////////////////////////////////////////

        agentStepCount = 0;
        Debug.Log("****************** Episode Begins ******************");
        RequestDecision();

    }

    public override void CollectObservations(VectorSensor sensor)
    {
        //////////////////////////////////////////////////////////////////////
        /// <TODO> Task 4-0
        /// Copy the code you've implemented in Task 2-2 & 3-2
        /// 
        /// Note:
        /// For the code from Task 3-2, remove the conditional statement that checks taskMode
        
        
        /// ////////////////////////////////////////////////////////////////////
    }

    private Dictionary<int, Vector3> actionDict = new Dictionary<int, Vector3>{
        { 0, Vector2.up },
        { 1, Vector2.down },
        { 2, Vector2.right },
        { 3, Vector2.left }
    };

    public override void OnActionReceived(ActionBuffers actions)
    {
        //////////////////////////////////////////////////////////////////////
        /// <TODO> Task 4-0
        /// Copy the code you've implemented in Task 2-3
        
        
        /// ////////////////////////////////////////////////////////////////////
    }

    private void ComputeReward()
    {
        //////////////////////////////////////////////////////////////////////
        /// <TODO> Task 4-0
        /// Copy the code you've implemented in Task 2-4
        
        
        /// ////////////////////////////////////////////////////////////////////
    }

    private void OnPostActuation()
    {
        agentStepCount++;
        ComputeReward();
        RequestDecision();     
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Pacman"))
        {
            Debug.Log("****************** Episode Success ******************");
            AddReward(100f);
            EndEpisode();
        }

    }

    /// Helper functions
    private float[] GetAvailableDirections()
    {
        float[] one_hot_direction = { 0.0f, 0.0f, 0.0f, 0.0f };

        RaycastHit2D up_hit = Physics2D.BoxCast(transform.position, Vector2.one * 0.5f, 0f, Vector2.up, 1.0f, obstacleLayer);
        RaycastHit2D down_hit = Physics2D.BoxCast(transform.position, Vector2.one * 0.5f, 0f, Vector2.down, 1.0f, obstacleLayer);
        RaycastHit2D right_hit = Physics2D.BoxCast(transform.position, Vector2.one * 0.5f, 0f, Vector2.right, 1.0f, obstacleLayer);
        RaycastHit2D left_hit = Physics2D.BoxCast(transform.position, Vector2.one * 0.5f, 0f, Vector2.left, 1.0f, obstacleLayer);

        one_hot_direction[0] = (up_hit.collider == null) ? 1.0f : 0.0f;
        one_hot_direction[1] = (down_hit.collider == null) ? 1.0f : 0.0f;
        one_hot_direction[2] = (right_hit.collider == null) ? 1.0f : 0.0f;
        one_hot_direction[3] = (left_hit.collider == null) ? 1.0f : 0.0f;

        return one_hot_direction;
    }


    public float ComputeTaxiDistance(Vector2 a, Vector2 b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }


    private List<Transform> FindPath(Transform start, Transform target)
    {
        var openList = new List<Transform> { start };
        var cameFrom = new Dictionary<Transform, Transform>();
        var gScore = new Dictionary<Transform, float> { [start] = 0 };
        var fScore = new Dictionary<Transform, float> { [start] = Vector3.Distance(start.localPosition, target.localPosition) };

        while (openList.Count > 0)
        {
            Transform current = openList.OrderBy(n => fScore.ContainsKey(n) ? fScore[n] : Mathf.Infinity).First();

            if (current == target)
            {
                return ReconstructPath(cameFrom, current);
            }

            openList.Remove(current);

            foreach (Transform neighbor in GetNeighbors(current))
            {
                float tentativeG = gScore[current] + Vector3.Distance(current.localPosition, neighbor.localPosition);

                if (!gScore.ContainsKey(neighbor) || tentativeG < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeG;
                    fScore[neighbor] = tentativeG + Vector3.Distance(neighbor.localPosition, target.localPosition);

                    if (!openList.Contains(neighbor)) openList.Add(neighbor);
                }
            }
        }
        return null;
    }

    private List<Transform> ReconstructPath(Dictionary<Transform, Transform> cameFrom, Transform current)
    {
        var totalPath = new List<Transform> { current }; 

        while (cameFrom.ContainsKey(current)){
            current = cameFrom[current];
            totalPath.Insert(0, current);
        }
        return totalPath;
    }

    private Transform GetClosestNode(Vector3 pos)
    {
        Transform closest = null;
        float minDist = float.MaxValue;

        foreach (var node in mNodeList)
        {
            float dist = Vector3.Distance(pos, node.localPosition);

            if (dist < minDist)
            {
                minDist = dist;
                closest = node;
            }
        }
        return closest;
    }
    
    private List<Transform> GetNeighbors(Transform node)
    {
        List<Transform> neighbors = new List<Transform>();
        Vector3 pos = node.localPosition;

        float nodeSpacing = 1.0f;
        Vector3[] directions = new Vector3[]{
            new Vector3(nodeSpacing, 0, 0),
            new Vector3(-nodeSpacing, 0, 0),
            new Vector3(0, nodeSpacing, 0),
            new Vector3(0, -nodeSpacing, 0)
        };

        foreach (var dir in directions){
            Vector3 neighborPos = pos + dir;
            Transform neighborNode = mNodeList.FirstOrDefault(n => Vector3.Distance(n.localPosition, neighborPos) < 0.1f);

            if (neighborNode != null){
                neighbors.Add(neighborNode);
            }
        }
        return neighbors;
    }
}