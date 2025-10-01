using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Rigidbody rb;
    public float speed = 5;
    public float jumpSpeed = 5;
    public float diveVelocity, diveTime, diveTimer;
    public bool grounded, canMoveInAir;
    public bool diving { get { return diveTimer > 0; } }
    public bool canMove { get { return (grounded || canMoveInAir) && !diving; } }
    public bool canJump { get { return grounded && !diving; } }

    // public GameObject 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    void Update()
    {
        if (diving){ diveTimer -= Time.deltaTime; }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!diving)
        {
            if (canMove){
                rb.linearVelocity = speed * spaceTo3D(InputManager.instance.GetMovement());
            }

            if(grounded && InputManager.instance.GetJump()){
                Jump();
            }
            
            if (InputManager.instance.GetDive()){
                Dive();
            }
        }

    }
    

    void Dive()
    {
        rb.linearVelocity = 
            spaceTo3D(InputManager.instance.GetMovement().normalized) * diveVelocity
            + Vector3.up * jumpSpeed / 3;
        diveTimer = diveTime;
    }

    void Jump()
    {
        rb.AddForce(Vector3.up * jumpSpeed, ForceMode.Impulse);
    }
    
    Vector3 spaceTo3D(Vector2 pos)
    {
        return new Vector3(
            pos.x,
            0,
            pos.y
        );
    }

    void OnCollisionEnter(Collision collision){
        if (collision.gameObject.name =="Ground" || collision.gameObject.CompareTag("Court")){
            grounded = true;
        }
    }
    
    void OnCollisionExit(Collision collision){
        if (collision.gameObject.name =="Ground" || collision.gameObject.CompareTag("Court")){
            grounded = false;
        }
    }

    public bool IsAirborne()
    {
        return !grounded;
    }
}
