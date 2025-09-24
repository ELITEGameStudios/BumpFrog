using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] Player currentPlayer, mainPlayer, secondPlayer;
    public Enemy[] enemies;
    [SerializeField] CameraMovement cam;
    public static GameManager instance { get; private set; }
    public bool second = false;
    bool started = false;
    public bool paused = false;


    public int playerPoints, enemyPoints, maxPoints = 7;
    public TMP_Text playerPointText, enemyPointText;
    public Transform ballStartPosition;

    public void ChangePlayerTarget(bool second)
    {
        this.second = second;

        if (currentPlayer != null)
        {
            currentPlayer.movement.enabled = false;
            currentPlayer.ai.enabled = true;
        }

        currentPlayer = second ? secondPlayer : mainPlayer;

        currentPlayer.movement.enabled = true;
        currentPlayer.ai.enabled = false;

        cam.ChangeTarget(currentPlayer.transform);
    }

    public void AwardPoint(bool toPlayer)
    {
        if (toPlayer)
        {
            playerPoints++;
            if (playerPoints >= maxPoints)
            {
                PlayWinSequence(true);
                return;
            }
        }
        else
        {
            enemyPoints++;
            if (enemyPoints >= maxPoints)
            {
                PlayWinSequence(false);
                return;
            }
        }

        
        PlayRestartSequence();
        
    }

    void PlayWinSequence(bool players)
    {
        playerPointText.text = maxPoints.ToString();
        enemyPointText.text = maxPoints.ToString();
        
        AudioManager.instance.Play(players? "Win Theme" : "Lose Theme");

    }

    void PlayRestartSequence()
    {
        playerPointText.text = playerPoints.ToString();
        enemyPointText.text = enemyPoints.ToString();

        Time.timeScale = 0f;
        started = false;
        ChangePlayerTarget(false);

        mainPlayer.Reset();
        secondPlayer.Reset();
        BallBehavior.instance.transform.position = ballStartPosition.position;
        BallBehavior.instance.rb.linearVelocity = Vector3.zero;
        foreach (Enemy enemy in enemies) enemy.Reset();
    }

    void Update()
    {
        if (InputManager.instance.GetSwitch())
        {
            ChangePlayerTarget(!second);
        }

        if (InputManager.instance.GetStart() && !started)
        {
            Time.timeScale = 1;
            BallBehavior.instance.BumpBall(BallBehavior.instance.transform.position + Vector3.down, true);   
            started = true;
        }
    }

    void Awake()
    {
        if (instance == null) { instance = this; }
        else if (instance != this) { Destroy(this); }

        PlayRestartSequence();
    }  
    
    public Player GetCurrentPlayer() { return currentPlayer; }
}
