using UnityEngine;
using Industry.AI.Routing;
using Industry.World;
using Industry.World.Buildings;
using Industry.World.Map;
using Industry.World.Roads;
using Industry.UI.Elements;
using Industry.Utilities;

namespace Industry.Managers
{
    public static class DestructionManager
    {
        public static bool Enabled
        {
            get; private set;
        }
        
        public static void Enable()
        {
            if (!Enabled)
                Enabled = true;
        }
        public static void Update()
        {
            if (Enabled)
            {
                if      (Input.GetMouseButtonDown(1)) { Cancel(); return; } // ПКМ, отмена
                else if (Input.GetMouseButtonDown(0) || Input.GetMouseButton(0)) { DestroyObject(); }
            }
        }

        private static void Cancel()
        {
            Enabled = false;
            ToolBar.Components.EnableButtons();
        }
        private static void DestroyObject()
        {
            Tile tile = null;

            try { tile = WorldMap.GetTile(); }
            catch (System.ArgumentOutOfRangeException) { return; }

            if (tile.Owner != null)
            {
                System.Type ownerType = tile.Owner.GetType();

                if (ownerType == typeof(Road))
                {
                    Road road = tile.Owner as Road;
                    DestroyRoad(road);
                }
                else if (ownerType.IsSubclassOf(typeof(Building)))
                {
                    Building building = tile.Owner as Building;
                    DestroyBuilding(building);
                }
            }
        }
        private static void DestroyRoad(Road road)
        {
            WorldMap.ClearTile(road.transform.position);
            Object.Destroy(road.gameObject);
            
            Road north = road.north;
            Road south = road.south;
            Road west  = road.west;
            Road east  = road.east;

            //Timer timer = new Timer();
            //timer.Start();

            switch (road.type)
            {
                case RoadType.Corner:

                    switch (road.direction)
                    {
                        case RoadDirection.NorthWest:

                            SetNorth(north);
                            SetWest(west);
                            
                            break;

                        case RoadDirection.NorthEast:

                            SetNorth(north);
                            SetEast(east);

                            break;

                        case RoadDirection.SouthWest:

                            SetSouth(south);
                            SetWest(west);
                            
                            break;

                        case RoadDirection.SouthEast:

                            SetSouth(south);
                            SetEast(east);
                            
                            break;
                    }
                    break;

                case RoadType.Straight:

                    switch (road.direction)
                    {
                        case RoadDirection.Horizontal:

                            SetWest(west);
                            SetEast(east);

                            break;

                        case RoadDirection.Vertical:

                            SetNorth(north);
                            SetSouth(south);

                            break;
                    }
                    break;

                case RoadType.End:

                    switch (road.direction)
                    {
                        case RoadDirection.North:

                            SetNorth(north);
                            
                            break;

                        case RoadDirection.South:
                            
                            SetSouth(south);

                            break;

                        case RoadDirection.West:
                            
                            SetWest(west);

                            break;

                        case RoadDirection.East:
                            
                            SetEast(east);

                            break;
                    }
                    break;

                case RoadType.Cross_T:

                    switch (road.direction)
                    {
                        case RoadDirection.North:

                            SetNorth(north);
                            SetWest(west);
                            SetEast(east);

                            break;

                        case RoadDirection.South:
                            
                            SetSouth(south);
                            SetWest(west);
                            SetEast(east);

                            break;

                        case RoadDirection.West:

                            SetNorth(north);
                            SetSouth(south);
                            SetWest(west);

                            break;

                        case RoadDirection.East:

                            SetNorth(north);
                            SetSouth(south);
                            SetEast(east);

                            break;
                    }
                    break;

                case RoadType.Cross_X:

                    SetNorth(north);
                    SetSouth(south);
                    SetWest(west);
                    SetEast(east);

                    break;
            }

            //DelayedOpsManager.AddOperation(() => { RouteSetWP.RecalculateRoutes(road); });
            //RouteSetWP.RecalculateRoutes(road);

            //Debug.Log("<color=orange>Routes Recalculation:</color> " + timer.ElapsedTime(Timer.Units.Milliseconds) + " ms.");
        }
        private static void DestroyBuilding(Building building)
        {
            if (building.GetType().IsSubclassOf(typeof(EntranceBuilding)))
            {
                EntranceBuilding entrBuilding = building as EntranceBuilding;

                Road entrance = entrBuilding.entrance;

                if (entrance.north != null || entrance.south != null || entrance.east != null || entrance.west != null)
                    DestroyRoad(entrance);
            }

            WorldMap.ClearArea(building.UpLeft, building.countX, building.countZ);
            Object.Destroy(building.gameObject);
        }
        
