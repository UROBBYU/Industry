//using System.Collections.Generic;
//using UnityEngine;
//using Industry.AI.Routing;
//using Industry.World.Map;
//using Industry.World.Roads;
//using Industry.World.Buildings;

//namespace Industry.World.Vehicles
//{
//    public abstract class Vehicle : MonoBehaviour
//    {
//        protected static int vehicleCount;

//        private enum Direction
//        {
//            North, South, East, West
//        }
//        private enum Mode
//        {
//            Forward, TurnLeft, TurnRight, U_Turn
//        }


//        protected bool canMove;
//        //private bool route_forward;
//        private int points_straight;
//        private int points_round;
//        private int points_current;
//        private float s_angle;

//        private Direction direction, dir_after_rotation;
//        private Mode mode;

//        private Road first, last, previous, current, next;
//        private Route route, temp;
//        private RouteSphere sphereCurr, sphereNext, lastSuccess;
//        private Vector3 rotationPoint;
//        private Vector3 lastSuccessfulPos;
//        private Quaternion lastSuccessfulRot;

//        public float Speed
//        {
//            get; protected set;
//        }


//        void Start()
//        {
//            if (route == null) { Abort("Route is null"); return; }

//            sphereCurr = new RouteSphere(route[0].Position, 1.3f, Color.yellow);
//            sphereNext = new RouteSphere(route[1].Position, 1.3f, Color.blue);
//            lastSuccess = new RouteSphere(transform.position, 1.3f, Color.black);

//            FindWayToRoute();

//            Speed = 5.0f;
//            canMove = true;
//            //route_forward = true;

//            float ts2 = WorldMap.TileSize / 2;

//            first = route[0];
//            next = route[1];
//            current = first;
//            previous = first;
//            last = route[route.Count - 1];

//            switch (current.direction)
//            {
//                case RoadDirection.North:
//                    direction = Direction.North;
//                    transform.rotation = Quaternion.Euler(0, 90, 0);
//                    Translate(new Vector3(WorldMap.TileSize / 4, 0, 0));
//                    break;
//                case RoadDirection.South:
//                    direction = Direction.South;
//                    transform.rotation = Quaternion.Euler(0, 270, 0);
//                    Translate(new Vector3(-WorldMap.TileSize / 4, 0, 0));
//                    break;
//                case RoadDirection.East:
//                    direction = Direction.East;
//                    transform.rotation = Quaternion.Euler(0, 180, 0);
//                    Translate(new Vector3(0, 0, -WorldMap.TileSize / 4));
//                    break;
//                case RoadDirection.West:
//                    direction = Direction.West;
//                    transform.rotation = Quaternion.Euler(0, 0, 0);
//                    Translate(new Vector3(0, 0, WorldMap.TileSize / 4));
//                    break;
//            }

//            lastSuccessfulPos = transform.position;
//            lastSuccessfulRot = transform.rotation;

//            dir_after_rotation = direction;

//            _Start();
//        }
//        void Update()
//        {
//            if (canMove)
//                PreMove();

//            _Update();
//        }

//        private bool EnsureRouteIntegrity()
//        {
//            for (int i = 0; i < route.Count; i++)
//                if (route[i] == null)
//                    return false;
//            return true;
//        }
//        private void FindWayToRoute()
//        {
//            Road from = ExtractRoadFrom(WorldMap.GetTile(transform.position));
//            if (from == null)
//                from = ExtractRoadFrom(WorldMap.GetTile(lastSuccessfulPos));


//            if (temp == null) temp = route;

//            route.HighLight(false);
//            temp.HighLight(false);

//            route = new Route(RouteCreator.CreatePathToRoute(from, temp));
//            if (route == null) { Abort("Path does not exist"); return; }

//            first = route[0];
//            current = first;
//            next = route[1];
//            last = route[route.Count - 1];

//            sphereCurr.SetPosition(current.Position);
//            sphereNext.SetPosition(next.Position);

//            route.HighLight(true, Color.red, 1.2f);
//            temp.HighLight(true);
//        }
//        private void Move()
//        {
//            if (current != previous)
//            {
//                previous = current;

//                if (temp != null && temp.Contains(current))
//                {
//                    route.HighLight(false);

//                    RouteSet.RemoveRoute(route);
//                    route = temp;
//                    first = route[0];
//                    last = route[route.Count - 1];
//                    temp = null;
//                }

