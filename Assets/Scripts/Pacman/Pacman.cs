using UnityEngine;

[RequireComponent(typeof(PacmanMovement))]
public class Pacman : MonoBehaviour
{
    public PacmanMovement mMovement { get; private set; }
    private void Awake()
    {
        mMovement = GetComponent<PacmanMovement>();
    }

    private void Update()
    {
        //////////////////////////////////////////////////////////////////////
        /// <TODO> Task 1-3 (1 point)
        /// This function handles the player input to control Pacman's movement and update its rotation.
        /// 
        /// Implementation steps:
        /// 1. Check for input keys (WASD or Arrow keys) and call mMovement.SetDirection()
        ///     - W / UpArrow: Vector2.up
        ///     - S / DownArrow: Vector2.down
        ///     - A / LeftArrow: Vector2.left
        ///     - D / RightArrow: Vector2.right
        /// 2. Update Pacman's rotation based on current movement direction
        ///     - Calculate angle using Mathf.Atan2(y, x)
        ///     - Apply rotation using Quaternion.AngleAixs()
        /// 
        /// Hints:
        ///     - Use Input.GetKeyDown() for responsive input
        ///     - Conver radians to degrees: angle * Mathf.Rad2Deg

        Vector2 inputDir = Vector2.zero;
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            inputDir = Vector2.up;
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            inputDir = Vector2.down;
        }
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            inputDir = Vector2.left;
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            inputDir = Vector2.right;
        }

        if (inputDir != Vector2.zero)
        {
            mMovement.SetDirection(inputDir);
        }


        Vector2 currDir = mMovement.mCurrDir;
        if (currDir != Vector2.zero)
        {
            float angleRad = Mathf.Atan2(currDir.y, currDir.x);
            float angleDeg = angleRad * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angleDeg, Vector3.forward);
        }
        
        //////////////////////////////////////////////////////////////////////
    }

    public void ResetState()
    {
        gameObject.SetActive(true);
        mMovement.ResetState();
    }
}
