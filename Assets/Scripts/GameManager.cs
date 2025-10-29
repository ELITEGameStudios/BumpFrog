using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] Player currentPlayer;
    public Enemy[] enemies;
    [SerializeField] CameraMovement cam;
    public static GameManager instance { get; private set; }
    public bool second = false;
    public bool paused = false;
    public bool maxLoveScore {get{ return loveScore >= targetLoveScore; }}


    public int playerPoints, enemyPoints, maxPoints = 7, loveScore, targetLoveScore, nedInsultScore, targetNedInsultScore, netPoints, lovePointsThisTree;
    public TMP_Text playerPointText, enemyPointText;
    public Transform ballStartPosition;

    public GameObject winScreen, winText, loseText;
    public GameObject winImage, loseImage;
    public GameState gameState;
    public Slider loveSlider, rageSlider;

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

    public void GiveLoveScore(int loveScore)
    {
        this.loveScore += loveScore;
        // if (maxLoveScore) { this.loveScore = targetLoveScore; }

        loveSlider.value = this.loveScore;
        loveSlider.maxValue = targetLoveScore;
        lovePointsThisTree += loveScore;
    }
    
    public void InsultNed()
    {
        nedInsultScore++;
        rageSlider.value = nedInsultScore;
        rageSlider.maxValue = targetNedInsultScore;

        if(nedInsultScore >= targetNedInsultScore)
        {
            CinematicSystem.instance.BeginNedSequence();
        } 
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

    public void GoToNextLevel()
    {
        SceneManager.LoadScene(2);
    }


    void PlayWinSequence(bool players)
    {
        bool wonGame = ((loveScore) + (players ? 75 : 50)) >= 100;
        Time.timeScale = 0;

        if (wonGame) { CinematicSystem.instance.BeginWinSequence(); }
        else { CinematicSystem.instance.BeginLoseSequence(); }
        
        // winScreen.SetActive(true);

        // winText.SetActive(wonGame);
        // winImage.SetActive(wonGame);

        // loseText.SetActive(!wonGame);
        // loseImage.SetActive(!wonGame);

        // playerPointText.text = maxPoints.ToString();
        // enemyPointText.text = maxPoints.ToString();
        
        // AudioManager.instance.Play(players? "Win Theme" : "Lose Theme");
    }

    void PlayRestartSequence()
    {
        Time.timeScale = 1f;
        if(gameState == GameState.RALLY) { gameState = GameState.PRERALLY; };
        ChangePlayerTarget(false);

        currentPlayer.Reset();
        // mainPlayer.Reset();
        // secondPlayer.Reset();
        BallBehavior.instance.transform.position = ballStartPosition.position;
        BallBehavior.instance.rb.linearVelocity = Vector3.zero;
        BallBehavior.instance.bumpable = false;
        foreach (Enemy enemy in enemies) enemy.Reset();
        
        lovePointsThisTree = 0;
    }

    void Update()
    {
        if (InputManager.instance.GetSwitch())
        {
            ChangePlayerTarget(!second);
        }

        if (gameState != GameState.RALLY)
        {
            BallBehavior.instance.rb.linearVelocity = Vector3.zero;
            BallBehavior.instance.transform.position = ballStartPosition.position;
            BallBehavior.instance.bumpable = false;

        }
        
        if (InputManager.instance.GetStart() && gameState == GameState.PRERALLY && !CinematicSystem.instance.inProgress)
        {
            gameState = GameState.RALLY;

            Time.timeScale = 1;
            // BallBehavior.instance.BumpBall(BallBehavior.instance.transform.position + Vector3.down, true);   
            BallBehavior.instance.transform.position = ballStartPosition.position;
            BallBehavior.instance.rb.linearVelocity = Vector3.up * 8;
            BallBehavior.instance.bumpable = true;

            AudioManager.instance.Play("Bump 1");
            
            return;
        }
        
    }

    void Awake()
    {
        if (instance == null) { instance = this; }
        else if (instance != this) { Destroy(this); }

        PlayRestartSequence();
        CinematicSystem.instance.BeginSequence();
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
