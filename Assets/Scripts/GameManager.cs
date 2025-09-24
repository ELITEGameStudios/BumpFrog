using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] Player currentPlayer, mainPlayer, secondPlayer;
    [SerializeField] CameraMovement cam;
    public static GameManager instance { get; private set; }
    bool second = false;

    void ChangePlayerTarget(bool second)
    {
        this.second = second;

        currentPlayer.movement.enabled = false;
        currentPlayer.ai.enabled = true;

        currentPlayer = second ? secondPlayer : mainPlayer;

        currentPlayer.movement.enabled = true;
        currentPlayer.ai.enabled = false;

        cam.ChangeTarget(currentPlayer.transform);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            ChangePlayerTarget(!second);
        }
    }

    void Awake()
    {
        if (instance == null) { instance = this; }
        else if (instance != this) { Destroy(this); }

        ChangePlayerTarget(false);
    }
    
    public Player GetCurrentPlayer(){ return currentPlayer; }
}
