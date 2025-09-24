using System.Numerics;
using System.Runtime.CompilerServices;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class BallBehavior : MonoBehaviour
{
    //ball physics
    public Rigidbody rb;
    public float bumpForce = 8f;
    public float spikeForce = 12f;
    public float upwardForce = 5f;
    public float ballGravityScale = 0.5f; //normal = 1   floaty < 1   bouncy > 1
    public Transform LandingPosTracker;
    public Enemy closestEnemy;
    public Vector3 landingPos;
    public Vector3 linearVelocity;
    public bool playerHasPosession;
    public int timesHit;

    //bumping
    private bool bumpable = true;
    private float bumpCooldown = 0.2f;
    
    //spiking
    public const float hitRange = 2f;
    public const float groundPos = 1f;
    public static BallBehavior instance { get; private set; }
    [SerializeField] GameObject spikeParticles;

    AudioManager audioManager;
    
    void Awake()
    {
        if (instance == null) { instance = this; }
        else if(instance != this) { Destroy(gameObject); }

        audioManager = FindObjectOfType<AudioManager>();
    }

    void Update()
    {
        landingPos = GetLandingPosition();
    }

    void FixedUpdate()
    {
        if (GameManager.instance.paused) return;

        rb.AddForce(Physics.gravity * (ballGravityScale), ForceMode.Acceleration);
        linearVelocity = rb.linearVelocity;
    }

    
    private void OnCollisionEnter(Collision collision)
    {
        // Hitting Player
        if (collision.collider.CompareTag("Player") && bumpable)
        {
            // Possesion Switch
            if (!playerHasPosession) { playerHasPosession = true; timesHit = 1; }
            else { timesHit++; if (timesHit > 3) { GameManager.instance.AwardPoint(false); } }

            // Vector3 direction = collision.transform.position.normalized - collision.GetContact(0).point;
            if (collision.collider.gameObject.GetComponent<PlayerMovement>().diving)
            {
                SpikeBall(collision.transform, true);
            }
            else
            {
                BumpBall(collision.transform.position, true);
            }


            GameManager.instance.ChangePlayerTarget(!GameManager.instance.second);
        }
        // Hitting Enemy
        else if (collision.collider.CompareTag("Enemy") && bumpable)
        {

            // Possesion Switch
            if (playerHasPosession) { playerHasPosession = false; timesHit = 1; }
            else { timesHit++; if (timesHit > 3) { GameManager.instance.AwardPoint(true); } }


            if (timesHit >= 2 && !playerHasPosession)
            {
                Debug.Log("EnemySpike");
                SpikeBall(collision.transform, false);
            }
            else
            {
                BumpBall(collision.transform.position, false);
            }


        }


        // Hitting any obstacle
        else if (collision.collider.CompareTag("Ground") || collision.collider.CompareTag("Court") || collision.collider.CompareTag("Net"))
        {
            if (collision.collider.CompareTag("Court"))
            {
                GameManager.instance.AwardPoint(transform.position.z > 0);
            }
            else
            {
                GameManager.instance.AwardPoint(!playerHasPosession);
            }
        }
        
        if(collision.collider.CompareTag("Ground"))
            audioManager.Play("Hits Ground");
        
        if(collision.collider.CompareTag("Net"))
            audioManager.Play("Hits Net");
    }

    void DisableParticles()
    {
        spikeParticles.SetActive(false);
    }

    public Vector3 GetLandingPosition()
    {
        Vector3 startPos = transform.position;
        Vector3 simulatedPos = transform.position;
        Vector3 startVelocity = rb.linearVelocity;
        float interval = 0.01f;

        for (int i = 0; simulatedPos.y > groundPos; i++)
        {
            float gap = interval * i;

            Vector3 simVelocity = (startVelocity + Physics.gravity * ballGravityScale * gap) * gap;
            simulatedPos = startPos + simVelocity;
        }
        LandingPosTracker.position = simulatedPos;
        return simulatedPos;
    }

    public Enemy ClosestEnemy()
    {
        Enemy closestCandadite = GameManager.instance.enemies[0];
        float closestDistance = Vector3.Distance(transform.position, closestCandadite.transform.position);

        for (int i = 0; i < GameManager.instance.enemies.Length; i++)
        {
            Enemy candadite = GameManager.instance.enemies[i];
            float distance = Vector3.Distance(transform.position, candadite.transform.position);
            if (distance < closestDistance)
            {
                closestCandadite = candadite;
                closestDistance = distance;
            }
        }

        closestEnemy = closestCandadite;

        return closestCandadite;
    }

    public void BumpBall(Vector3 playerTf, bool byPlayer)
    {
        Debug.Log("Bump ball");
        if (spikeParticles.activeInHierarchy) spikeParticles.SetActive(false);

        bumpable = false;
        Invoke(nameof(BumpReset), bumpCooldown);

        Vector3 direction = (transform.position - playerTf).normalized;
        if (direction.y < 0.5f){
            direction.y = 0.5f;
        }
        direction.z = Mathf.Abs(direction.z) * (byPlayer ? 1 : -1);
        direction.Normalize();

        if (!byPlayer)
        {
            float maxAngle = 30;
            float margin = Mathf.Cos(maxAngle * Mathf.Deg2Rad);
            direction.x = Mathf.Clamp(direction.x, -margin, margin);
        }

        rb.linearVelocity = Vector3.zero;
        rb.AddForce(direction * bumpForce + Vector3.up * upwardForce, ForceMode.Impulse);
        
        int random = Random.Range(0, 3);
        if(random == 0)
        {
            audioManager.Play("Bump 1");
        }
        else if(random == 1)
        {
            audioManager.Play("Bump 2");
        }
        else
        {
            audioManager.Play("Bump 3");
        }
    }

    public void SpikeBall(Transform playerTf, bool byPlayer)
    {
        Debug.Log("Spike ball");
        
        Vector3 direction = (transform.position - playerTf.position).normalized;
        direction.y = byPlayer ? -0.25f : 0f;
        direction.z = Mathf.Abs(direction.z) * (byPlayer ? 1 : -1);
        direction.Normalize();

        float maxAngle = 30;
        float margin = Mathf.Cos(maxAngle * Mathf.Deg2Rad);
        direction.x = byPlayer ? Mathf.Clamp(direction.x, -margin, margin) : 0;
        Debug.Log(direction.x); // dont need to normalize since this remains within the bounds
        direction.Normalize();

        rb.linearVelocity = Vector3.zero;
        rb.AddForce(direction * spikeForce, ForceMode.Impulse);

        spikeParticles.SetActive(true);
        
        audioManager.Play("Spike Whiff");
    }

    private void BumpReset()
    {
        bumpable = true;
    }


}
