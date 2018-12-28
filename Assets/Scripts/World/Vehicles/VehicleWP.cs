using System.Collections.Generic;
using UnityEngine;
using Industry.AI.Routing;
using Industry.World.Map;
using Industry.World.Roads;
using Industry.World.Buildings;
using Industry.Utilities;

namespace Industry.World.Vehicles
{
    public abstract class VehicleWP : MonoBehaviour
    {
        protected static int vehicleCount;
        
        protected bool can_move;
        private bool on_main_route;
        public bool route_forward;

        private int current_WP;

        private RouteWP route, temp;
        private RouteSphere sphereCurr;
        private Vector3 lastSuccessfulPos;
        private Vector3 v_height;
        private Quaternion lastSuccessfulRot;

        public float MoveSpeed
        {
            get; protected set;
        }
        public float RotateSpeed
        {
            get; protected set;
        }
        public Vector3 Position
        {
            get { return transform.position; }
            protected set { transform.position = value; }
        }
        public WayPoint current
        {
            get; private set;
        }

        public GameObject sphere;
        public Vector3 trRight;

        void Start()
        {
            if (route == null) { Abort("Route is null"); return; }

            sphereCurr = new RouteSphere(route.startPoint, 0.5f, Color.red);
            
            MoveSpeed = 8.0f;
            RotateSpeed = 5.0f;
            v_height = new Vector3(0f, 1.5f, 0f);

            can_move = true;
            on_main_route = true;
            route_forward = true;

            current = route[0];

            //FindWayToRoute();
            route.HighLight(true, Color.green);

            lastSuccessfulPos = transform.position;
            lastSuccessfulRot = transform.rotation;

            vehicleCount++;

            _Start();
        }
        void Update()
        {
            if (can_move)
                PreMove();

            _Update();
        }
        
        /*
        private void FindWayToRoute()
        {
            if (temp != null)
                RouteSetWP.RemoveRoute(temp);

            Road from = ExtractRoadFrom(WorldMap.GetTile(Position));
            WayPoint fromWP = from.WPContainer.GetClosestWP(Position);

            if (Vector3.Dot(Position + transform.right, transform.InverseTransformPoint(fromWP.Position)) < 0)
                fromWP = from.WPContainer.GetClosestWPExcept(Position, fromWP);

            temp = new RouteWP(RouteCreator.CreatePathToRouteWP(prev, route, route_forward), true);

            if (temp == null) { Abort("Path does not exist"); return; }

            if (!route.Contains(temp[temp.Count - 1]))
            {
                Debug.Log("<color=red>WTF temp route does not lead to main route</color>\nTrying to recalculate...");

                temp = new RouteWP(RouteCreator.CreatePathToRouteWP(fromWP, route, route_forward), true);

                if (!route.Contains(temp[temp.Count - 1]))
                    Debug.Log("<color=red>RECALCULATION FAILED</color>");
            }

            RouteSetWP.AddRoute(temp, false);

            on_main_route = false;
            current_WP = 0;

            if (sphereCurr == null)
                throw new System.NullReferenceException("sphereCurr");

            sphereCurr.SetPosition(fromWP.Position);
            
            temp.HighLight(true, Color.red);
        }
        */

        private void FindWayToRoute()
        {
            List<WayPoint> newTemp = RouteCreator.CreatePathWP(current, route_forward ? route.endWP : route.startWP, false);

            if (newTemp == null) { Abort("Path does not exist"); return; }

            if (temp != null)
                RouteSetWP.RemoveRoute(temp);

            temp = new RouteWP(newTemp, true);
            RouteSetWP.AddRoute(temp, false);
            
            on_main_route = false;
            current_WP = 0;
            
            sphereCurr.SetPosition(current.Position);
        }
        
        private void Move()
        {
            RouteWP _route = on_main_route ? route : temp;

            if (current_WP < 0 || current_WP >= _route.Count)
                throw new System.ArgumentOutOfRangeException(current_WP.ToString());

            //WayPoint toGo = current;
            //Vector3 WPpos = toGo.Position + v_height;
            
            WayPoint toGo = _route[current_WP];

            if (toGo == null)
            {
                Debug.Log("<color=red>toGo is null</color>");


                return;
            }

            Vector3 WPpos = toGo.Position + v_height;
            Position = Vector3.MoveTowards(this.Position, WPpos, Time.deltaTime * MoveSpeed);

            //var rotation = Quaternion.LookRotation(toGo.Position - this.Position);
            var rotation = Quaternion.LookRotation(toGo.Position - this.Position);
            rotation.eulerAngles = new Vector3(0, rotation.eulerAngles.y + 90f, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * RotateSpeed);

            sphere.transform.position = (Position + 4 * transform.right);

            if (Vector3.Distance(WPpos, this.Position) <= 0.6f)
            {
                current_WP++;

                if (!on_main_route && current_WP >= temp.Count)
                {
                    current = temp.endWP;

                    //if (route.Contains(curr)) { }
                    on_main_route = true;

                    RouteSetWP.RemoveRoute(temp);

                    temp.HighLight(false);
                    temp = null;
                    
                    current_WP = route.IndexOf(current);
                    //sphereCurr.SetPosition(current.Position);
                }

                if (current_WP < _route.Count)
                {
                    current = _route[current_WP];
                    sphereCurr.SetPosition(current.Position);
                }
                
                if (current_WP > route.Count / 2)
                    route_forward = false;
            }

            if (current_WP >= route.Count)
            {
                if (!on_main_route)
                    Abort("Did not reach main route");

                route_forward = true;

                current_WP = 0;
                current = route.startWP;

                sphereCurr.SetPosition(current.Position);
            }
        }
        private void PreMove()
        {
            if (route == null) { Abort("has no Route"); return; }

            //Road curr = ExtractRoadFrom(WorldMap.GetTile(transform.position));
            //if (curr == null) { Abort("is off-road"); return; }
            
            if (!(temp ?? route).Contains(current))
            {
                FindWayToRoute();
            }
            else
            {
                Move();
            }
        }
        private Road ExtractRoadFrom(Tile tile)
        {
            if (tile.Owner == null) return null;
            
            Road road = tile.Owner as Road;

            if (road != null)
            {
                return road;
            }
            else if (tile.Owner is EntranceBuilding)
            {
                Road entrance = (tile.Owner as EntranceBuilding).entrance;
                if (Mathf.Approximately(entrance.Position.x, tile.Position.x) && 
                    Mathf.Approximately(entrance.Position.z, tile.Position.z))
                    return entrance;
            }

            return null;
        }
        
        public void Abort(string reason)
        {
            //enabled = false;
            can_move = false;
            Debug.Log("<color=red> Aborted:</color> Vehicle \"" + name + "\": " + reason + "!");
        }
        public void SetRoute(RouteWP _route)
        {
            if (_route == null)
                throw new System.NullReferenceException("route is null");

            if (this.route == null)
            {
                this.route = _route;
            }
            else
            {
                //this.route.HighLight(false);

                this.route = _route;
                current_WP = _route.IndexOf(current);

                if (current_WP < 0)
                    FindWayToRoute();
                sphereCurr.SetPosition(current.Position);


                //this.route.HighLight(true);
            }

        }
        
        public abstract void Stop();
        protected abstract void _Start();
        protected abstract void _Update();
    }
}
