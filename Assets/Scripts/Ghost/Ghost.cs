using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class Ghost : MonoBehaviour
{
    public GhostAgentFinal brain { get; private set; }
    public GhostHome home { get; private set; }
    public GhostFrightened frightened { get; private set; }
    

    public enum InitBehaviorType {Chase, Home};
    public InitBehaviorType initBehavior = InitBehaviorType.Chase;

    public int delay = 0;
    public int points = 200;

    private void Awake()
    {
        brain = GetComponent<GhostAgentFinal>();
        home = GetComponent<GhostHome>();
        frightened = GetComponent<GhostFrightened>();
    }

    private void Start()
    {
        ResetState();
    }

    public void ResetState()
    {
        gameObject.SetActive(true);
        brain.ResetPosNState();
        brain.RequestDirection();
        frightened.Disable();

        if (initBehavior == InitBehaviorType.Chase)
        {
            brain.SetBehaviorMode(GhostAgentFinal.BehaviorMode.Chase);
            home.Disable();
        }

        if (initBehavior == InitBehaviorType.Home)
        {
            brain.SetBehaviorMode(GhostAgentFinal.BehaviorMode.Home);
            brain.SetPositionHomeIn(home.inside.localPosition);
            home.Enable(delay);
        }
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Pacman"))
        {
            if (brain.GetBehaviorMode() == GhostAgentFinal.BehaviorMode.Frightened)
            {
                var gameManager = FindObjectOfType<GameManager>();
                if (gameManager != null)
                {
                    gameManager.GhostEaten(this);
                    return;
                }
            }
        }
    }
}
