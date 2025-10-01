using UnityEngine;

public class FixedRotator : MonoBehaviour
{
    [SerializeField] float speed;


    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.up * speed * Time.unscaledDeltaTime);        
    }
}
