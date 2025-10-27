using System.Collections;
using Unity.MLAgents;
using UnityEngine;

public class GhostHome : GhostBehavior
{
    public Transform inside;
    public Transform outside;

    private void OnEnable()
    {
        if (ghost != null && ghost.brain != null)
        {
            ghost.brain.ResetState();
            ghost.brain.SetPositionHomeIn(inside.localPosition);
            ghost.brain.SetDirectionHomeIn();
            ghost.brain.SetBehaviorMode(GhostAgentFinal.BehaviorMode.Home);
        }
    }

    private void OnDisable()
    {
        if (ghost != null && ghost.brain != null)
        {
            ghost.brain.ResetState();
            ghost.brain.SetPositionHomeIn(outside.localPosition);
            ghost.brain.SetDirectionHomeIn();
            ghost.brain.SetBehaviorMode(GhostAgentFinal.BehaviorMode.Chase);
        }
    }
}