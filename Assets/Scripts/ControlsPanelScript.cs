using UnityEngine;
using UnityEngine.UI;

public class ControlsPanelScript : MonoBehaviour
{

    [SerializeField] Image move, jump, dive;
    [SerializeField] Color dimColor, fullColor = Color.white;



    void Update()
    {
        move.color = GameManager.instance.GetCurrentPlayer().movement.canMove ? fullColor : dimColor;
        jump.color = GameManager.instance.GetCurrentPlayer().movement.canJump ? fullColor : dimColor;
        dive.color = !GameManager.instance.GetCurrentPlayer().movement.diving ? fullColor : dimColor;
    }
}
