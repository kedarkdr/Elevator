using System;
namespace ElevatorLibrary
{
    public interface IElevatorController
    {
        //This event is raised whenever an elevator makes a stop at a floor
        event EventHandler<int> FloorServiced;
        //This event is raised whenever an elevator passes a floor, this event is raised regardless of whetner elevator makes a stop or not
        event EventHandler<int> DepartedFloor;

        int CurrentFloorNumber { get; }
        Direction Direction { get; }
        ElevatorStatus Status { get; }

        void PickupAtFloor(int floorNumber);
        void CancelFloor(int floorNumber, Direction direction);

        void AddElevatorPanelStop(int floorNumber);
    }
}