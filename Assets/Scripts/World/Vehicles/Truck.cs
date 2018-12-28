using System;
using System.Collections.Generic;
using UnityEngine;

namespace Industry.World.Vehicles
{
    public class Truck : VehicleWP
    {
        public int capacity;




        public override void Stop()
        {
            //Speed = 0f;
        }

        protected override void _Start()
        {
            //vehicleCount++;
            gameObject.name = "Truck " + vehicleCount;
            
        }

        protected override void _Update()
        {
            
        }
    }
}
