using UnityEngine;

public class RoomTrigger : MonoBehaviour
{
    [SerializeField] private ResetLevelManager room;
    [SerializeField] private RoomManager roomManager;

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            roomManager.SetCurrentRoom(room);
        }
    }
}
