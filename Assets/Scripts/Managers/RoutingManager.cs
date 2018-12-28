using UnityEngine;
using Industry.AI.Routing;
using Industry.World.Buildings;
using Industry.World.Map;
using Industry.World.Roads;
using Industry.World;
using Industry.World.Vehicles;
using Industry.UI.Elements;

namespace Industry.Managers
{
    public static class RoutingManager
    {
        private static Road from;
        private static Road to;
        private static Route last_route;
        
        public static bool Enabled
        {
            get; private set;
        }

        private static void Disable()
        {
            Enabled = false;
            
            if (last_route != null)
            {
                last_route.HighLight(false);
                
                RouteSet.AddRoute(last_route);

                last_route = null;
            }

            from = null;
            to = null;

            ToolBar.Components.EnableButtons();
        }
        private static void ShowPath()
        {
            var roads = RouteCreator.CreatePathWP(from, to);

            if (roads != null)
            {
                last_route = new Route(roads);
                last_route.HighLight(true);
            }
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
                if (Input.GetMouseButtonDown(1)) { Disable(); return; } // ПКМ, отмена

                if (Input.GetMouseButtonDown(0))
                {
                    Tile tile = null;

                    try { tile = WorldMap.GetTile(); }
                    catch (System.ArgumentOutOfRangeException) { return; }

                    UnityEngine.Object owner = tile.Owner;

                    if (owner != null && (owner.GetType().IsSubclassOf(typeof(EntranceBuilding)) || (owner.GetType() == typeof(Road) && (owner as Road).type == RoadType.End)))
                    {
                        Road entrance = null;
                        try
                        {
                            entrance = (owner as EntranceBuilding).entrance;
                        }
                        catch (System.NullReferenceException)
                        {
                            entrance = owner as Road;
                        }

                        if (from == null)
                        {
                            from = entrance;
                            if (last_route != null)
                                last_route.HighLight(false);
                        }
                        else if (to == null)
                        {
                            to = entrance;

                            ShowPath();

                            from = null;
                            to = null;
                        }
                    }
                }
            }
        }
    }
}