//                if (current == first || current == last)
//                {
//                    route.Reverse();
//                    next = route[1];
//                }
//                else
//                {
//                    int curr = route.IndexOf(current);
//                    next = route[curr + 1];
//                }

//                lastSuccessfulPos = transform.position;
//                lastSuccessfulRot = transform.rotation;
//                lastSuccess.SetPosition(transform.position);
//                sphereCurr.SetPosition(current.Position);
//                sphereNext.SetPosition(next.Position);

//                ResetMode();
//            }

//            #region Movement
//            if (mode == Mode.Forward)
//            {
//                Translate(GetDirection() * Speed * Time.deltaTime);
//                transform.rotation = Quaternion.Euler(GetRotation());
//            }
//            else if (mode == Mode.TurnLeft)
//            {
//                if (points_straight == 0 || points_round == 0)
//                {
//                    float circle = Mathf.PI * (WorldMap.TileSize * 3 / 8);
//                    points_round = Mathf.RoundToInt(circle * Speed); //////
//                    s_angle = -90.0f / points_round;
//                }

//                points_current++;

//                if (points_current > 0 && points_current <= points_round + 1)
//                {
//                    transform.RotateAround(rotationPoint, Vector3.up, s_angle);
//                }

//            }
//            else if (mode == Mode.TurnRight)
//            {
//                if (points_straight == 0 || points_round == 0)
//                {
//                    float circle = Mathf.PI * (WorldMap.TileSize / 8);
//                    points_round = Mathf.RoundToInt(circle * Speed); //////
//                    s_angle = 90.0f / points_round;
//                }

//                points_current++;

//                if (points_current > 0 && points_current <= points_round + 1)
//                {
//                    transform.RotateAround(rotationPoint, Vector3.up, s_angle);
//                }

//            }
//            else // Mode.U_Turn
//            {
//                if (points_straight == 0 || points_round == 0)
//                {
//                    points_straight = Mathf.RoundToInt(WorldMap.TileSize / (Speed * 0.01f));
//                    points_round = Mathf.RoundToInt(WorldMap.TileSize / 4 * Mathf.PI * Speed);
//                    s_angle = -180.0f / points_round;
//                }

//                points_current++;

//                if (points_current > 0 && points_current <= points_straight / 2)
//                {
//                    Translate(GetDirection() * Speed * 0.01f);
//                    transform.rotation = Quaternion.Euler(GetRotation());
//                }
//                else if (points_current > (points_straight / 2) && points_current <= (points_straight / 2) + points_round)
//                {
//                    transform.RotateAround(current.Position, Vector3.up, s_angle);
//                }
//                else if (points_current > (points_straight / 2) + points_round && points_current <= points_straight + points_round + 1)
//                {
//                    direction = dir_after_rotation;

//                    Translate(GetDirection() * Speed * 0.01f); ////// dTime
//                    transform.rotation = Quaternion.Euler(GetRotation());
//                }
//            }
//            #endregion
//        }
//        private void PreMove()
//        {
//            current = ExtractRoadFrom(WorldMap.GetTile(transform.position));
//            if (route == null) { Abort("has no Route"); return; }
//            //if (current == null) { Abort("is off-road"); return; }

//            if (!EnsureRouteIntegrity() || !route.Contains(current))
//            {
//                FindWayToRoute();
//                ResetMode();
//            }

//            else
//            {
//                Move();
//            }

//        }
//        private void ResetMode()
//        {
//            points_straight = 0;
//            points_round = 0;
//            points_current = 0;
//            s_angle = 0.0f;

//            rotationPoint = Vector3.zero;

//            direction = dir_after_rotation;

//            Direction rr = GetRoadRelation(current, next);

//            if (current.type == RoadType.Straight)
//            {
//                switch (direction)
//                {
//                    case Direction.North:
//                        if (rr == Direction.North)
//                        {
//                            mode = Mode.Forward;
//                            dir_after_rotation = Direction.North;
//                        }
//                        else
//                        {
//                            mode = Mode.U_Turn;
//                            dir_after_rotation = Direction.South;
//                        }
//                        break;
//                    case Direction.South:
//                        if (rr == Direction.South)
//                        {
//                            mode = Mode.Forward;
//                            dir_after_rotation = Direction.South;
//                        }
//                        else
//                        {
//                            mode = Mode.U_Turn;
//                            dir_after_rotation = Direction.North;
//                        }
//                        break;
//                    case Direction.West:
//                        if (rr == Direction.West)
//                        {
//                            mode = Mode.Forward;
//                            dir_after_rotation = Direction.West;
//                        }
//                        else
//                        {
//                            mode = Mode.U_Turn;
//                            dir_after_rotation = Direction.East;
//                        }
//                        break;
//                    case Direction.East:
//                        if (rr == Direction.East)
//                        {
//                            mode = Mode.Forward;
//                            dir_after_rotation = Direction.East;
//                        }
//                        else
//                        {
//                            mode = Mode.U_Turn;
//                            dir_after_rotation = Direction.West;
//                        }
//                        break;
//                }

