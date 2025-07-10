using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cactus : MonoBehaviour
{
    private GameManager gameManager;

    private void Awake()
    {
        gameManager = FindFirstObjectByType<GameManager>();
    }

    public void DestroyCactus()
    {
        // Increase score points on GameManager
        gameManager.scorePoints++;

        Destroy(gameObject);
    }
}