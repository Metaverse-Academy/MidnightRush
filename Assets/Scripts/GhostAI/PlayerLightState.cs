using UnityEngine;

public class PlayerLightState : MonoBehaviour
{
    public bool isInDark { get; private set; } = true;
    public void SetInDark(bool state)
    {
        isInDark = state;
    }
    void OnTriggerEnter(Collider other)
    {
        // تحقق مما إذا كان المجسم الذي دخلت فيه يحتوي على سكربت LightRoomTrigger
        if (other.GetComponent<LightRoomTrigger>() != null)
        {
            Debug.Log("Player entered a lit area.");
            isInDark = false;
        }
    }

    void OnTriggerExit(Collider other)
    {
        // تحقق مما إذا كان المجسم الذي خرجت منه يحتوي على سكربت LightRoomTrigger
        if (other.GetComponent<LightRoomTrigger>() != null)
        {
            Debug.Log("Player exited a lit area.");
            isInDark = true;
        }
    }
}