//            }
//            else if (current.type == RoadType.End)
//            {
//                mode = Mode.U_Turn;

//                switch (direction)
//                {
//                    case Direction.North: dir_after_rotation = Direction.South; break;
//                    case Direction.South: dir_after_rotation = Direction.North; break;
//                    case Direction.West: dir_after_rotation = Direction.East; break;
//                    case Direction.East: dir_after_rotation = Direction.West; break;
//                }
//            }
//            else if (current.type == RoadType.Cross_T || current.type == RoadType.Cross_X)
//            {
//                Tile tile = WorldMap.GetTile(transform.position);

//                if (rr == Direction.North)
//                {
//                    if (direction == Direction.North)
//                        mode = Mode.Forward;
//                    else if (direction == Direction.South)
//                        mode = Mode.U_Turn;
//                    else if (direction == Direction.East)
//                    {
//                        mode = Mode.TurnLeft;
//                        rotationPoint = tile.Position + new Vector3(-WorldMap.TileSize / 2, 0, WorldMap.TileSize / 2);
//                    }
//                    else if (direction == Direction.West)
//                    {
//                        mode = Mode.TurnRight;
//                        rotationPoint = tile.Position + new Vector3(WorldMap.TileSize / 2, 0, WorldMap.TileSize / 2);
//                    }
//                    dir_after_rotation = Direction.North;
//                }
//                else if (rr == Direction.South)
//                {
//                    if (direction == Direction.South)
//                        mode = Mode.Forward;
//                    else if (direction == Direction.North)
//                        mode = Mode.U_Turn;
//                    else if (direction == Direction.East)
//                    {
//                        mode = Mode.TurnRight;
//                        rotationPoint = tile.Position + new Vector3(-WorldMap.TileSize / 2, 0, -WorldMap.TileSize / 2);
//                    }
//                    else if (direction == Direction.West)
//                    {
//                        mode = Mode.TurnLeft;
//                        rotationPoint = tile.Position + new Vector3(WorldMap.TileSize / 2, 0, -WorldMap.TileSize / 2);
//                    }
//                    dir_after_rotation = Direction.South;
//                }
//                else if (rr == Direction.West)
//                {
//                    if (direction == Direction.West)
//                        mode = Mode.Forward;
//                    else if (direction == Direction.East)
//                        mode = Mode.U_Turn;
//                    else if (direction == Direction.North)
//                    {
//                        mode = Mode.TurnLeft;
//                        rotationPoint = tile.Position + new Vector3(-WorldMap.TileSize / 2, 0, -WorldMap.TileSize / 2);
//                    }
//                    else if (direction == Direction.South)
//                    {
//                        mode = Mode.TurnRight;
//                        rotationPoint = tile.Position + new Vector3(-WorldMap.TileSize / 2, 0, WorldMap.TileSize / 2);
//                    }
//                    dir_after_rotation = Direction.West;
//                }
//                else if (rr == Direction.East)
//                {
//                    if (direction == Direction.East)
//                        mode = Mode.Forward;
//                    else if (direction == Direction.West)
//                        mode = Mode.U_Turn;
//                    else if (direction == Direction.North)
//                    {
//                        mode = Mode.TurnRight;
//                        rotationPoint = tile.Position + new Vector3(WorldMap.TileSize / 2, 0, -WorldMap.TileSize / 2);
//                    }
//                    else if (direction == Direction.South)
//                    {
//                        mode = Mode.TurnLeft;
//                        rotationPoint = tile.Position + new Vector3(WorldMap.TileSize / 2, 0, WorldMap.TileSize / 2);
//                    }
//                    dir_after_rotation = Direction.East;
//                }
//            }
//            else if (current.type == RoadType.Corner)
//            {
//                Tile tile = WorldMap.GetTile(transform.position);

