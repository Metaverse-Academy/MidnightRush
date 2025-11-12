using UnityEngine;

public class PlayerLightState : MonoBehaviour
{
    public bool isInDark = true; // If true, player is in dark; if false, player is in light
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("LightArea"))
            isInDark = false;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("LightArea"))
            isInDark = true;
    }
}