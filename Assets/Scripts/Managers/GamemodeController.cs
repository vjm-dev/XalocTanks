using UnityEngine;

// Controller to manage the selected game mode
public class GamemodeController : MonoBehaviour
{
    public static bool IsSinglePlayer { get; private set; }

    public static void SetGameMode(bool isSinglePlayer)
    {
        IsSinglePlayer = isSinglePlayer;
    }
}