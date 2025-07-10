using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public int m_NumRoundsToWin = 3;            // The number of rounds a single player has to win to win the game.
    public float m_StartDelay = 3f;             // The delay between the start of RoundStarting and RoundPlaying phases.
    public float m_EndDelay = 3f;               // The delay between the end of RoundPlaying and RoundEnding phases.
    public CameraControl m_CameraControl;       // Reference to the CameraControl script for control during different phases.
    public Text m_MessageText;                  // Reference to the overlay Text to display winning text, etc.
    public GameObject m_TankPrefab;             // Reference to the prefab the players will control.
    public TankManager[] m_Tanks;               // A collection of managers for enabling and disabling different aspects of the tanks.
    private int numTanks;                       // Used to manage the number of tanks in the game

    private int m_RoundNumber;                  // Which round the game is currently on.
    private WaitForSeconds m_StartWait;         // Used to have a delay whilst the round starts.
    private WaitForSeconds m_EndWait;           // Used to have a delay whilst the round or game ends.
    private TankManager m_RoundWinner;          // Reference to the winner of the current round.  Used to make an announcement of who won.
    private TankManager m_GameWinner;           // Reference to the winner of the game.  Used to make an announcement of who won.

    public GameObject[] itemPrefab;             // Item prefab
    public ItemManager[] m_Items;               // Items (on Inspector these are treated as SpawnPoints)

    public Text textAmmoComponent;              // Show how much ammo they have
    public Text textScorePointsComponent;       // Show how many scorepoints/won rounds they have
    public Text textTimeComponent;              // Show elapsed time

    private float startTime;                    // Time since game started
    private bool isPaused;                      // Check to pause
    private string timeResult;                  // Result of elapsed time

    public int maxCactuses = 5;                 // Maximum number of cactuses

    [HideInInspector] public int scorePoints;   // Score points of the destroyed cactuses
    public int maxScorePoints = 20;             // Maximum score points of the destroyed cactuses

    public GameObject cactusPrefab;             // Cactus prefab
    private GameObject[] cactus;                // Cactus being spawned on some place

    private void Start()
    {
        // Initialize the score points
        scorePoints = 0;

        // If the game mode is single player mode, only spawn one tank
        numTanks = (GamemodeController.IsSinglePlayer) ? 1 : m_Tanks.Length;

        // Set up the texts and their sizes
        textAmmoComponent.text = "";
        textScorePointsComponent.text = "";
        textTimeComponent.text = "";

        textAmmoComponent.fontSize = 16;
        textScorePointsComponent.fontSize = 16;
        textTimeComponent.fontSize = 19;

        // Create the delays so they only have to be made once.
        m_StartWait = new WaitForSeconds(m_StartDelay);
        m_EndWait = new WaitForSeconds(m_EndDelay);

        SpawnItems();

        // Checks if game mode is single player mode or two player mode
        if (GamemodeController.IsSinglePlayer)
        {
            // Don't start the time yet
            isPaused = true;

            cactus = new GameObject[maxCactuses];
            // Initialize other game elements

            SpawnTanks();

            // Once the tanks have been created and the camera is using them as targets, start the game.
            StartCoroutine(GameLoop());

            // Start cactus generation
            StartCoroutine(GenerateCactus());
        }
        else
        {
            // Set up two players mode gameplay
            SpawnTanks();

            // Once the tanks have been created and the camera is using them as targets, start the game.
            StartCoroutine(GameLoop());
        }
        SetCameraTargets();
    }

    private void Update()
    {
        if (GamemodeController.IsSinglePlayer)
            DisplayTime();

        // Update the ammo text to show how much ammo they have
        DisplayAmmo();

        if (!GamemodeController.IsSinglePlayer)
        {
            // Update how many won rounds they have
            DisplayWonRounds();
        }
        else
        {
            // Update how many points obtained destroying cactuses
            DisplayScorePoints();
        }
    }


    private void DisplayAmmo()
    {
        string ammoPlayers = (!GamemodeController.IsSinglePlayer)
                            ? ""
                            : "AMMO: " + m_Tanks[0].m_Shooting.ammoCount.ToString();

        // Display every player how much ammo they have
        if (!GamemodeController.IsSinglePlayer)
            for (int i = 0; i < numTanks; i++)
                ammoPlayers += "Player " + (i + 1) + " - AMMO: " + m_Tanks[i].m_Shooting.ammoCount.ToString() + "\n";

        textAmmoComponent.text = ammoPlayers;
    }


    private void DisplayWonRounds()
    {
        string wonRoundPlayers = "";
        // Display every player how much ammo they have
        for (int i = 0; i < numTanks; i++)
            wonRoundPlayers += "Player " + (i + 1) + " - WINS: " + m_Tanks[i].m_Wins.ToString() + "\n";

        textScorePointsComponent.text = wonRoundPlayers;
    }


    private void DisplayScorePoints()
    {
        string scorePointstxt = "";
        scorePointstxt += "SCORE: " + scorePoints.ToString() + "\n";

        textScorePointsComponent.text = scorePointstxt;
    }


    private void DisplayTime()
    {
        // If it's paused, don't show the elapsed time
        if (isPaused)
        {
            textTimeComponent.enabled = false;
            return;
        }

        textTimeComponent.enabled = true;

        // Calculate the elapsed time since the start of the game
        float elapsedTime = Time.time - startTime;

        // Convert the elapsed time to a readable format (minutes:seconds)
        string formattedTime = string.Format("{0:00}:{1:00}", Mathf.FloorToInt(elapsedTime / 60), elapsedTime % 60);

        textTimeComponent.text = formattedTime;
        timeResult = formattedTime;
    }

    // CACTUS MANAGEMENT METHODS

    // To count how many active cactuses are available
    int CountActiveCactus()
    {
        int count = 0;

        for (int i = 0; i < cactus.Length; i++)
            if (cactus[i] != null)
                count++;

        return count;
    }


    private Vector3 GetRandomSpawnPosition()
    {
        // current terrain distances
        float minX = -50f;
        float maxX = 20f;
        float minZ = -35f;
        float maxZ = 35f;
        float y = 0;

        Vector3 randomPosition = Vector3.zero;
        int solidObjectLayer = LayerMask.NameToLayer("Solid");
        bool validPosition = false;
        int maxAttempts = 10;
        int currentAttempts = 0;

        while (!validPosition && currentAttempts < maxAttempts)
        {
            float randomX = Random.Range(minX, maxX);
            float randomZ = Random.Range(minZ, maxZ);

            randomPosition = new Vector3(randomX, y, randomZ);

            // Check if the generated position is near other objects
            Collider[] colliders = Physics.OverlapSphere(randomPosition, 10f);

            bool positionOccupied = false;

            foreach (Collider collider in colliders)
            {
                // Ignore the collider of the cactus itself and other objects that we don't want to consider
                if (collider.gameObject == gameObject || collider.gameObject.CompareTag("Player"))
                    continue;

                // Check for collision with a solid object
                if (collider.gameObject.layer == solidObjectLayer)
                {
                    positionOccupied = true;
                    break;
                }
            }

            if (!positionOccupied)
                validPosition = true;
            else
                currentAttempts++;
        }

        if (!validPosition)
        {
            // If a valid position is not found after several attempts, use a default position or take some other action
            randomPosition = new Vector3(0, y, 0);

            return randomPosition;
        }

        return randomPosition;
    }


    private IEnumerator GenerateCactus()
    {
        // End reaching until maximum score points
        while (scorePoints < maxScorePoints)
        {
            // Generate new cactus if the limit hasn't been reached
            if (CountActiveCactus() < maxCactuses)
                SpawnCactus();

            yield return new WaitForSeconds(Random.Range(1f, 3f));
        }

        // Stop cactus generation
        StopCoroutine(GenerateCactus());
    }


    private void SpawnCactus()
    {
        // Generate random position for the cactus
        Vector3 spawnPosition = GetRandomSpawnPosition();

        // Instantiate the cactus prefab at the generated position
        GameObject newCactus = Instantiate(cactusPrefab, spawnPosition, Quaternion.identity);

        // Add a reference to the generated cactus to the array
        for (int i = 0; i < cactus.Length; i++)
        {
            if (cactus[i] == null)
            {
                cactus[i] = newCactus;
                break;
            }
        }
    }

    // ITEMS MANAGEMENT METHOD
    private void SpawnItems()
    {
        for (int i = 0; i < m_Items.Length; i++)
        {
            int randomItem = Random.Range(0, 3);

            m_Items[i].m_Instance =
                Instantiate(itemPrefab[randomItem], m_Items[i].m_SpawnPoint.position, m_Items[i].m_SpawnPoint.rotation) as GameObject;

            m_Items[i].Setup();
        }
    }


    private void SpawnTanks()
    {
        // For all the tanks...
        for (int i = 0; i < numTanks; i++)
        {
            // ... create them, set their player number and references needed for control.
            m_Tanks[i].m_Instance =
                Instantiate(m_TankPrefab, m_Tanks[i].m_SpawnPoint.position, m_Tanks[i].m_SpawnPoint.rotation) as GameObject;
            m_Tanks[i].m_PlayerNumber = i + 1;
            m_Tanks[i].Setup();
        }
    }


    private void SetCameraTargets()
    {
        // Create a collection of transforms the same size as the number of tanks.
        Transform[] targets = (!GamemodeController.IsSinglePlayer)
                                ? new Transform[m_Tanks.Length]
                                : new Transform[1];

        // For each of these transforms...
        for (int i = 0; i < targets.Length; i++)
        {
            // ... set it to the appropriate tank transform.
            targets[i] = m_Tanks[i].m_Instance.transform;
        }

        // These are the targets the camera should follow.
        m_CameraControl.m_Targets = targets;
    }


    // This is called from start and will run each phase of the game one after another.
    private IEnumerator GameLoop()
    {
        // Start off by running the 'RoundStarting' coroutine but don't return until it's finished.
        yield return StartCoroutine(RoundStarting());

        // Once the 'RoundStarting' coroutine is finished, run the 'RoundPlaying' coroutine but don't return until it's finished.
        yield return StartCoroutine(RoundPlaying());

        // Once execution has returned here, run the 'RoundEnding' coroutine, again don't return until it's finished.
        yield return (GamemodeController.IsSinglePlayer) 
                    ? StartCoroutine(SinglePlayerModeEnding())
                    : StartCoroutine(RoundEnding()); 

        // This code is not run until 'SinglePlayerModeEnding' or 'RoundEnding' has finished.  At which point, check if a game winner has been found.
        if (m_GameWinner != null || scorePoints >= maxScorePoints)
        {
            // If there is a game winner or already obtained maximum score points destroying cactuses, go to main menu.
            SceneManager.LoadScene("MainMenu");
        }
        else
        {
            // If there isn't a winner yet, restart this coroutine so the loop continues.
            // Note that this coroutine doesn't yield.  This means that the current version of the GameLoop will end.
            StartCoroutine(GameLoop());
        }
    }


    private IEnumerator SinglePlayerModeEnding()
    {
        // Stop tanks from moving.
        DisableTankControl();
        
        // Pause and save the elapsed time result
        isPaused = true;

        // Get the message about the result of the time
        m_MessageText.text = "YOUR TIME RECORD: " + timeResult;

        // Wait for the specified length of time until yielding control back to the game loop.
        yield return m_StartWait;
    }


    private IEnumerator RoundStarting()
    {
        // As soon as the round starts reset the tanks and make sure they can't move.
        ResetAllTanks();
        DisableTankControl();

        // Snap the camera's zoom and position to something appropriate for the reset tanks.
        m_CameraControl.SetStartPositionAndSize();

        if (GamemodeController.IsSinglePlayer)
            m_MessageText.text = "DESTROY ALL CACTUSES AS FAST AS POSSIBLE!";
        else
        {
            // Increment the round number and display text showing the players what round it is.
            m_RoundNumber++;
            m_MessageText.text = "ROUND " + m_RoundNumber;
        }

        // Wait for the specified length of time until yielding control back to the game loop.
        yield return m_StartWait;

        // Save the start time and unpause the time
        startTime = Time.time;
        isPaused = false;
    }


    private IEnumerator RoundPlaying()
    {
        // As soon as the round begins playing let the players control the tanks.
        EnableTankControl();

        // Clear the text from the screen.
        m_MessageText.text = string.Empty;

        // While there is not one tank left...
        if (!GamemodeController.IsSinglePlayer)
            while (!OneTankLeft())
            {
                // ... return on the next frame.
                yield return null;
            }
        else
            while (scorePoints < maxScorePoints)
                yield return null;
    }


    private IEnumerator RoundEnding()
    {
        // Stop tanks from moving.
        DisableTankControl();

        // Clear the winner from the previous round.
        m_RoundWinner = null;

        // See if there is a winner now the round is over.
        m_RoundWinner = GetRoundWinner();

        // If there is a winner, increment their score.
        if (m_RoundWinner != null)
            m_RoundWinner.m_Wins++;

        // Now the winner's score has been incremented, see if someone has one the game.
        m_GameWinner = GetGameWinner();

        // Get a message based on the scores and whether or not there is a game winner and display it.
        string message = EndMessage();
        m_MessageText.text = message;

        // Wait for the specified length of time until yielding control back to the game loop.
        yield return m_EndWait;
    }


    // This is used to check if there is one or fewer tanks remaining and thus the round should end.
    private bool OneTankLeft()
    {
        // Start the count of tanks left at zero.
        int numTanksLeft = 0;

        // Go through all the tanks...
        for (int i = 0; i < numTanks; i++)
        {
            // ... and if they are active, increment the counter.
            if (m_Tanks[i].m_Instance.activeSelf)
                numTanksLeft++;
        }

        // If there are one or fewer tanks remaining return true, otherwise return false.
        return numTanksLeft <= 1;
    }


    // This function is to find out if there is a winner of the round.
    // This function is called with the assumption that 1 or fewer tanks are currently active.
    private TankManager GetRoundWinner()
    {
        // Go through all the tanks...
        for (int i = 0; i < numTanks; i++)
        {
            // ... and if one of them is active, it is the winner so return it.
            if (m_Tanks[i].m_Instance.activeSelf)
                return m_Tanks[i];
        }

        // If none of the tanks are active it is a draw so return null.
        return null;
    }


    // This function is to find out if there is a winner of the game.
    private TankManager GetGameWinner()
    {
        // Go through all the tanks...
        for (int i = 0; i < numTanks; i++)
        {
            // ... and if one of them has enough rounds to win the game, return it.
            if (m_Tanks[i].m_Wins == m_NumRoundsToWin)
                return m_Tanks[i];
        }

        // If no tanks have enough rounds to win, return null.
        return null;
    }


    // Returns a string message to display at the end of each round.
    private string EndMessage()
    {
        // By default when a round ends there are no winners so the default end message is a draw.
        string message = "DRAW!";

        // If there is a winner then change the message to reflect that.
        if (m_RoundWinner != null)
            message = m_RoundWinner.m_ColoredPlayerText + " WINS THE ROUND!";

        // Add some line breaks after the initial message.
        message += "\n\n\n\n";

        // Go through all the tanks and add each of their scores to the message.
        for (int i = 0; i < numTanks; i++)
            message += m_Tanks[i].m_ColoredPlayerText + ": " + m_Tanks[i].m_Wins + " WINS\n";

        // If there is a game winner, change the entire message to reflect that.
        if (m_GameWinner != null)
            message = m_GameWinner.m_ColoredPlayerText + " WINS THE GAME!";

        return message;
    }


    // This function is used to turn all the tanks back on and reset their positions and properties.
    private void ResetAllTanks()
    {
        for (int i = 0; i < numTanks; i++)
            m_Tanks[i].Reset();
    }


    private void EnableTankControl()
    {
        for (int i = 0; i < numTanks; i++)
            m_Tanks[i].EnableControl();
    }


    private void DisableTankControl()
    {
        for (int i = 0; i < numTanks; i++)
            m_Tanks[i].DisableControl();
    }
}