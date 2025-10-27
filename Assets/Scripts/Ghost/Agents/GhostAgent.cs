using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Collections.Generic;
using UnityEngine.Assertions;

using Unity.VisualScripting;

public class GhostAgent : Agent
{
    public enum TaskMode {Null, Task2, Task3};
    public TaskMode taskMode = TaskMode.Null;

    private GhostMovement mMovement;
    private Vector3 mGhostStartLocation;

    public Transform pacman;
    private Vector3 mPacmanStartLocation;

    public Transform nodes;
    private List<Transform> mNodeList;

    public LayerMask obstacleLayer;

    private Vector3 mGhostPosPrev = Vector3.zero;
    private int mAction = -1;
    private float[] mAvailableDirOneHot = { 0.0f, 0.0f, 0.0f, 0.0f };
    
    private void Awake()
    {
        Assert.IsTrue(taskMode != TaskMode.Null, "Select correct task mode");

        // Set Observation space / Action space / Max episode steps
        BehaviorParameters bp = GetComponent<BehaviorParameters>();

        // Observation Space
        if (taskMode == TaskMode.Task2)
        {
            bp.BrainParameters.VectorObservationSize = 8;
        }
        else if (taskMode == TaskMode.Task3)
        {
            bp.BrainParameters.VectorObservationSize = 10;
        }
        else
        {
            Assert.IsTrue(false, "Something wrong");
        }
        bp.BrainParameters.NumStackedVectorObservations = 1;

        // Action Space (Discrete)
        bp.BrainParameters.ActionSpec = ActionSpec.MakeDiscrete(4);

        // Max episode steps
        // For animating movement for characters, we use inner loop with mMovement's FixedUpdate
        // Since this Agent class counts all of this loop, MaxStep has larger than real decision making numbers
        // Note that Python trainer stores the state, action, and reward informations only for the decision making timestep
        MaxStep = 32 * 9;

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
        // Get node transformations in which execute the action
        mNodeList = new List<Transform>();
        foreach (Transform node in nodes)
        {
            mNodeList.Add(node);
        }
        Assert.IsTrue(mNodeList != null && mNodeList.Count > 0, "Node is not specified");
    }

    public override void OnEpisodeBegin()
    {
        Debug.Log("****************** Episode Begins ******************");

        /// This function reset the environment at the start of each training episode.

        //////////////////////////////////////////////////////////////////////
        /// <TODO> Task 2-1 (2 points)
        /// For Task 2, we use fixed random spawn position for ghost, and fixed spawn position for Pacman.
        /// 
        /// Implementation steps:
        /// 1. Reset Pacman's local position to mPacmanStartLocation
        /// 2. Reset ghost movement state
        /// 3. Randomly select and initialize spawn position for ghost from mNodeList 
        /// 
        /// Hints:
        ///     - Use Random.Range() to get random node index
        ///     - Use ResetState for initialize ghost's movement
        ///     - Use either SetPosition or direct substitution for updating ghost position


        //////////////////////////////////////////////////////////////////////

        if (taskMode == TaskMode.Task3)
        {
            //////////////////////////////////////////////////////////////////////
            /// <TODO> Task 3-1 (1 point)
            /// For Task 3, we use random spawn position also for Pacman.
            /// 
            /// Implementation steps:
            /// 1. Randomly select the random node index while ensuring that Pacman and ghost do not spawn at the same node.
            /// 2. Initialize Pacman's local position using the selected random node.


            //////////////////////////////////////////////////////////////////////
        }
        
        if (Vector2.Distance(pacman.transform.localPosition, transform.localPosition) < 2.0f)
        {
            // For handling colliding due to close distance
            transform.localPosition = mGhostStartLocation;
            pacman.transform.localPosition = mPacmanStartLocation;
        }
        mGhostPosPrev = transform.localPosition;

        Debug.Log("****************** Episode Begins ******************");

        RequestDecision();

    }

    public override void CollectObservations(VectorSensor sensor)
    {
        //////////////////////////////////////////////////////////////////////
        /// <TODO> Task 2-2 (2 points)
        /// Add observations to RL agent
        /// 
        /// Implementation steps:
        /// 1. Add observation for pacman's local position (2 dimensions)
        /// 2. Add observation for ghost's local position (2 dimensions)
        /// 3. Compute available directions using GetAvailableDirections() function and store it to mAvailableDirOneHot
        /// 4. Add observation for available directions (4 dimensions)
        /// 
        /// Hints:
        /// Use sensor.AddObservation() to add observations
        
        
        //////////////////////////////////////////////////////////////////////
        
        if (taskMode == TaskMode.Task3)
        {
            //////////////////////////////////////////////////////////////////////
            /// <TODO> Task 3-2 (1 point)
            /// Add observation for displacement between Pacman and ghost
            /// 
            /// Implementation steps:
            /// 1. Compute displacement vector from ghost to Pacman
            /// 2. Add observation for displacement vector (2 dimensions)
            

            //////////////////////////////////////////////////////////////////////
        }
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
        /// <TODO> Task 2-3 (2 points)
        /// Process the action received from RL agent and move the ghost.
        /// 
        /// Implementation steps:
        /// 1. Extract discrete action from actions
        /// 2. Get movement direction from actionDict
        /// 3. Set ghost movement direction using mMovement.SetDirection()
        /// 4. Store action to mAction for computing reward
        
        
        //////////////////////////////////////////////////////////////////////
    }

    private void ComputeReward()
    {
        //////////////////////////////////////////////////////////////////////
        /// <TODO> Task 2-4 (2 points)
        /// Compute and assign reward to the ghost agent.
        /// 
        /// Implementation steps:
        /// 1. Compute taxi distance between Pacman and ghost's local positions
        /// 2. Add distance reward (closer to Pacman -> higher reward)
        ///     Formula: exp(-0.005f * currentDistance * currentDistance) - 1
        /// 3. Add displacement reward (encourage ghost to move)
        ///    If displacement between current ghost position and previous ghost position is less than 0.5f (stationary), assign -1.0f for penalty
        /// 4. Update previous ghost position with current ghost position for next use
        /// 5. Add action penalty to encourage ghost to take meaningful actions
        ///   If current action is not in mAvailableDirOneHot, assign -1.0f penalty
        /// 
        /// Hints:
        /// Use AddReward() to assign reward to the agent.
        
        
        //////////////////////////////////////////////////////////////////////
    }

    private void OnPostActuation()
    {
        ComputeReward();
        RequestDecision();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        /// Episode termination condition
        if (collision.gameObject.CompareTag("Pacman"))
        {
            Debug.Log("****************** Episode Success ******************");
            AddReward(100f);
            EndEpisode();
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