using UnityEngine;

public class Player : MonoBehaviour
{
    public PlayerMovement movement;
    public Transform startTf;
    public TargetPointAI ai;
    [SerializeField] bool ballCloseToPlayer;


    public void Update()
    {
        // if (Input.GetKeyDown(KeyCode.E) && !movement.grounded && IsCloseToPlayer())
        // {
        //     BallBehavior.instance.SpikeBall(transform);
        // }
    }

    private bool IsCloseToPlayer()
    {
        ballCloseToPlayer = Vector3.Distance(transform.position, BallBehavior.instance.transform.position) < BallBehavior.hitRange;
        return ballCloseToPlayer;
    }

    public void Reset()
    {
        transform.position = startTf.position;
    }
}
