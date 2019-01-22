using ElevatorLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElevatorApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var elevatorManager = new ElevatorCentralController(10, 3);
            elevatorManager.RequestElevator(10, Direction.Upward);
        }
    }
}
