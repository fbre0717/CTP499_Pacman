using Unity.MLAgents.Policies;
using UnityEngine;

public class GhostFrightened : GhostBehavior
{
    public SpriteRenderer body;
    public SpriteRenderer eyes;
    public SpriteRenderer blue;
    public SpriteRenderer white;

    private bool eaten;
    
    public override void Enable(float duration)
    {
        base.Enable(duration);

        body.enabled = false;
        eyes.enabled = false;
        blue.enabled = true;
        white.enabled = false;

        Invoke(nameof(Flash), duration / 2f);
    }

    public override void Disable()
    {
        base.Disable();

        body.enabled = true;
        eyes.enabled = true;
        blue.enabled = false;
        white.enabled = false;
    }

    private void Eaten()
    {
        eaten = true;
        ghost.home.Enable(mDuration);

        body.enabled = false;
        eyes.enabled = true;
        blue.enabled = false;
        white.enabled = false;
    }

    private void Flash()
    {
        if (!eaten)
        {
            blue.enabled = false;
            white.enabled = true;
            white.GetComponent<SpriteAnimation>().ResetAnim();
        }
    }

    private void OnEnable()
    {
        blue.GetComponent<SpriteAnimation>().ResetAnim();
        if (ghost != null && ghost.brain != null)
        {
            ghost.brain.SetSpeed(0.5f);
            ghost.brain.SetBehaviorMode(GhostAgentFinal.BehaviorMode.Frightened);
        }
        
        eaten = false;
    }

    private void OnDisable()
    {
        if (ghost != null && ghost.brain != null && (ghost.brain.GetBehaviorMode() != GhostAgentFinal.BehaviorMode.Home))
        {
            ghost.brain.SetSpeed(1.0f);
            ghost.brain.SetBehaviorMode(GhostAgentFinal.BehaviorMode.Chase);
        }

        eaten = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Pacman"))
        {
            if (enabled) {
                Eaten();
            }
        }
    }

}