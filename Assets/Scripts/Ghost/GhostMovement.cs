using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Rigidbody2D))]
public class GhostMovement : PacmanMovement
{
    public Tilemap tileMap;
    public Vector2 mCurrCellPos { get; private set; }
    public Vector2 mNextCellPos { get; private set; }

    public bool bMoving { get; private set; } = false;
    private int mMovingCount = 0;
    private int mMaxMovingCount = 0;
    
    public System.Action OnNewNodeArrived;
    private bool bOnNewNodeArrivedInvoked = true;

    private void Awake()
    {
        mRigidBody = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        Time.fixedDeltaTime = 1f / 64f;
        ResetState();
    }

    public override void ResetState()
    {
        base.ResetState();
        bMoving = false;
        bOnNewNodeArrivedInvoked = true;

        Vector3Int cellPos = tileMap.LocalToCell(transform.localPosition);
        Vector3 cellCenterPos = tileMap.GetCellCenterLocal(cellPos);
    
        mCurrCellPos = cellCenterPos;
        mNextCellPos = cellCenterPos;
        mMovingCount = 0;
        mMaxMovingCount = (int)(1.0f / (speedMultiplier * speedDefault * Time.fixedDeltaTime)) - 1;
    }

    private void Update()
    {
    }

    private void FixedUpdate()
    {
        if (bMoving)
        {
            Vector2 curr_position = mRigidBody.position;
            Vector2 translation = speedDefault * speedMultiplier * Time.fixedDeltaTime * mCurrDir;
            Vector2 next_position = curr_position + translation;
            Vector2 next_position_local = transform.parent.InverseTransformPoint(next_position);
            mRigidBody.MovePosition(next_position);

            if ((Vector2.Distance(mCurrCellPos, mNextCellPos) < 0.001f) && mMovingCount < mMaxMovingCount)
            {
                // For matching loop count per each action
                mMovingCount++;
                return;
            }

            // Check distance
            if ((Vector2.Distance(next_position_local, mNextCellPos) < 0.125f) || (Vector2.Distance(mCurrCellPos, mNextCellPos) < 0.001f))
            {
                mRigidBody.MovePosition(transform.parent.TransformPoint(mNextCellPos));
                bMoving = false;
                mMovingCount = 0;
            }
        }
        else
        {
            if (!bOnNewNodeArrivedInvoked)
            {
                OnNewNodeArrived?.Invoke();
                bOnNewNodeArrivedInvoked = true;
            }
        }
    }

    public override void SetDirection(Vector2 direction)
    {
        mCurrDir = direction;
        Vector3Int currCell = tileMap.LocalToCell(transform.parent.InverseTransformPoint(transform.position));
        Vector3Int nextCell = currCell + new Vector3Int((int)direction.x, (int)direction.y, 0);

        if (Occupied(direction))
            nextCell = currCell;

        mCurrCellPos = tileMap.GetCellCenterLocal(currCell);
        mNextCellPos = tileMap.GetCellCenterLocal(nextCell);

        bMoving = true;
        bOnNewNodeArrivedInvoked = false;

        mMaxMovingCount = (int)(1.0f / (speedMultiplier * speedDefault * Time.fixedDeltaTime)) - 1;
    }

    public void SetPosition(Vector2 pos)
    {
        transform.localPosition = pos;
    }

    public void ChangeSpeed(float speed)
    {
        speedMultiplier = speed;
        mMaxMovingCount = (int)(1.0f / (speedMultiplier * speedDefault * Time.fixedDeltaTime)) - 1;
    }
}
