
using UnityEngine;

public class GhostEyes : MonoBehaviour
{
    public GhostMovement mMovement { get; private set; }
    public SpriteRenderer mSpriteRenderer { get; private set; }

    [Header("Sprite Animations")]
    public Sprite up;
    public Sprite down;
    public Sprite right;
    public Sprite left;


    private void Awake()
    {
        mSpriteRenderer = GetComponent<SpriteRenderer>();
        mMovement = GetComponentInParent<GhostMovement>();
    }
    private void Update()
    {
        if (mMovement.mCurrDir == Vector2.up){
            mSpriteRenderer.sprite = up;
        }
        else if (mMovement.mCurrDir == Vector2.down){
            mSpriteRenderer.sprite = down;
        }
        else if (mMovement.mCurrDir == Vector2.right){
            mSpriteRenderer.sprite = right;
        }
        else if (mMovement.mCurrDir == Vector2.left){
            mSpriteRenderer.sprite = left;
        }
    }
}

