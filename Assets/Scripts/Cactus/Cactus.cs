using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cactus : MonoBehaviour
{
    private GameManager gameManager;

    private void Awake()
    {
        gameManager = GameObject.FindObjectOfType<GameManager>();
    }

    public void DestroyCactus()
    {
        // Increase score points on GameManager
        gameManager.scorePoints++;

        Destroy(gameObject);
    }
}