using System;

namespace ElevatorLibrary
{
    public interface IElevatorCentralController
    {
        void RequestElevator(int floorNumber, Direction upward);
    }
}