using System;

namespace ElevatorLibrary
{
    public interface IElevator
    {
        IElevatorController ElevatorController { get; set; }
        void Ascend();
        void Decend();
    }
}