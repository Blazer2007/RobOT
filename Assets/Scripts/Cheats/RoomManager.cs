using UnityEngine;

public class RoomManager : MonoBehaviour
{
    [SerializeField] private ResetLevelManager currentRoom;
    [SerializeField] private KeyCode resetKey = KeyCode.B;

    private void Update()
    {
        if(Input.GetKeyDown(resetKey) && currentRoom != null) currentRoom.RestartLevel();
            
    }

    public void SetCurrentRoom(ResetLevelManager room)
    {
        currentRoom = room;
    }   

    public void ResetCurrentRoom()
    {
        if(currentRoom != null) currentRoom.RestartLevel();   
    }
}
