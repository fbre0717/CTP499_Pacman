using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;

using Unity.VisualScripting;

//////////////////////////////////////////////////////////////////////////////////
/// This class is not an assignment. 
/// But we kindly give you this for those who need BC training for task 5
//////////////////////////////////////////////////////////////////////////////////

public class GhostAgentFinalAstar : Agent
{
    private GhostMovement mMovement;
    private Vector3 mGhostStartLocation;

    public Transform pacman;
    private Vector3 mPacmanStartLocation;

    public Transform nodes;
    private List<Transform> mNodeList;

    public Transform passage_left;
    public Transform passage_right;

    public LayerMask obstacleLayer;

    private Vector3 mGhostPosPrev = Vector3.zero;
    private int mAction = -1;
    private float[] mAvailableDirOneHot = { 0.0f, 0.0f, 0.0f, 0.0f };

    private List<Transform> mPath = new List<Transform>();
    public bool visPath = true;

    private int pathStepCount;

    private void Awake()
    {
        BehaviorParameters bp = GetComponent<BehaviorParameters>();
        bp.BrainParameters.VectorObservationSize = 10;
        bp.BrainParameters.NumStackedVectorObservations = 1;

        bp.BrainParameters.ActionSpec = ActionSpec.MakeDiscrete(4);

        MaxStep = 64 * 9;

        mMovement = GetComponent<GhostMovement>();
        mMovement.OnNewNodeArrived += OnPostActuation;
        mGhostStartLocation = transform.localPosition;
        mPacmanStartLocation = pacman.transform.localPosition;

        mGhostPosPrev = Vector3.zero;
        mAction = -1;
        mAvailableDirOneHot = new float[] { 0.0f, 0.0f, 0.0f, 0.0f };

        pathStepCount = 0;
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


    public override void Heuristic(in ActionBuffers actionsOut)
    {
        int bestIdx = 0;
        Vector3[] directions = new Vector3[]
        {
            Vector3.up,
            Vector3.down,
            Vector3.right,
            Vector3.left
        };

        //////////////////////////////////////////////////////////////////////
        /// Copy the code you've implemented in Task 4-2     

        /// Note:
        /// Change the step 1: 
        ///     - Check the distance between the next path eleemnt and current local position using pathStepCount 
        ///     - If the distance < 1.0f Increase pathStepCount
        ///     - Calculate bestDirection using adjacent path element with pathStepCount

        Vector3 currentPos = transform.localPosition;
        if (pathStepCount + 1 < mPath.Count)
        {
            Vector3 nextPos = mPath[pathStepCount + 1].localPosition;
            if (Vector3.Distance(currentPos, nextPos) < 1.0f)
            {
                pathStepCount++;
            }
        }
        
        Vector3 frPos, toPos;
        if (pathStepCount + 1 < mPath.Count)
        {
            frPos = mPath[pathStepCount].localPosition;
            toPos = mPath[pathStepCount + 1].localPosition;
        }
        else
        {
            frPos = mPath[mPath.Count - 2].localPosition;
            toPos = mPath[mPath.Count - 1].localPosition;
        }
        Vector2 bestDirection = (new Vector2(toPos.x, toPos.y) - new Vector2(frPos.x, frPos.y)).normalized;

        float maxDot = float.NegativeInfinity;

        for (int i = 0; i < directions.Length; i++)
        {
            Vector2 dir2 = new Vector2(directions[i].x, directions[i].y);
            float dot = Vector2.Dot(bestDirection, dir2);
            if (dot > maxDot)
            {
                maxDot = dot;
                bestIdx = i;
            }
        }        

        /// //////////////////////////////////////////////////////////////////////

        ActionSegment<int> DiscreteActions = actionsOut.DiscreteActions;
        DiscreteActions[0] = bestIdx;
        mAction = bestIdx;
    }


    public override void OnEpisodeBegin()
    {
        //////////////////////////////////////////////////////////////////////
        /// Copy the code you've implemented in Task 5-0
        /// 
        /// Note:
        /// For the code from Task 5-0, remove the conditional statement that checks taskMode

        pacman.transform.localPosition = mPacmanStartLocation;
        mMovement.ResetState();

        int ghostNodeIndex = Random.Range(0, mNodeList.Count);
        Transform ghostSpawnNode = mNodeList[ghostNodeIndex];
        transform.localPosition = ghostSpawnNode.localPosition;

        int pacmanNodeIndex;
        do
        {
            pacmanNodeIndex = Random.Range(0, mNodeList.Count);
        } while (pacmanNodeIndex == ghostNodeIndex);

        Transform pacmanSpawnNode = mNodeList[pacmanNodeIndex];
        pacman.transform.localPosition = pacmanSpawnNode.localPosition;

        //////////////////////////////////////////////////////////////////////

        if (Vector2.Distance(pacman.transform.localPosition, transform.localPosition) < 2.0f)
        {
            transform.localPosition = mGhostStartLocation;
            pacman.transform.localPosition = mPacmanStartLocation;
        }
        mGhostPosPrev = transform.localPosition;

        Debug.Log("****************** Episode Begins ******************");
        RequestDecision();


        //////////////////////////////////////////////////////////////////////
        /// Copy the code you've implemented in Task 4-1    

        Transform currentNode = GetClosestNode(transform.localPosition);
        Transform targetNode = GetClosestNode(pacman.transform.localPosition);
        mPath = FindPath(currentNode, targetNode);

        //////////////////////////////////////////////////////////////////////

        pathStepCount = 0;

        Debug.Log("****************** Episode Begins ******************");
        RequestDecision();

    }

    public override void CollectObservations(VectorSensor sensor)
    {
        //////////////////////////////////////////////////////////////////////
        /// Copy the code you've implemented in Task 5-0
        /// 
        /// Note:
        /// Remove the conditional statement that checks taskMode

        Vector2 pacmanPos = pacman.transform.localPosition;
        sensor.AddObservation(pacmanPos);

        Vector2 ghostPos = transform.localPosition;
        sensor.AddObservation(ghostPos);

        mAvailableDirOneHot = GetAvailableDirections();
        sensor.AddObservation(mAvailableDirOneHot);

        Vector2 displacement = pacmanPos - ghostPos;
        sensor.AddObservation(displacement);   

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
        /// Copy the code you've implemented in Task 5-0

        int action = actions.DiscreteActions[0];
        Vector3 moveDir = actionDict[action];
        mMovement.SetDirection(moveDir);
        mAction = action;

        /// ////////////////////////////////////////////////////////////////////
    }

    private void ComputeReward()
    {
        //////////////////////////////////////////////////////////////////////
        /// Copy the code you've implemented in Task 5-0

        Vector2 pacmanPos = pacman.transform.localPosition;
        Vector2 ghostPos = transform.localPosition;
        float currentDistance = ComputeTaxiDistance(pacmanPos, ghostPos);
        float distanceReward = Mathf.Exp(-0.005f * currentDistance * currentDistance) - 1.0f;
        AddReward(distanceReward);

        float movingReward = -1.0f;
        float displacement = Vector2.Distance(ghostPos, mGhostPosPrev);
        if (displacement < 0.5f)
        {
            AddReward(movingReward);
        }

        float blockedReward = -1.0f;
        mGhostPosPrev = ghostPos;
        if (mAvailableDirOneHot[mAction] == 0.0f)
        {
            AddReward(blockedReward);
        }

        /// ////////////////////////////////////////////////////////////////////
    }

    private void OnPostActuation()
    {
        if (Vector2.Distance(passage_left.localPosition, transform.localPosition) < 0.5)
        {
            transform.localPosition = passage_right.localPosition + Vector3.left;
        }
        else if (Vector2.Distance(passage_right.localPosition, transform.localPosition) < 0.5)
        {
            transform.localPosition = passage_left.localPosition + Vector3.right;
        }

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

        while (cameFrom.ContainsKey(current))
        {
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

        foreach (var dir in directions)
        {
            if (CheckPossibleDirection(node.position, dir)) continue;

            var neighbor = mNodeList
                .Where(n =>
                {
                    Vector3 diff = n.localPosition - pos;
                    return n != node && Vector3.Dot(diff.normalized, dir.normalized) > 0.99f;
                })
                .OrderBy(n => Vector3.Distance(pos, n.localPosition))
                .FirstOrDefault();

            if (neighbor != null)
                neighbors.Add(neighbor);
        }

        return neighbors;
    }

    public bool CheckPossibleDirection(Vector3 pos, Vector2 direction)
    {
        RaycastHit2D hit = Physics2D.BoxCast(pos, Vector2.one * 0.5f, 0f, direction, 1.0f, obstacleLayer);
        return hit.collider != null;
    }

}