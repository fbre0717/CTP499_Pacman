using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PacmanMovement : MonoBehaviour
{

    [Header("Movement Settings")]
    public float speedDefault = 8.0f;
    public float speedMultiplier = 1.0f;

    public LayerMask obstacleLayer;
    public Rigidbody2D mRigidBody { get; protected set; }

    public Vector3 mStartLocation { get; protected set; }
    public Vector2 mCurrDir { get; protected set; }
    public Vector2 mNextDir { get; protected set; }

    private void Awake()
    {
        mRigidBody = GetComponent<Rigidbody2D>();
        mStartLocation = transform.position;
    }

    private void Start()
    {
        Time.fixedDeltaTime = 1f / 64f;
        ResetState();
    }

    public virtual void ResetState()
    {
        speedMultiplier = 1.0f;
        transform.position = mStartLocation;
        mCurrDir = Vector2.zero;
        mNextDir = Vector2.zero;
        
        mRigidBody.isKinematic = false;
        enabled = true;
    }

    private void Update()
    {
        // Make movement
        if (mNextDir != Vector2.zero)
        {
            SetDirection(mNextDir);
        }
    }
    
    private void FixedUpdate()
    {
        //////////////////////////////////////////////////////////////////////
        /// <TODO> Task 1-1 (1 point)
        /// This function handles Pacman's movement every frame.
        /// 
        /// Implementation steps:
        /// 1. Get current Rigidbody2D position
        /// 2. Calculate translation distance using: speed * multiplier * deltaTime * direction
        /// 3. Move to new position using MovePosition()
        
        Vector2 currentPosition = mRigidBody.position;
        Vector2 translation = speedDefault * speedMultiplier * Time.fixedDeltaTime * mCurrDir;
        Vector2 newPosition = currentPosition + translation;
        mRigidBody.MovePosition(newPosition);

        //////////////////////////////////////////////////////////////////////
    }

    
    public virtual void SetDirection(Vector2 direction)
    {
        //////////////////////////////////////////////////////////////////////
        /// <TODO> Task 1-2 (1 point)
        /// This function sets Pacman's movement direction. It changes the direction immediately if path is clear. Otherwise, it queues the direction for later.
        /// 
        /// Implementation steps:
        /// 1. Check if obstacle exists in the given direction using Occupied()
        /// 2. If no obstacle: set mCurrDir and clear mNextDir with origin
        /// 3. If obstacle exists: store direction in mNextDir queue

        if (!Occupied(direction))
        {
            mCurrDir = direction;
            mNextDir = Vector2.zero;
        }
        else
        {
            mNextDir = direction;
        }

        //////////////////////////////////////////////////////////////////////
    }

    public bool Occupied(Vector2 direction)
    {
        RaycastHit2D hit = Physics2D.BoxCast(transform.position, Vector2.one * 0.5f, 0f, direction, 1.0f, obstacleLayer);
        return hit.collider != null;
    }
}
