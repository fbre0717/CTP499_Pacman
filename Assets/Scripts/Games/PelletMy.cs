using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PelletMy : MonoBehaviour
{
    public int points = 10;
    private PacmanAgentPellet agent;

    private void Start()
    {
        agent = FindObjectOfType<PacmanAgentPellet>();
    }

    private void Eat()
    {
        agent?.OnPelletEaten(this);
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Pacman"))
        {
            Eat();
        }
    }
}
