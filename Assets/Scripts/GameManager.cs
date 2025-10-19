using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] Player currentPlayer;
    public Enemy[] enemies;
    [SerializeField] CameraMovement cam;
    public static GameManager instance { get; private set; }
    public bool second = false;
    bool started = false;
    public bool paused = false;


    public int playerPoints, enemyPoints, maxPoints = 7;
    public TMP_Text playerPointText, enemyPointText;
    public Transform ballStartPosition;

    public GameObject winScreen, winText, loseText;
    public GameObject winImage, loseImage;
    public GameState gameState;

    public DialogueStringScript stringScript;

    public void ChangePlayerTarget(bool second)
    {
        this.second = second;

        // if (currentPlayer != null)
        // {
        //     currentPlayer.movement.enabled = false;
        //     currentPlayer.ai.enabled = true;
        // }

        // currentPlayer = second ? secondPlayer : mainPlayer;

        currentPlayer.movement.enabled = true;
        currentPlayer.ai.enabled = false;
        cam.ChangeTarget(currentPlayer.transform);
    }

    public void AwardPoint(bool toPlayer)
    {
        if (toPlayer)
        {
            playerPoints++;
            playerPointText.text = playerPoints.ToString();

            if (playerPoints >= maxPoints)
            {
                PlayWinSequence(true);
                return;
            }

            stringScript.BeginDialogueTree(Dialogue.winTrees[playerPoints-1]);
        }
        else
        {
            enemyPoints++;
            enemyPointText.text = enemyPoints.ToString();

            if (enemyPoints >= maxPoints)
            {
                PlayWinSequence(false);
                return;
            }
            
            stringScript.BeginDialogueTree(Dialogue.loseTrees[enemyPoints-1]);
        }


        PlayRestartSequence();        
    }

    public void PlayGame()
    {
        SceneManager.LoadScene(1);
    }


    void PlayWinSequence(bool players)
    {
        Time.timeScale = 0;
        winScreen.SetActive(true);

        winText.SetActive(players);
        winImage.SetActive(players);

        loseText.SetActive(!players);
        loseImage.SetActive(!players);

        // playerPointText.text = maxPoints.ToString();
        // enemyPointText.text = maxPoints.ToString();
        
        AudioManager.instance.Play(players? "Win Theme" : "Lose Theme");
    }

    void PlayRestartSequence()
    {

        Time.timeScale = 0f;
        started = false;
        ChangePlayerTarget(false);

        currentPlayer.Reset();
        // mainPlayer.Reset();
        // secondPlayer.Reset();
        BallBehavior.instance.transform.position = ballStartPosition.position;
        BallBehavior.instance.rb.linearVelocity = Vector3.zero;
        BallBehavior.instance.bumpable = false;
        foreach (Enemy enemy in enemies) enemy.Reset();
        
    }

    void Update()
    {
        if (InputManager.instance.GetSwitch())
        {
            ChangePlayerTarget(!second);
        }

        if (InputManager.instance.GetStart() && !started && gameState == GameState.PRERALLY)
        {
            Time.timeScale = 1;
            // BallBehavior.instance.BumpBall(BallBehavior.instance.transform.position + Vector3.down, true);   
            BallBehavior.instance.rb.linearVelocity = Vector3.up * 8;
            BallBehavior.instance.bumpable = true;

            AudioManager.instance.Play("Bump 1");
            started = true;
            gameState = GameState.RALLY;
        }
        
        // if(gameState != GameState.RALLY){
        //     BallBehavior.instance.transform.position = ballStartPosition.position;
        //     BallBehavior.instance.bumpable = false;
            
        // }
    }

    void Awake()
    {
        if (instance == null) { instance = this; }
        else if (instance != this) { Destroy(this); }

        PlayRestartSequence();
        gameState = GameState.PRERALLY;
    }

    public Player GetCurrentPlayer() { return currentPlayer; }

    public enum GameState
    {
        DIALOGUE,
        PRERALLY,
        RALLY,
        POSTRALLY
    }
}
