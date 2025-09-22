using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] Player currentPlayer, mainPlayer, secondPlayer;
    [SerializeField] CameraMovement cam;
    bool second = false;

    void ChangePlayerTarget(bool second){
        this.second = second;
        currentPlayer.movement.enabled = false;
        currentPlayer = second ? secondPlayer : mainPlayer;

        currentPlayer.movement.enabled = true;
        cam.ChangeTarget(currentPlayer.transform);
    }

    void Update(){
        if(Input.GetKeyDown(KeyCode.Y)){
            ChangePlayerTarget(!second);
        }
    }

    void Awake(){
        ChangePlayerTarget(false);
    }
}
