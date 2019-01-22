using System;
using System.Collections.Generic;
using System.Linq;

namespace ElevatorLibrary
{
    public class ElevatorCentralController : IElevatorCentralController
    {
        enum RequestStatus
        {
            Open,
            Allotted,
            Serviced
        }

        class ServiceRequest
        {
            public int FloorNumber;
            public RequestStatus Status;
            public IElevatorController AllotedElevator;
            public Direction Direction;
        }

        private int _requestCounter = 0;

        private List<ServiceRequest> _requests;
        private readonly Int32 _noOfFloors;
        private readonly int _noOfLifts;

        private readonly List<IElevatorController> _elevators = new List<IElevatorController>();

        public ElevatorCentralController(int noOfFloors, int noOfLifts)
        {
            noOfFloors = noOfFloors;
            RegisterDepartedStopEvent();
            RegisterFloorServicedEvent();
        }

        public void RequestElevator(int floorNumber, Direction upward)
        {
            var existingRequest = _requests.FirstOrDefault(x => x.FloorNumber == floorNumber && x.Direction == upward);
            if (existingRequest != null)
                return;

            _requestCounter++;

            var elevator = FindNearestElevator(floorNumber, upward);
            var request = new ServiceRequest()
            {
                FloorNumber = floorNumber,
                Status = RequestStatus.Allotted,
                AllotedElevator = elevator,
                Direction = upward == Direction.Upward ? Direction.Upward : Direction.Downward
            };
            elevator.PickupAtFloor(floorNumber);
            _requests.Add(request);
        }

        private void RegisterFloorServicedEvent()
        {
            foreach (var elevator in _elevators)
            {
                elevator.FloorServiced += OnFloorServiced;
            }
        }

        private void RegisterDepartedStopEvent()
        {
            foreach (var elevator in _elevators)
            {
                elevator.DepartedFloor += OnDepartedFloor;
            }
        }

        private void OnDepartedFloor(object sender, int floorNumber)
        {
            var elevator = (Elevator) sender;
            int floorNoToLookup = 0;

            if (elevator.Direction == Direction.Downward)
            {
                floorNoToLookup = floorNumber - 1;
            }
            else
            {
                floorNoToLookup = floorNumber + 1;
            }

            var request = _requests.First(r => r.FloorNumber == floorNoToLookup);
            if (request.AllotedElevator.CurrentFloorNumber != floorNumber)
            {
                request.AllotedElevator.CancelFloor(floorNoToLookup, request.AllotedElevator.Direction);
                request.AllotedElevator = (IElevatorController)sender;
                request.AllotedElevator.PickupAtFloor(request.FloorNumber);
            }
        }

        private void OnFloorServiced(object sender, int floorNumber)
        {
            var elevator = (Elevator) sender;

            var request = _requests.FirstOrDefault(x => x.FloorNumber == floorNumber && elevator.Direction == x.Direction);
            if (request!=null)
            {
                request.Status = RequestStatus.Serviced;
                _requests.Remove(request);
                System.Diagnostics.Debug.Write(string.Format("Floor {0} is serviced", floorNumber));
            }
        }

        private IElevatorController FindNearestElevator(int floorNumber, Direction upward)
        {
            if (upward == Direction.Upward)
            {
                return GetClosestUpwardElevator(floorNumber);
            }
            else
            {
                return GetClosestDownwardElevator(floorNumber);    
            }
        }

        /// <summary>
        /// Find the nearest upward moving elevator for the requested floor number
        /// </summary>
        /// <param name="floorNumber"></param>
        /// <returns></returns>
        private IElevatorController GetClosestUpwardElevator(int floorNumber)
        {
            var upwardElevators =
                _elevators.Where(
                    x =>
                        (x.Status == ElevatorStatus.InFlight && x.Direction == Direction.Upward &&
                         x.CurrentFloorNumber < floorNumber)
                        || (x.Status == ElevatorStatus.WaitingForInput && x.CurrentFloorNumber > floorNumber)
                );

            IElevatorController upwardElevator = null;

            foreach (var e in upwardElevators)
            {
                if (e.CurrentFloorNumber < floorNumber)
                {
                    if ((upwardElevator != null && upwardElevator.CurrentFloorNumber < e.CurrentFloorNumber) ||
                        upwardElevator == null)
                    {
                        upwardElevator = e;
                    }
                }
            }
            return upwardElevator;
        }

        /// <summary>
        /// Find the nearest downward moving elevator for the requested floor number
        /// </summary>
        /// <param name="floorNumber"></param>
        /// <returns></returns>
        private IElevatorController GetClosestDownwardElevator(int floorNumber)
        {
            var downgoingElevators =
                _elevators.Where(
                    x =>
                        (x.Status == ElevatorStatus.InFlight && x.Direction == Direction.Downward &&
                         x.CurrentFloorNumber > floorNumber)
                        || (x.Status == ElevatorStatus.WaitingForInput && x.CurrentFloorNumber > floorNumber)
                );

            IElevatorController downwardElevator = null;

            foreach (var e in downgoingElevators)
            {
                if (e.CurrentFloorNumber > floorNumber)
                {
                    if ((downwardElevator != null && downwardElevator.CurrentFloorNumber > e.CurrentFloorNumber) ||
                        downwardElevator == null)
                    {
                        downwardElevator = e;
                    }
                }
            }
            return downwardElevator;
        }
    }
}
