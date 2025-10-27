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

        
        //////////////////////////////////////////////////////////////////////
    }

    public void ResetState()
    {
        gameObject.SetActive(true);
        mMovement.ResetState();
    }
}