//                if (rr == Direction.North)
//                {
//                    if (direction == Direction.East)
//                    {
//                        mode = Mode.TurnLeft;
//                        rotationPoint = tile.Position + new Vector3(-WorldMap.TileSize / 2, 0, WorldMap.TileSize / 2);
//                    }
//                    else if (direction == Direction.West)
//                    {
//                        mode = Mode.TurnRight;
//                        rotationPoint = tile.Position + new Vector3(WorldMap.TileSize / 2, 0, WorldMap.TileSize / 2);
//                    }
//                    dir_after_rotation = Direction.North;
//                }
//                else if (rr == Direction.South)
//                {
//                    if (direction == Direction.East)
//                    {
//                        mode = Mode.TurnRight;
//                        rotationPoint = tile.Position + new Vector3(-WorldMap.TileSize / 2, 0, -WorldMap.TileSize / 2);
//                    }
//                    else if (direction == Direction.West)
//                    {
//                        mode = Mode.TurnLeft;
//                        rotationPoint = tile.Position + new Vector3(WorldMap.TileSize / 2, 0, -WorldMap.TileSize / 2);
//                    }
//                    dir_after_rotation = Direction.South;
//                }
//                else if (rr == Direction.West)
//                {
//                    if (direction == Direction.North)
//                    {
//                        mode = Mode.TurnLeft;
//                        rotationPoint = tile.Position + new Vector3(-WorldMap.TileSize / 2, 0, -WorldMap.TileSize / 2);
//                    }
//                    else if (direction == Direction.South)
//                    {
//                        mode = Mode.TurnRight;
//                        rotationPoint = tile.Position + new Vector3(-WorldMap.TileSize / 2, 0, WorldMap.TileSize / 2);
//                    }
//                    dir_after_rotation = Direction.West;
//                }
//                else if (rr == Direction.East)
//                {
//                    if (direction == Direction.North)
//                    {
//                        mode = Mode.TurnRight;
//                        rotationPoint = tile.Position + new Vector3(WorldMap.TileSize / 2, 0, -WorldMap.TileSize / 2);
//                    }
//                    else if (direction == Direction.South)
//                    {
//                        mode = Mode.TurnLeft;
//                        rotationPoint = tile.Position + new Vector3(WorldMap.TileSize / 2, 0, WorldMap.TileSize / 2);
//                    }
//                    dir_after_rotation = Direction.East;
//                }
//            }
//        }

//        private Road ExtractRoadFrom(Tile tile)
//        {
//            if (tile.Owner == null) return null;

//            if (tile.Owner.GetType() == typeof(Road))
//            {
//                return tile.Owner as Road;
//            }
//            if (tile.Owner.GetType().IsSubclassOf(typeof(EntranceBuilding)))
//            {
//                Road entrance = (tile.Owner as EntranceBuilding).entrance;
//                if (Mathf.Approximately(entrance.Position.x, tile.Position.x) && Mathf.Approximately(entrance.Position.z, tile.Position.z))
//                    return entrance;
//            }

//            return null;
//        }
//        private Vector3 GetDirection()
//        {
//            switch (direction)
//            {
//                case Direction.North: return Vector3.forward;
//                case Direction.South: return Vector3.back;
//                case Direction.East: return Vector3.right;
//                case Direction.West: return Vector3.left;
//                default: throw new System.Exception("GetDirection()");
//            }
//        }
//        private Vector3 GetRotation()
//        {
//            switch (direction)
//            {
//                case Direction.North: return new Vector3(0, 90, 0);
//                case Direction.South: return new Vector3(0, 270, 0);
//                case Direction.East: return new Vector3(0, 180, 0);
//                case Direction.West: return new Vector3(0, 0, 0);
//                default: throw new System.Exception("GetRotation()");
//            }
//        }
//        private Direction GetRoadRelation(Road r1, Road r2)
//        {
//            if (r1.north == r2)
//                return Direction.North;
//            if (r1.south == r2)
//                return Direction.South;
//            if (r1.east == r2)
//                return Direction.East;
//            if (r1.west == r2)
//                return Direction.West;

//            throw new System.ArgumentException("Roads are not nearby each other.");
//        }

