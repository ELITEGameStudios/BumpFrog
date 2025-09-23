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

    //player
    public Transform player;
    public PlayerMovement playerMovement;

    //bumping
    private bool bumpable = true;
    private float bumpCooldown = 0.2f;
    
    //spiking
    public float hitRange = 2f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && playerMovement.IsAirborne() && IsCloseToPlayer())
        {
            SpikeBall();
        }
    }

    void FixedUpdate()
    {
        rb.AddForce(Physics.gravity * (ballGravityScale), ForceMode.Acceleration);
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Player") && bumpable)
        {
            BumpBall();
        }
    }

    private void BumpBall()
    {
        Debug.Log("Bump ball");
        
        bumpable = false;
        Invoke(nameof(BumpReset), bumpCooldown);
        
        Vector3 direction = (transform.position - player.position).normalized;
        direction.y = 0.3f;

        rb.linearVelocity = Vector3.zero;
        rb.AddForce(direction * bumpForce + Vector3.up * upwardForce, ForceMode.Impulse);
    }

    private void SpikeBall()
    {
        Debug.Log("Spike ball");
        
        Vector3 direction = (transform.position - player.position).normalized;
        direction.y = -0.25f;
        
        rb.linearVelocity = Vector3.zero;
        rb.AddForce(direction * spikeForce, ForceMode.Impulse);
    }

    private void BumpReset()
    {
        bumpable = true;
    }

    private bool IsCloseToPlayer()
    {
        return Vector3.Distance(transform.position, player.position) < hitRange;
    }
}
