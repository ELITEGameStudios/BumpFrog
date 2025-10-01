using UnityEngine;

public class Enemy : MonoBehaviour
{

    public Transform startTf;
    public TargetPointAI targetAI;
    public Animator animator;


    // Update is called once per frame
    public void Reset()
    {
        transform.position = startTf.position;
    }
}
