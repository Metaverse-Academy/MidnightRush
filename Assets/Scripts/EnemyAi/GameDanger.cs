// GameDanger.cs (put on an empty object)
using UnityEngine;

public class GameDanger : MonoBehaviour
{
    public static GameDanger I;
    [Range(0,1)] public float lampCharge = 1f;  // 1=bright, 0=dead
    public bool flashlightOn = true;            // protector’s torch
    private void Awake(){ I = this; }
}
