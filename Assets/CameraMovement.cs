using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] Transform target, cameraTf;
    [SerializeField] float kp;
    [SerializeField] Vector3 offset;
    
    // Update is called once per frame
    void Update(){
        transform.position = Vector3.Lerp(transform.position, target.position, kp) + offset;
    }

    public void ChangeTarget(Transform target){
        this.target = target;
    }
}
