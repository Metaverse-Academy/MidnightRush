using UnityEngine;

public interface IFlashable
{
    bool IsWorking { get; }
    bool IsOn { get; }
    void TurnOn();
    void TurnOff();
    void StartRecharge();
}