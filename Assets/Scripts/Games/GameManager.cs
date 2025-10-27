using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Ghost[] ghosts;
    public Pacman pacman;
    public Transform pellets;
    public GameObject gameCompletedCanvas;
    public GameObject gameOverCanvas;

    public int score { get; private set; }
    [SerializeField] private TextMeshProUGUI scoreText;

    public int lives { get; private set; } = 3;
    public Transform liveIcon;
    private List<Transform> mLiveIcons;

    private void Start()
    {
        gameCompletedCanvas.SetActive(false);
        gameOverCanvas.SetActive(false);
        mLiveIcons = new List<Transform>();
        
        foreach (Transform icon in liveIcon)
        {
            icon.gameObject.SetActive(true);
            mLiveIcons.Add(icon);
            
        }

        Time.timeScale = 1;

        NewRound();
    }

    private void Update()
    {
        UpdateScoreText();

        if (lives <= 0)
        {
            GameOver();
        }
    }

    private void NewRound()
    {
        foreach (Transform pellet in pellets)
        {
            pellet.gameObject.SetActive(true);
        }

        ResetState();
    }

    private void ResetState()
    {
        gameCompletedCanvas.SetActive(false);
        gameOverCanvas.SetActive(false);
        
        foreach (Ghost ghost in ghosts)
        {
            ghost.ResetState();
        }

        pacman.ResetState();
    }

    private void GameOver()
    {
        gameOverCanvas.SetActive(true);
        foreach (Ghost ghost in ghosts)
        {
            ghost.gameObject.SetActive(false);
        }

        pacman.gameObject.SetActive(false);

        Time.timeScale = 0;
    }

    private void SetScore(int score)
    {
        this.score = score;
    }

    public void GhostEaten(Ghost ghost)
    {
        SetScore(score + ghost.points);
    }

    public void PacmanEaten()
    {
        pacman.gameObject.SetActive(false);
        SetLives(lives - 1);
        
        if (lives > 0)
        {
            Invoke(nameof(ResetState), 2.0f);
        }
        else
        {
            GameOver();
        }
    }

    public void PelletEaten(Pellet pellet)
    {
        pellet.gameObject.SetActive(false);
        SetScore(score + pellet.points);

        if (!HasRemainingPellets())
        {
            GameCompleted();
        }
    }

    public void PowerPelletEaten(PowerPellet pellet)
    {

        foreach (Ghost ghost in ghosts)
        {
            if (ghost.brain.GetBehaviorMode() != GhostAgentFinal.BehaviorMode.Home)
            {
                ghost.frightened.Enable(pellet.duration);                
            }
        }
        PelletEaten(pellet);
    }

    private bool HasRemainingPellets()
    {
        foreach (Transform pellet in pellets)
        {
            if (pellet.gameObject.activeSelf)
            {
                return true;
            }
        }

        return false;
    }

    private void GameCompleted()
    {
        if (gameCompletedCanvas != null)
        {
            gameCompletedCanvas.SetActive(true);
        }

        Time.timeScale = 0;
    }

    private void SetLives(int lives)
    {
        this.lives = lives;
        mLiveIcons[lives].gameObject.SetActive(false);
    }

    private void UpdateScoreText()
    {
        scoreText.text = $"\n   Score: {score}";
    }
}