        private static void SetEast(Road east)
        {
            east.west = null;
            //road.east = null;

            switch (east.type)
            {
                case RoadType.Corner:   east.SetTypeAndDirection(RoadType.End, east.direction == RoadDirection.NorthWest ? RoadDirection.North : RoadDirection.South); break;
                case RoadType.Straight: east.SetTypeAndDirection(RoadType.End, RoadDirection.East); break;
                case RoadType.End:
                    if (!WorldMap.IsOwnerTypeOf(typeof(EntranceBuilding), east.Position))
                    {
                        WorldMap.ClearTile(east.Position);
                        Object.Destroy(east.gameObject);
                    }
                    break;
                case RoadType.Cross_T:
                    if (east.direction == RoadDirection.North)
                        east.SetTypeAndDirection(RoadType.Corner, RoadDirection.NorthEast);
                    else if (east.direction == RoadDirection.South)
                        east.SetTypeAndDirection(RoadType.Corner, RoadDirection.SouthEast);
                    else
                        east.SetTypeAndDirection(RoadType.Straight, RoadDirection.Vertical);
                    break;
                case RoadType.Cross_X: east.SetTypeAndDirection(RoadType.Cross_T, RoadDirection.East); break;
            }

            //DelayedOpsManager.AddOperation(() => { RouteSetWP.RecalculateRoutes(east); });
            //RouteSetWP.RecalculateRoutes(east);
        }
        private static void SetNorth(Road north)
        {
            north.south = null;
            //road.north  = null;

            switch (north.type)
            {
                case RoadType.Corner: north.SetTypeAndDirection(RoadType.End, north.direction == RoadDirection.SouthWest ? RoadDirection.West : RoadDirection.East); break;
                case RoadType.Straight: north.SetTypeAndDirection(RoadType.End, RoadDirection.North); break;
                case RoadType.End:
                    if (!WorldMap.IsOwnerTypeOf(typeof(EntranceBuilding), north.Position))
                    {
                        WorldMap.ClearTile(north.Position);
                        Object.Destroy(north.gameObject);
                    }
                    break;
                case RoadType.Cross_T:
                    if (north.direction == RoadDirection.South)
                        north.SetTypeAndDirection(RoadType.Straight, RoadDirection.Horizontal);
                    else if (north.direction == RoadDirection.West)
                        north.SetTypeAndDirection(RoadType.Corner, RoadDirection.NorthWest);
                    else
                        north.SetTypeAndDirection(RoadType.Corner, RoadDirection.NorthEast);
                    break;
                case RoadType.Cross_X: north.SetTypeAndDirection(RoadType.Cross_T, RoadDirection.North); break;
            }

            //DelayedOpsManager.AddOperation(() => { RouteSetWP.RecalculateRoutes(north); });
            //RouteSetWP.RecalculateRoutes(north);
        }
        private static void SetSouth(Road south)
        {
            south.north = null;
            //road.south = null;

            switch (south.type)
            {
                case RoadType.Corner:   south.SetTypeAndDirection(RoadType.End, south.direction == RoadDirection.NorthWest ? RoadDirection.West : RoadDirection.East); break;
                case RoadType.Straight: south.SetTypeAndDirection(RoadType.End, RoadDirection.South); break;
                case RoadType.End:
                    if (!WorldMap.IsOwnerTypeOf(typeof(EntranceBuilding), south.Position))
                    {
                        WorldMap.ClearTile(south.Position);
                        Object.Destroy(south.gameObject);
                    }
                    break;
                case RoadType.Cross_T:
                    if (south.direction == RoadDirection.North)
                        south.SetTypeAndDirection(RoadType.Straight, RoadDirection.Horizontal);
                    else if (south.direction == RoadDirection.West)
                        south.SetTypeAndDirection(RoadType.Corner, RoadDirection.SouthWest);
                    else
                        south.SetTypeAndDirection(RoadType.Corner, RoadDirection.SouthEast);
                    break;
                case RoadType.Cross_X: south.SetTypeAndDirection(RoadType.Cross_T, RoadDirection.South); break;
            }

            //DelayedOpsManager.AddOperation(() => { RouteSetWP.RecalculateRoutes(south); });
            //RouteSetWP.RecalculateRoutes(south);
        }
        private static void SetWest(Road west)
        {
            west.east = null;
            //road.west = null;

            switch (west.type)
            {
                case RoadType.Corner:   west.SetTypeAndDirection(RoadType.End, west.direction == RoadDirection.NorthEast ? RoadDirection.North : RoadDirection.South); break;
                case RoadType.Straight: west.SetTypeAndDirection(RoadType.End, RoadDirection.West); break;
                case RoadType.End:
                    if (!WorldMap.IsOwnerTypeOf(typeof(EntranceBuilding), west.Position))
                    {
                        WorldMap.ClearTile(west.Position);
                        Object.Destroy(west.gameObject);
                    }
                    break;
                case RoadType.Cross_T:
                    if (west.direction == RoadDirection.North)
                        west.SetTypeAndDirection(RoadType.Corner, RoadDirection.NorthWest);
                    else if (west.direction == RoadDirection.South)
                        west.SetTypeAndDirection(RoadType.Corner, RoadDirection.SouthWest);
                    else
                        west.SetTypeAndDirection(RoadType.Straight, RoadDirection.Vertical);
                    break;
                case RoadType.Cross_X: west.SetTypeAndDirection(RoadType.Cross_T, RoadDirection.West); break;
            }

            //DelayedOpsManager.AddOperation(() => { RouteSetWP.RecalculateRoutes(west); });
            //RouteSetWP.RecalculateRoutes(west);
        }       
    }
}
