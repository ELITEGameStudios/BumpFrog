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
    [SerializeField] private bool switchInput;
    [SerializeField] private bool startInput;
    
    
    [Header("Bind Settings")]
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode diveKey = KeyCode.Mouse1;
    [SerializeField] private KeyCode switchKey = KeyCode.Mouse0;
    [SerializeField] private KeyCode bumpKey = KeyCode.Mouse0;
    // [SerializeField] private KeyCode startKey = KeyCode.KeypadEnter;

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
                switchInput = Input.GetKeyDown(switchKey);
                startInput = Input.anyKeyDown;

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
    
    public bool GetSwitch()
    {
        return switchInput;
    }

    public bool GetStart()
    {
        return startInput && !Input.GetKeyDown(KeyCode.Escape);
    }
}
