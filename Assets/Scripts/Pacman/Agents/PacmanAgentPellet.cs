using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;

using Unity.VisualScripting;

public class PacmanAgentPellet : Agent
{
    public enum TaskMode {Null, Train, Test};
    public TaskMode taskMode = TaskMode.Null;

    // private GhostMovement mMovement;
    private PacmanMovementNew mMovement;
    private Vector3 mGhostStartLocation;

    public Transform ghost;
    public GhostAgentMy ghostAgent;

    private Vector3 mPacmanStartLocation;

    public Transform nodes;
    private List<Transform> mNodeList;
    public Transform pellets;
    private List<PelletMy> mPelletList;
    private float[] mPelletOccupancy;
    private int totalPellets = 0;
    private int pelletsEaten = 0;
    private int stepsSinceLastPellet = 0;


    public Transform passage_left;
    public Transform passage_right;

    public LayerMask obstacleLayer;

    private Vector3 mPacmanPosPrev = Vector3.zero;
    private int mAction = -1;
    private float[] mAvailableDirOneHot = { 0.0f, 0.0f, 0.0f, 0.0f };

    // public enum BehaviorMode {Chase, Frightened, Home};

    // private BehaviorMode mBehaviorMode = BehaviorMode.Chase;

    // ===============================
    // Reward / Termination Parameters
    // ===============================
    private float distanceThreshold   = 8.0f;   // 고스트와 이 거리 이하일 때부터 거리 패널티
    private float maxDistancePenalty  = 0.2f;   // 고스트와 매우 가까울 때 한 스텝 최대 패널티 (~ -0.2)

    private float movingPenalty       = -0.1f;  // 거의 안 움직였을 때 패널티
    private float blockedPenalty      = -0.3f;  // 막힌 방향으로 액션을 넣었을 때 패널티

    private float pelletReward        = 3.0f;   // 펠렛 하나당 기본 보상
    private float pelletAllReward     = 200.0f; // 모든 펠렛 클리어 보상

    private float deathPenalty        = -40.0f; // 고스트에게 잡혔을 때 페널티

    private float noPelletPenalty     = 0.0f; // 그때 줄 페널티
    private int   maxStepsWithoutPellet = 16 * 9; // 144 step 이 스텝 수 동안 펠렛을 못 먹으면 에피소드 종료

    private float score = 0.0f;

    private void Awake()
    {
        Assert.IsTrue(taskMode != TaskMode.Null, "Select correct task mode");

        BehaviorParameters bp = GetComponent<BehaviorParameters>();
        // bp.BrainParameters.VectorObservationSize = 10;
        int baseObsSize = 10;  // pacmanPos(2) + ghostPos(2) + available(4) + displacement(2)

        // pellets
        mPelletList = pellets.GetComponentsInChildren<PelletMy>().ToList();
        totalPellets = mPelletList.Count;
        mPelletOccupancy = new float[totalPellets];

        bp.BrainParameters.VectorObservationSize = baseObsSize + mPelletOccupancy.Length;
        bp.BrainParameters.NumStackedVectorObservations = 1;
        bp.BrainParameters.ActionSpec = ActionSpec.MakeDiscrete(4);

        if (taskMode == TaskMode.Train)
        {
            MaxStep = 64 * 4 * 9;
        }
        else if (taskMode == TaskMode.Test)
        {
            MaxStep = 0;
        }

        if (bp.BehaviorType == BehaviorType.InferenceOnly)
        {
            MaxStep = 64 * 4 * 9;
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
            
            foreach (Transform pellet in pellets)
            {
                pellet.gameObject.SetActive(true);
                // Debug.Log(pellet.name + " / " + pellet.localPosition);
            }

            // Reset Counter
            pelletsEaten = 0;
            stepsSinceLastPellet = 0;
            score = 0.0f;            

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

            // Debug.Log($"Score: {score} | Steps: {StepCount}");
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

        // Pellet List
        UpdatePelletOccupancy();
        sensor.AddObservation(mPelletOccupancy);
        // Debug.Log("PelletOccupancy = [" + string.Join(", ", mPelletOccupancy) + "]");
        
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
        // float survivalReward = 0.01f;
        // AddReward(survivalReward);

        Vector2 pacmanPos = transform.localPosition;
        Vector2 ghostPos = ghost.transform.localPosition;
        float currentDistance = ComputeTaxiDistance(pacmanPos, ghostPos);

        // 1. 거리 페널티 (위험 구역)
        if (currentDistance < distanceThreshold)
        {
            float t = (distanceThreshold - currentDistance) / distanceThreshold; // 0~1
            t = Mathf.Clamp01(t);

            float distancePenalty = -maxDistancePenalty * t * t;
            AddReward(distancePenalty);
        }

        // 2. 안 움직임 페널티
        float displacement = Vector2.Distance(pacmanPos, mPacmanPosPrev);
        if (displacement < 0.5f)
        {
            AddReward(movingPenalty);
        }

        // 3. 막힌 방향 액션 페널티
        if (mAvailableDirOneHot[mAction] == 0.0f)
        {
            AddReward(blockedPenalty);
        }

        // 4. 위치 업데이트
        mPacmanPosPrev = pacmanPos;
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

        // 펠렛 안 먹은 스텝 카운트 증가
        stepsSinceLastPellet++;
        // Debug.Log(stepsSinceLastPellet - maxStepsWithoutPellet);

        // 펠렛이 남아 있는데 너무 오래 펠렛을 안 먹었다 → 도망만 다니는 에피소드라고 보고 종료
        if (HasRemainingPellets() && stepsSinceLastPellet >= maxStepsWithoutPellet)
        {
            Debug.Log("Early termination: no pellet eaten for too long.");
            AddReward(noPelletPenalty);   // -30 정도
            EndEpisode();
            ghostAgent?.EndEpisode();

            return;
        }
        
        RequestDecision();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (taskMode == TaskMode.Train)
        {
            if (collision.gameObject.CompareTag("Ghost"))
            {
                Debug.Log("****************** Pacman Episode Fail ******************");
                AddReward(deathPenalty);
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


    public void OnPelletEaten(PelletMy pellet)
    {
        if (taskMode != TaskMode.Train)
            return;

        pellet.gameObject.SetActive(false);

        pelletsEaten++;
        stepsSinceLastPellet = 0;   // 펠렛을 먹었으니 “오래 안 먹음” 카운터 리셋

        AddReward(pelletReward);    // +3.0
        score += pelletReward;

        if (!HasRemainingPellets())
        {
            OnAllPelletsCleared();
        }
    }

    public void OnAllPelletsCleared()
    {
        if (taskMode != TaskMode.Train)
            return;

        AddReward(pelletAllReward);   // +200
        score += pelletAllReward;

        Debug.Log("****************** All Pellets Cleared ******************");
        EndEpisode();
    }

    private bool HasRemainingPellets()
    {
        foreach (Transform t in pellets)
        {
            if (t.gameObject.activeSelf)
                return true;
        }
        return false;
    }

    private void UpdatePelletOccupancy()
    {
        int activeCount = 0;

        for (int i = 0; i < mPelletList.Count; i++)
        {
            bool isActive = mPelletList[i].gameObject.activeSelf;
            mPelletOccupancy[i] = mPelletList[i].gameObject.activeSelf ? 1.0f : 0.0f;
            if (isActive)
                activeCount++;            
        }

        // Debug.Log($"Active Pellets: {activeCount}");
    }

}