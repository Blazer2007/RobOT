using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class RoomManager : MonoBehaviour
{
    [SerializeField] private ResetLevelManager currentRoom;

    [SerializeField] private List<ResetLevelManager> rooms = new List<ResetLevelManager>();
    [SerializeField] private KeyCode resetKey = KeyCode.B;
    [SerializeField] private KeyCode nextLevelKey = KeyCode.F2;
    [SerializeField] private KeyCode previousLevelKey = KeyCode.F3;

    private void Update()
    {
        if(Input.GetKeyDown(resetKey) && currentRoom != null) currentRoom.RestartLevel();
        if(Input.GetKeyDown(nextLevelKey)) GoToNextRoom();
        if(Input.GetKeyDown(previousLevelKey)) GoToPreviousRoom();

    }

    public void SetCurrentRoom(ResetLevelManager room)
    {
        currentRoom = room;
    }   

    public void ResetCurrentRoom()
    {
        if(currentRoom != null) currentRoom.RestartLevel();   
    }

    public void GoToNextRoom()
    {
        int nextRoom = rooms.IndexOf(currentRoom) + 1;

        if (nextRoom > rooms.Count) return;

        SetCurrentRoom(rooms[nextRoom]);
        currentRoom.RestartLevel();
    }

    public void GoToPreviousRoom()
    {
        int previousRoom = rooms.IndexOf(currentRoom) - 1;

        if (previousRoom < 0) return;

        SetCurrentRoom(rooms[previousRoom]);
        currentRoom.RestartLevel();
    }


}
