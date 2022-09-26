using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CefSharp.OffScreen.Models
{
    public class Result
    {
        public List<Vehicle> firstVehicleList { get; set; }
        public List<Vehicle> firstVehicleHomeDeliveryList { get; set; }
        public VehicleDetail firstVehicleDetail { get; set; }
        public List<Vehicle> secondVehicleList { get; set; }
        public List<Vehicle> secondVehicleHomeDeliveryList { get; set; }
        public VehicleDetail secondVehicleDetail { get; set; }
    }
}
