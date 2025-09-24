using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] Transform target, cameraTf;
    [SerializeField] float kp;
    [SerializeField] Vector3 offset;
    
    // Update is called once per frame
    void Update(){
        Vector3 direction = (target.position - BallBehavior.instance.transform.position).normalized;
        Vector3 trueTarget = target.position + (direction * direction.magnitude / 2);

        transform.position = Vector3.Lerp(transform.position, trueTarget, kp) + offset;
    }

    public void ChangeTarget(Transform target){
        this.target = target;
    }
}
