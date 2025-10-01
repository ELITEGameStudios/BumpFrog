using UnityEngine;

public class TargetPointAI : MonoBehaviour
{
    [SerializeField] Rigidbody rb;
    [SerializeField] bool playerSide;
    [SerializeField] bool secondary;
    [SerializeField] bool grounded;
    public static TargetPointAI enemy;

    [SerializeField] Vector3 otherPosition, thisPosition, targetPosition;
    Transform targetTf;

    [SerializeField] Vector3 rawDirectionVector;

    // Constants
    public const float spaceHalfWidth = 7.25f;
    public const float spaceLength = 7.25f;
    public const float spaceOffset = 5;
    public const float flatZ = 0;
    public const float maxAiSpeed = 5;
    public float Kp = 0.8f;

    float spaceTranslator { get { return playerSide ? -1 : 1; } }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        enemy = this;
        // if (playerSide)
        // {
        //     if (secondary) { playerB = this; }
        //     else { playerA = this; }
        // }
        // else
        // {
        //     else { enemyA = this; }
        //     if (secondary) { enemyB = this; }
        // }
    }

    Vector3 TranslateToSpace(Vector3 worldPos)
    {
        return new Vector3(
            worldPos.x * spaceTranslator,
            worldPos.y,
            (worldPos.z) * spaceTranslator
        );
    }

    Vector3 TranslateToWorld(Vector3 spacePos)
    {
        return new Vector3(
            spacePos.x / spaceTranslator,
            spacePos.y,
            spacePos.z / spaceTranslator
        );
    }


    // Update is called once per frame
    void Update()
    {
        // otherPosition = TranslateToSpace(otherAI.transform.position);
        thisPosition = TranslateToSpace(transform.position);

        if (targetTf == null) ResolveTarget();
        else targetPosition = TranslateToSpace(targetTf.position);
    }

    void ResolveTarget()
    {

        if (TranslateToSpace(BallBehavior.instance.GetLandingPosition()).z < 0)// && BallBehavior.instance.ClosestEnemy().targetAI == this))
        {
            bool targetIsLeftSide = otherPosition.x >= 0;
            float availableSpaceX = Mathf.Abs(otherPosition.x) + spaceHalfWidth;
            // float zTarget = (otherPosition.z >= spaceLength / 2) ? spaceLength / 4 : spaceLength * 3 / 4;
            float zTarget = spaceLength / 2;
            float xTarget = 0;
            // float xTarget = (spaceHalfWidth - (spaceHalfWidth * 2 - (Mathf.Abs(otherPosition.x)) / 2)) * (targetIsLeftSide ? 1 : -1) / 2;

            targetPosition = new Vector3(
                xTarget,
                flatZ,
                zTarget
            );
        }
        else
        {
            targetPosition = TranslateToSpace(BallBehavior.instance.GetLandingPosition()) + Vector3.forward *0.5f;
            targetPosition.y = 0;
        }
    }

    void FixedUpdate()
    {
        // rawDirectionVector = playerSide ? thisPosition - targetPosition : targetPosition - thisPosition;
        rawDirectionVector = targetPosition - thisPosition;
        rawDirectionVector = new Vector3(
            rawDirectionVector.x,
            0,
            rawDirectionVector.z
        );


        if (grounded)
        {
            // if (!playerSide)
            // {
                if (Vector3.Distance(BallBehavior.instance.transform.position, transform.position) <= 1f && grounded)
                { Jump(); return; }
            // }

            rb.linearVelocity = rawDirectionVector.normalized * Mathf.Clamp(rawDirectionVector.magnitude * Kp, 0, 5);
        }
    }
    void Jump()
    {
        rb.AddForce(Vector3.up * 5, ForceMode.Impulse);
        // diveTimer = diveTime;
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
    
}
