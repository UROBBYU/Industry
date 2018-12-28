using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Industry.World.Roads;
using Industry.World.Vehicles;
using UnityEngine;
using Industry.Utilities;

namespace Industry.AI.Routing
{
    public class Route
    {
        private static int lastID = 0;
        
        public Route(Road fromEnd, Road toEnd, bool is_temp = false)
        {
            if (fromEnd == null || toEnd == null)
                throw new ArgumentNullException("from/to");

            if (fromEnd.type != RoadType.End || toEnd.type != RoadType.End)
                throw new ArgumentException("from's and/or to's type is not RoadType.End");

            _Init(RouteCreator.CreatePathWP(fromEnd, toEnd, true), is_temp);
        }
        public Route(List<WayPoint> wp_sequence, bool is_temp = false)
        {
            if (wp_sequence == null)
                throw new ArgumentNullException("WP_Sequence is null.");

            _Init(wp_sequence, is_temp);
        }
        private void _Init(List<WayPoint> wp_sequence, bool is_temp)
        {
            route = wp_sequence;
            IsTemp = is_temp;

            lastID++;
            RouteID = lastID;

            startWP = route[0];
            endWP = route[is_temp ? route.Count - 1 : route.Count / 2];
            startPoint = startWP.Position;
            endPoint = endWP.Position;

            s1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            s1.transform.position = startPoint;
            s1.transform.localScale *= 1.5f;
            s2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            s2.transform.position = endPoint;
            s2.transform.localScale *= 1.5f;

            vehicles = new HashSet<VehicleWP>();
        }

        private bool highlighted;

        private List<WayPoint> route;
        private HashSet<VehicleWP> vehicles;

        public GameObject s1, s2;

        public bool IsTemp
        {
            get; private set;
        }
        public int Count
        {
            get { return route.Count; }
        }
        public int RouteID
        {
            get; private set;
        }
        
        public Vector3 startPoint
        {
            get; private set;
        }
        public Vector3 endPoint
        {
            get; private set;
        }
        public WayPoint startWP
        {
            get; private set;
        }
        public WayPoint endWP
        {
            get; private set;
        }


        public void AddVehicle(VehicleWP vehicle)
        {
            vehicles.Add(vehicle);
        }
        public bool Contains(Road road)
        {
            return route.Find(r => r.road == road) != null;
        }
        public bool Contains(WayPoint wp)
        {
            return route.Contains(wp);
        }
        public void HighLight(bool on)
        {
            HighLight(on, IsTemp ? Color.red : Color.green);
        }
        public void HighLight(bool on, Color color)
        {
            if ((on && highlighted) || (!on && !highlighted))
                return;
            
            if (on && !highlighted)
            {
                for (int i = 1; i < route.Count; i++)
                    GizmosDrawer.AddVectorPair(new Vector3[] { route[i - 1].Position, route[i].Position }, color);

                //GizmosDrawer.AddVectorPair(new Vector3[] { route[route.Count - 1].transform.position, route[0].transform.position }, color);

                highlighted = true;
            }
            else
            {
                for (int i = 1; i < route.Count; i++)
                    GizmosDrawer.RemoveVectorPair(new Vector3[] { route[i - 1].Position, route[i].Position });

                //GizmosDrawer.RemoveVectorPair(new Vector3[] { endPoint, startPoint });

                highlighted = false;
            }
        }
        public int  IndexOf(WayPoint wp)
        {
            return route.IndexOf(wp);
        }        
        public void RemoveVehicle(VehicleWP vehicle)
        {
            vehicles.Remove(vehicle);
        }
        public void RemoveAllVehicles()
        {
            vehicles.Clear();
        }
        public void Set(List<WayPoint> wp_sequence)
        {
            if (wp_sequence == null)
                throw new ArgumentNullException("WP_Sequence is null.");

            this.route = wp_sequence;

            startWP = route[0];
            endWP = route[IsTemp ? route.Count - 1 : route.Count / 2];
            startPoint = startWP.Position;
            endPoint = endWP.Position;

            ResetSpheres();

            if (!IsTemp)
                foreach (var veh in vehicles)
                    veh.SetRoute(this);
        }
        public void SetRouteToAllVehicles(Route route)
        {
            foreach (var veh in vehicles)
            {
                if (route != null)
                    veh.SetRoute(route);
                else
                    veh.Abort("A null route was set at SetRouteToAllVehicles().");
            }
        }
        public void ResetSpheres()
        {
            if (s1 != null)
                s1.transform.position = startPoint;
            if (s2 != null)
                s2.transform.position = endPoint;
        }
        public void UpdateTempStartWP()
        {
            if (!IsTemp)
                return;
            
            if (startWP == null)
            {
                startWP = vehicles.First().current;
                startPoint = startWP.Position;
            }
        }
        
        public WayPoint this[int index]
        {
            get { return route[index]; }
        }
    }
}
