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
    public Vector3 landingPos;
    public Vector3 linearVelocity;

    //bumping
    private bool bumpable = true;
    private float bumpCooldown = 0.2f;
    
    //spiking
    public const float hitRange = 2f;
    public const float groundPos = 1f;
    public static BallBehavior instance { get; private set; }

    AudioManager audioManager;
    
    void Awake()
    {
        if (instance == null) { instance = this; }
        else if(instance != this) { Destroy(gameObject); }

        audioManager = FindObjectOfType<AudioManager>();
    }
    void Start()
    {
        BumpBall(Vector3.down);   
    }

    void Update()
    {
        landingPos = GetLandingPosition();
    }

    void FixedUpdate()
    {
        rb.AddForce(Physics.gravity * (ballGravityScale), ForceMode.Acceleration);
        linearVelocity = rb.linearVelocity;
    }

    
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Player") && bumpable)
        {
            // Vector3 direction = collision.transform.position.normalized - collision.GetContact(0).point;
            BumpBall(collision.transform.position);
        }
        
        if(collision.collider.CompareTag("Ground"))
            audioManager.Play("Hits Ground");
        
        if(collision.collider.CompareTag("Net"))
            audioManager.Play("Hits Net");
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

    private void BumpBall(Vector3 playerTf)
    {
        Debug.Log("Bump ball");

        bumpable = false;
        Invoke(nameof(BumpReset), bumpCooldown);

        Vector3 direction = (transform.position - playerTf).normalized;
        direction.y = 0.3f;

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

    public void SpikeBall(Transform playerTf)
    {
        Debug.Log("Spike ball");
        
        Vector3 direction = (transform.position - playerTf.position).normalized;
        direction.y = -0.25f;
        
        rb.linearVelocity = Vector3.zero;
        rb.AddForce(direction * spikeForce, ForceMode.Impulse);
        
        audioManager.Play("Spike Whiff");
    }

    private void BumpReset()
    {
        bumpable = true;
    }


}