//        public void Abort(string reason)
//        {
//            enabled = false;
//            Debug.Log("<color=red> Aborted:</color> Vehicle \"" + name + "\": " + reason + "!");
//        }
//        public void SetRoute(Route route)
//        {
//            this.route = route;
//        }
//        public void Translate(Vector3 direction)
//        {
//            transform.Translate(direction, Space.World);
//        }

//        public abstract void Stop();
//        protected abstract void _Start();
//        protected abstract void _Update();
//    }
//}



///*
//using System.Collections.Generic;
//using UnityEngine;
//using Industry.AI.Routing;
//using Industry.World.Map;
//using Industry.World.Roads;
//using Industry.World.Buildings;

//namespace Industry.World.Vehicles
//{
//    public abstract class Vehicle : MonoBehaviour
//    {
//        protected static int vehicleCount;

//        private enum Direction
//        {
//            North, South, East, West
//        }
        
//        private Route mainRoute, tempRoute;
//        private RouteSphere sphereCurr, sphereNext, lastSuccess;

//        protected bool canMove;

//        private Route Route
//        {
//            get { return tempRoute == null ? mainRoute : tempRoute; }
//        }
//        public float Speed
//        {
//            get; protected set;
//        }
//        public Vector3 Position
//        {
//            get { return transform.position; }
//            set { transform.position = value; }
//        }

//        void Start()
//        {
//            if (mainRoute == null) { Abort("Route is null"); return; }

//            sphereCurr = new RouteSphere(mainRoute[0].Position, 1.3f, Color.yellow);
//            sphereNext = new RouteSphere(mainRoute[1].Position, 1.3f, Color.blue);
//            lastSuccess = new RouteSphere(transform.position, 1.3f, Color.black);

//            Speed = 5.0f;
//            canMove = true;

//            FindWayToRoute();

//            _Start();
//        }
//        void Update()
//        {
//            if (canMove)
//                PreMove();
            
//            _Update();
//        }

        
//        private void PreMove()
//        {
//            Road current = Extract(Position);
            

//        }

//        private void Move()
//        {

//        }

//        private void FindWayToRoute()
//        {
//            Road from = Extract(Position);

//            if (tempRoute == null)
//                tempRoute = mainRoute;

//            mainRoute = new Route(RouteCreator.CreatePathToRoute(from, tempRoute));

//            if (mainRoute == null) { Abort("Path does not exist"); return; }

//            //route.HighLight(false);
//            //temp.HighLight(false);
            
//            /*
//            first = route[0];
//            current = first;
//            next = route[1];
//            last = route[route.Count - 1];
//            ***

//            //sphereCurr.SetPosition(current.Position);
//            //sphereNext.SetPosition(next.Position);

//            //route.HighLight(true, Color.red, 1.2f);
//            //temp.HighLight(true);
//        }
//        private Road Extract(Tile tile)
//        {
//            if (tile.Owner == null) return null;

//            if (tile.Owner.GetType() == typeof(Road))
//            {
//                return tile.Owner as Road;
//            }
//            if (tile.Owner.GetType().IsSubclassOf(typeof(EntranceBuilding)))
//            {
//                Road entrance = (tile.Owner as EntranceBuilding).entrance;
//                if (Mathf.Approximately(entrance.Position.x, tile.Position.x) && Mathf.Approximately(entrance.Position.z, tile.Position.z))
//                    return entrance;
//            }

//            return null;
//        }
//        private Road Extract(Vector3 pos)
//        {
//            return Extract(WorldMap.GetTile(pos));
//        }
//        private Direction GetRoadRelation(Road r1, Road r2)
//        {
//            if (r1.north == r2)
//                return Direction.North;
//            if (r1.south == r2)
//                return Direction.South;
//            if (r1.east == r2)
//                return Direction.East;
//            if (r1.west == r2)
//                return Direction.West;
            
//            throw new System.ArgumentException("Roads are not nearby each other.");
//        }

//        public void Abort(string reason)
//        {
//            enabled = false;
//            Debug.Log("<color=red> Aborted:</color> Vehicle \"" + name + "\": " + reason + "!");
//        }
//        public void SetRoute(Route route)
//        {
//            if (route == null)
//                throw new System.ArgumentNullException("route is null");
//            this.mainRoute = route;
//        }
//        public void Translate(Vector3 direction)
//        {
//            transform.Translate(direction, Space.World);
//        }

//        public abstract void Stop();
//        protected abstract void _Start();
//        protected abstract void _Update();
//    }
//}
//*/

