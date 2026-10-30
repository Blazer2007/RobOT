using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController _CharController;

    private void Awake()
    {
        _CharController = GetComponent<CharacterController>();
    }

    private void MovePlayer() 
    {
        
    } 
}
