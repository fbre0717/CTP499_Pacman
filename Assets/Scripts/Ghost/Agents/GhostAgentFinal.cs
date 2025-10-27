using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;

using Unity.VisualScripting;

public class GhostAgentFinal : Agent
{
    public enum TaskMode {Null, Train, Test};
    public TaskMode taskMode = TaskMode.Null;

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

    public enum BehaviorMode {Chase, Frightened, Home};

    private BehaviorMode mBehaviorMode = BehaviorMode.Chase;

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
        mMovement = GetComponent<GhostMovement>();
        mMovement.OnNewNodeArrived += OnPostActuation;
        mGhostStartLocation = transform.localPosition;
        mPacmanStartLocation = pacman.transform.localPosition;

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
            

            //////////////////////////////////////////////////////////////////////

            if (Vector2.Distance(pacman.transform.localPosition, transform.localPosition) < 2.0f)
            {
                transform.localPosition = mGhostStartLocation;
                pacman.transform.localPosition = mPacmanStartLocation;
            }
            mGhostPosPrev = transform.localPosition;

            Debug.Log("****************** Episode Begins ******************");
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
        if (taskMode == TaskMode.Train)
        {
            //////////////////////////////////////////////////////////////////////
            /// <TODO> Task 5-0
            /// Copy the code you've implemented in Task 2-3
            
            
            /// ////////////////////////////////////////////////////////////////////
            
        }

        else if (taskMode == TaskMode.Test)
        {
            //////////////////////////////////////////////////////////////////////
            /// <TODO> Task 5-1 (2 points)
            /// Change the action according to the mBehaviorMode
            ///     - case 1: BehaviorMode.Frightened -> Reverse the direction (Up <-> Down, Right <-> Left)
            ///     - case 2: BehaviorMode.Home -> Set Up direction only
            
            /// ////////////////////////////////////////////////////////////////////
        }
    }

    private void ComputeReward()
    {
        //////////////////////////////////////////////////////////////////////
        /// <TODO> Task 5-0
        /// Copy the code you've implemented in Task 2-4
        
        
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
        if (taskMode == TaskMode.Train)
        {
            if (collision.gameObject.CompareTag("Pacman"))
            {
                Debug.Log("****************** Episode Success ******************");
                AddReward(100f);
                EndEpisode();
            }
        }
        else if (taskMode == TaskMode.Test)
        {
            if (collision.gameObject.CompareTag("Pacman"))
            {
                if (mBehaviorMode == BehaviorMode.Chase)
                {
                    var gameManager = FindObjectOfType<GameManager>();
                    gameManager.PacmanEaten();
                    return;
                }
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

    public void SetSpeed(float speed)
    {
        mMovement.ChangeSpeed(speed);
    }

    public void SetPositionHomeIn(Vector2 pos)
    {
        mMovement.SetPosition(pos);
    }
    
    public void ResetState()
    {
        mMovement.ResetState();
    }

    public void ResetPosNState()
    {
        mMovement.ResetState();
        mMovement.SetPosition(mGhostStartLocation);
        mGhostPosPrev = mGhostStartLocation;
    }

    public void RequestDirection()
    {
        RequestDecision();
    }

    public void SetDirectionHomeIn()
    {
        mMovement.SetDirection(Vector2.zero);
    }

    public void SetBehaviorMode(BehaviorMode mode)
    {
        mBehaviorMode = mode;
    }

    public BehaviorMode GetBehaviorMode()
    {
        return mBehaviorMode;
    }
}