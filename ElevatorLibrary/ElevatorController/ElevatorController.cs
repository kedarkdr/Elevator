using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ElevatorLibrary
{
    public class ElevatorController : IElevatorController
    {
        private Elevator _elevator;
        private const int STOP_TIME_MS = 1000;
        private const int TIME_TO_REACH_NEXT_FLOOR = 1000;

        private readonly int _topFloorNumber;
        private int _currentFloorNumber = 0;
        private int _bottomFloorNumber;
        private ElevatorStatus _status = ElevatorStatus.WaitingForInput;
        private List<int> _upwardstops = new List<int>();
        private List<int> _downwardstops = new List<int>();

        public event EventHandler<int> FloorServiced;
        public event EventHandler<int> DepartedFloor;
        
        public Direction Direction {
            get { return _elevator.Direction; }
        }

        public ElevatorController(int topFloorNumber, int bottomFloorNumber)
        {
            _elevator = new Elevator(topFloorNumber, bottomFloorNumber, this);
            _topFloorNumber = topFloorNumber;
            _bottomFloorNumber = bottomFloorNumber;
        }

        public int CurrentFloorNumber
        {
            get { return _currentFloorNumber; }
            private set { _currentFloorNumber = value; }
        }

        public ElevatorStatus Status
        {
            get { return _status; }
        }

        public void AddElevatorPanelStop(int floorNumber)
        {
            AddStop(floorNumber);
            Move();
        }

        public void CancelFloor(int floorNumber, Direction direction)
        {
            RemoveStop(floorNumber, direction);
        }

        public void PickupAtFloor(int floorNumber)
        {
            AddStop(floorNumber);
            Move();
        }

        private void RemoveStop(int floorNumber, Direction direction)
        {
            var stops = direction == Direction.Downward ? _downwardstops : _upwardstops;
            stops.Remove(floorNumber);
        }

        private void SetDirection(int floorNumber)
        {
            if (Status == ElevatorStatus.InFlight)
                return;
            if (Status == ElevatorStatus.WaitingForInput)
            {
                if (_currentFloorNumber > floorNumber)
                {
                    _elevator.Direction = Direction.Downward;
                }
                else
                {
                    _elevator.Direction = Direction.Upward;
                }
            }
        }

        private void AddStop(int floorNumber)
        {
            SetDirection(floorNumber);

            List<int> stopslist = null;

            if ( (_elevator.Direction == Direction.Downward && _currentFloorNumber >= floorNumber) || 
                (_elevator.Direction == Direction.Upward && _currentFloorNumber > floorNumber) )
            {
                stopslist = _downwardstops;
            }
            else if ( (_elevator.Direction == Direction.Downward && _currentFloorNumber < floorNumber) ||
                (_elevator.Direction == Direction.Upward && _currentFloorNumber < floorNumber) )
            {
                stopslist = _upwardstops;
            }

            if (_currentFloorNumber == floorNumber || floorNumber > _topFloorNumber || floorNumber < _bottomFloorNumber)
                return;

            if (stopslist.Any(s => s == floorNumber))
            {
                return;
            }

            stopslist.Add(floorNumber);
            stopslist.Sort();
        }

        private void Move()
        {
            _status = ElevatorStatus.InFlight;

            if (_elevator.Direction == Direction.Downward)
            {
                Decend();
            }
            else
            {
                Ascend();
            }
        }

        private void Ascend()
        {
            while (_upwardstops.Any())
            {
                _elevator.Direction = Direction.Upward;

                //assume that we reach next floor in 1000ms, 
                //this can be replaced by another event notifier that can raise an event when a floor is reached
                Thread.Sleep(TIME_TO_REACH_NEXT_FLOOR);
                _currentFloorNumber++;

                if (_upwardstops.Any(x => x == _currentFloorNumber))
                {
                    Stop();
                }

                NotifyFloorDeparture();
                _upwardstops.Remove(_upwardstops.First());
            }

            if (_downwardstops.Any())
            {
                Decend();
            }
            else
            {
                _status = ElevatorStatus.WaitingForInput;
            }
        }

        private void Decend()
        {
            while (_downwardstops.Any())
            {
                _elevator.Direction = Direction.Downward;

                Thread.Sleep(TIME_TO_REACH_NEXT_FLOOR);

                _currentFloorNumber--;

                if (_downwardstops.Any(x => x == _currentFloorNumber))
                {
                    Stop();
                }

                NotifyFloorDeparture();
                _downwardstops.Remove(_downwardstops.First());
            }

            if (_upwardstops.Any())
            {
                Ascend();
            }
            else
            {
                _status = ElevatorStatus.WaitingForInput;
            }
        }

        private void NotifyFloorDeparture()
        {
            if (null != DepartedFloor)
                DepartedFloor.Invoke(this, _currentFloorNumber);
        }

        private void NotifyFloorServiced()
        {
            if (null != FloorServiced)
                FloorServiced.Invoke(this, _currentFloorNumber);
        }

        private void Stop()
        {
            _elevator.Stop();
            Thread.Sleep(STOP_TIME_MS);
            _elevator.Continue();
            NotifyFloorServiced();
        }
    }
}
