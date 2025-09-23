using UnityEngine;

public class TargetPointAI : MonoBehaviour
{
    [SerializeField] bool playerSide;
    [SerializeField] bool secondary;
    public static TargetPointAI playerA, playerB, enemyA, enemyB;

    Vector3 otherPosition, thisPosition, targetPosition;
    Transform targetTf;

    // Constants
    public const float spaceWidth = 5;
    public const float spaceLength = 5;
    public const float spaceOffset = 5;
    public const float flatZ = 0;
    
    float spaceTranslator { get { return playerSide ? 1 : -1; } }


    public TargetPointAI otherAI { 
        get
        {
            if (playerSide){
                return secondary ? playerA : playerB; 
            }
            else{
                return secondary ? enemyA : enemyB; 
            }
        } 
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (playerSide)
        {
            if (secondary) { playerB = this; }
            else { playerA = this; }
        }
        else
        {
            if (secondary) { enemyB = this; }
            else { enemyA = this; }
        }
    }

    Vector3 TranslateToSpace(Vector3 worldPos)
    {
        return new Vector3(
            (worldPos.x - spaceWidth) * spaceTranslator,
            worldPos.y,
            (worldPos.z - spaceLength) * spaceTranslator
        );
    }

    Vector3 TranslateToWorld(Vector3 spacePos)
    {
        return new Vector3(
            spacePos.x/spaceTranslator + spaceWidth,
            spacePos.y,
            spacePos.z/spaceTranslator + spaceLength
        );
    }


    // Update is called once per frame
    void Update()
    {
        otherPosition = TranslateToSpace(otherAI.transform.position);
        thisPosition = TranslateToSpace(transform.position);
        
        if (targetTf == null) ResolveTarget();
        else targetPosition = TranslateToSpace(targetTf.position);
    }

    void ResolveTarget()
    {
        bool targetIsLeftSide = otherPosition.x >= 0;
        float availableSpaceX = Mathf.Abs(otherPosition.x) + spaceWidth;
        float zTarget = (otherPosition.y >= spaceLength / 2) ? spaceLength / 4 : spaceLength * 3 / 4;
        float xTarget = (spaceWidth - (spaceWidth * 2 - (spaceWidth - Mathf.Abs(otherPosition.x)) / 2)) * (targetIsLeftSide ? -1 : 1);

        targetPosition = new Vector3(
            xTarget,
            flatZ,
            zTarget
        );
    }


    
}
