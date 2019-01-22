namespace ElevatorLibrary
{
    public class Elevator : IElevator, IElevatorButtonPanel
    {
        private readonly int _topFloorNumber;
        private readonly int _bottomFloorNumber;

        public IElevatorController ElevatorController { get; set; }
        public Direction Direction { get; set; }

        public Elevator(int topFloorNumber, int bottomFloorNumber, IElevatorController elevatorController)
        {
            _topFloorNumber = topFloorNumber;
            _bottomFloorNumber = bottomFloorNumber;
            ElevatorController = elevatorController;
        }

        public void DropAtFloor(int floorNumber)
        {
            ElevatorController.AddElevatorPanelStop(floorNumber);
        }

        public void Ascend()
        {
            Direction = Direction.Upward;
        }

        public void Decend()
        {
            Direction = Direction.Downward;
        }

        private void OpenDoor()
        {
            
        }

        private void CloseDoor()
        {
            
        }

        public void Continue()
        {
            CloseDoor();
        }

        public void Stop()
        {
            OpenDoor();
        }
    }
}
