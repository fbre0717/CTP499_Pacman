using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;

using Unity.VisualScripting;

public class PacmanAgent : Agent
{
    public enum TaskMode {Null, Train, Test};
    public TaskMode taskMode = TaskMode.Null;

    // private GhostMovement mMovement;
    private PacmanMovementNew mMovement;
    private Vector3 mGhostStartLocation;

    public Transform ghost;
    private Vector3 mPacmanStartLocation;

    public Transform nodes;
    private List<Transform> mNodeList;

    public Transform passage_left;
    public Transform passage_right;

    public LayerMask obstacleLayer;

    private Vector3 mPacmanPosPrev = Vector3.zero;
    private int mAction = -1;
    private float[] mAvailableDirOneHot = { 0.0f, 0.0f, 0.0f, 0.0f };

    // public enum BehaviorMode {Chase, Frightened, Home};

    // private BehaviorMode mBehaviorMode = BehaviorMode.Chase;

    private void Awake()
    {
        Assert.IsTrue(taskMode != TaskMode.Null, "Select correct task mode");

        BehaviorParameters bp = GetComponent<BehaviorParameters>();
        bp.BrainParameters.VectorObservationSize = 10;
        bp.BrainParameters.NumStackedVectorObservations = 1;
        bp.BrainParameters.ActionSpec = ActionSpec.MakeDiscrete(4);

        if (taskMode == TaskMode.Train)
        {
            MaxStep = 64 * 9;
        }
        else if (taskMode == TaskMode.Test)
        {
            MaxStep = 0;
        }

        // Get GhostMovment object
        mMovement = GetComponent<PacmanMovementNew>();
        mMovement.OnNewNodeArrived += OnPostActuation;
        
        mGhostStartLocation = ghost.transform.localPosition;
        mPacmanStartLocation = transform.localPosition;

        mPacmanPosPrev = Vector3.zero;
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

    public override void OnEpisodeBegin()
    {
        if (taskMode == TaskMode.Train)
        {
            //////////////////////////////////////////////////////////////////////
            /// <TODO> Task 5-0
            /// Copy the code you've implemented in Task 2-1 & 3-1
            /// 
            /// Note:
            /// For the code from Task 3-1, remove the conditional statement that checks taskMode
            
            ghost.transform.localPosition = mGhostStartLocation;
            mMovement.ResetState();

            int pacmanNodeIndex = Random.Range(0, mNodeList.Count);
            Transform pacmanSpawnNode = mNodeList[pacmanNodeIndex];
            transform.localPosition = pacmanSpawnNode.localPosition;

            int ghostNodeIndex;
            do
            {
                ghostNodeIndex = Random.Range(0, mNodeList.Count);
            } while (pacmanNodeIndex == ghostNodeIndex);

            Transform ghostSpawnNode = mNodeList[ghostNodeIndex];
            ghost.transform.localPosition = ghostSpawnNode.localPosition;

            //////////////////////////////////////////////////////////////////////

            if (Vector2.Distance(transform.localPosition, ghost.transform.localPosition) < 2.0f)
            {
                ghost.transform.localPosition = mGhostStartLocation;
                transform.localPosition = mPacmanStartLocation;
            }
            mPacmanPosPrev = transform.localPosition;

            Debug.Log("****************** Pacman Episode Begins ******************");
            RequestDecision();
        }
        else if (taskMode == TaskMode.Test)
        {
            // Do nothing. GameManager and Ghost will handle the ghost's behavior
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        //////////////////////////////////////////////////////////////////////
        /// <TODO> Task 5-0
        /// Copy the code you've implemented in Task 2-2 & 3-2
        /// 
        /// Note:
        /// For the code from Task 3-2, remove the conditional statement that checks taskMode
        
        Vector2 pacmanPos = transform.localPosition;
        sensor.AddObservation(pacmanPos);

        Vector2 ghostPos = ghost.transform.localPosition;
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
        int action = actions.DiscreteActions[0];
        Vector3 moveDir = actionDict[action];
        mMovement.SetDirection(moveDir);
        mAction = action;
    }

    private void ComputeReward()
    {
        // 0. 생존 보상
        float survivalReward = 0.01f;
        AddReward(survivalReward);

        Vector2 pacmanPos = transform.localPosition;
        Vector2 ghostPos = ghost.transform.localPosition;
        float currentDistance = ComputeTaxiDistance(pacmanPos, ghostPos);

        // 1. 거리 페널티 (위험 구역)
        float distanceThreshold = 10.0f;
        float maxDistancePenalty = 1.0f;

        if (currentDistance < distanceThreshold)
        {
            float t = (distanceThreshold - currentDistance) / distanceThreshold; // 0~1
            t = Mathf.Clamp01(t);

            float distancePenalty = -maxDistancePenalty * t * t;
            AddReward(distancePenalty);
        }

        // 2. 안 움직임 페널티
        float movingPenalty = -0.5f;
        float displacement = Vector2.Distance(pacmanPos, mPacmanPosPrev);
        if (displacement < 0.5f)
        {
            AddReward(movingPenalty);
        }

        // 3. 막힌 방향 액션 페널티
        float blockedPenalty = -1.0f;
        mPacmanPosPrev = pacmanPos;
        if (mAvailableDirOneHot[mAction] == 0.0f)
        {
            AddReward(blockedPenalty);
        }
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
        if (taskMode == TaskMode.Train)
        {
            if (collision.gameObject.CompareTag("Ghost"))
            {
                Debug.Log("****************** Episode Fail ******************");
                AddReward(-100f);
                EndEpisode();
            }
        }
    }

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
}