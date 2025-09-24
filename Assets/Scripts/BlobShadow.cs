using UnityEngine;

public class BlobShadow : MonoBehaviour
{
    public GameObject shadow;
    public RaycastHit hit;
    public float offset;

    void FixedUpdate()
    {
        // Ray downRay = new Ray(new Vector3(this.transform.position.x, this.transform.position.y - offset, this.transform.position.z), -Vector3.up);

        Vector3 newPos = BallBehavior.instance.GetLandingPosition();
        newPos.y = 0.2f;
        // shadow.transform.position = BallBehavior.instance.GetLandingPosition(); //hitPosition;
        shadow.transform.position = newPos; //hitPosition;
    }
}
