using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteAnimation : MonoBehaviour
{
    public SpriteRenderer mRenderer { get; private set; }

    [Header("Sprite Animations")]
    public Sprite[] sprites;
    [Header("Loop Setting")]
    public bool loop = true;
    
    private float mAnimTimeStep = 0.125f; // Need to change correspond to the game dt
    public int mAnimCurrFrame { get; private set; }

    private void Awake()
    {
        mRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        InvokeRepeating(nameof(StepAnim), 0f, mAnimTimeStep);
    }

    private void StepAnim()
    {
        if (!mRenderer.enabled)
        {
            return;
        }

        mAnimCurrFrame++;

        if (mAnimCurrFrame >= sprites.Length)
        {
            if (loop)
            {
                mAnimCurrFrame = 0; // Loop back to the first frame
            }
            else
            {
                CancelInvoke(nameof(StepAnim)); // Stop animation if not looping
                return;
            }
        }

        if (mAnimCurrFrame >= 0 && mAnimCurrFrame < sprites.Length)
        {
            mRenderer.sprite = sprites[mAnimCurrFrame];
        }
    }

    public void ResetAnim()
    {
        CancelInvoke(nameof(StepAnim)); 
        mAnimCurrFrame = 0;
        InvokeRepeating(nameof(StepAnim), 0f, mAnimTimeStep);
    }
}
