using UnityEngine;

public class InputManager : MonoBehaviour
{

    public enum TargetPlatform
    {
        PC,
        MOBILE,
        CONTROLLER
    }
    public TargetPlatform platform;

    [Header("Movement Inputs")]
    [SerializeField] private Vector2 movement;
    [SerializeField] private bool jumpInput;
    [SerializeField] private bool diveInput;
    
    [Header("Functional Inputs")]
    [SerializeField] private bool spikeInput;
    [SerializeField] private bool bumpInput;
    
    
    [Header("Bind Settings")]
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode diveKey = KeyCode.Mouse1;
    [SerializeField] private KeyCode spikeKey = KeyCode.Mouse0;
    [SerializeField] private KeyCode bumpKey = KeyCode.Mouse0;

    public static InputManager instance { get; private set; }
    


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if(instance == null){ instance = this; }
        else if(instance != this){ Destroy(this); }
    }

    // Update is called once per frame
    void Update()
    {
        switch (platform)
        {
            case TargetPlatform.PC:
                movement = new Vector2(
                    Input.GetAxis("Horizontal"),
                    Input.GetAxis("Vertical")
                );

                jumpInput = Input.GetKey(jumpKey);
                diveInput = Input.GetKey(diveKey);
                spikeInput = Input.GetKey(spikeKey);
                bumpInput = Input.GetKey(bumpKey);

                return;
            
            case TargetPlatform.MOBILE:
                return;
        }
    }

    public Vector2 GetMovement(bool normalized = true)
    {
        // return normalized ? movement.normalized : movement;
        if(normalized){
            if(movement.magnitude > 1) { return movement.normalized;}
        }
        return movement;
        // ? movement.normalized : movement;
            
    }

    public bool GetDive()
    {
        return diveInput;
    }
    
    public bool GetJump(){
        return jumpInput;
    }
}
