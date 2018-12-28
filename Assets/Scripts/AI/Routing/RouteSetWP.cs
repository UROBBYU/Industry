using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Industry.World;
using Industry.World.Buildings;
using Industry.World.Roads;
using Industry.World.Vehicles;
using Industry.Utilities;

namespace Industry.AI.Routing
{
    public static class RouteSetWP
    {
        private static List<RouteWP> routes = new List<RouteWP>(16);

        public static bool checking = true;
        public static int Count
        {
            get { return routes.Count; }
        }


        //public static void AddRoute(RouteWP route)
        //{
        //    if (route != null && route.Count > 1)
        //    {
        //        /*
        //        Depot[] depots = Object.FindObjectsOfType<Depot>();
        //        float[] dists = new float[depots.Length];

        //        for (int i = 0; i < depots.Length; i++)
        //        {
        //            dists[i] = Vector3.Distance(route[route.Count / 2].transform.position, depots[i].entrance.transform.position);
        //        }
        //        */

        //        Depot[] depots = Object.FindObjectsOfType<Depot>();

        //        if (depots.Length > 0)
        //        {
        //            routes.Add(route);

        //            Vector3 pos = depots[0].entrance.transform.position; pos.y = 1.5f;
        //            Truck truck = UnityEngine.Object.Instantiate(Objects.GameObjects.truck, pos, Quaternion.identity);
        //            //truck.SetRoute(route);
        //            //route.AddVehicle(truck);
        //        }


        //    }
        //}

        public static void AddRoute(RouteWP route, bool createTruck = true)
        {
            if (createTruck)
            {
                Vector3 pos = route.startPoint; pos.y = 1.5f;

                Truck truck = UnityEngine.Object.Instantiate(Objects.GameObjects.truck, pos, Quaternion.identity);
                truck.SetRoute(route);

                route.AddVehicle(truck);
            }

            routes.Add(route);
        }
    
        public static void RecalculateRoutes(Road changed = null)
        {
            if (Count < 1) return;
            
            if (changed == null)
            {
                for (int i = 0; i < Count; i++)
                {

                    RouteWP routesI = routes[i];
                    routesI.HighLight(false);

                    Debug.Log("Recalculation: Route " + routesI.RouteID + ", temp - " + routesI.IsTemp);
                    
                    var newRoute = RouteCreator.CreatePathWP(routesI.startWP, routesI.endWP, !routesI.IsTemp);
                    routesI.Set(newRoute);

                    routesI.HighLight(true, routesI.IsTemp ? Color.red : Color.green);
                }
                return;
            }
            else
            {
                for (int i = 0; i < Count; i++)
                {

                    RouteWP routesI = routes[i];
                    
                    if (routesI.Contains(changed))
                    {
                        Debug.Log("Recalculation: " + changed.ToString());

                        if (routesI[0] == changed || routesI[routesI.Count / 2] == changed)
                        {
                            Debug.Log("<color=red>Route " + routesI.RouteID + ": Begin or End was deleted!</color>");
                            routesI.Set(null); // ToImplement
                        }
                        else
                        {
                            var newRoute = RouteCreator.CreatePathWP(routesI.startWP, routesI.endWP, !routesI.IsTemp);

                            if (newRoute == null)
                            { }


                            if (newRoute == null)
                                Debug.Log("<color=red>Route " + routesI.RouteID + ": Path does not exist anymore!</color>");
                            else
                                Debug.Log("<color=green>Route " + routesI.RouteID + ": Path recalculated successfully.</color>");
                            
                            routesI.HighLight(false);
                            routesI.Set(newRoute);
                            routesI.HighLight(true, routesI.IsTemp ? Color.red : Color.green);
                        }
                    }
                }
            }
        }
        
        public static void RemoveRoute(RouteWP route)
        {
            Object.Destroy(route.s1);
            Object.Destroy(route.s2);

            route.HighLight(false);
            //route.SetRouteToAllVehicles(null);
            route.RemoveAllVehicles();
            routes.Remove(route);
        }

        public static void HighLightAll(bool on)
        {
            for (int i = 0; i < Count; i++)
            {
                try { routes[i].HighLight(on); }
                catch (MissingReferenceException) { }
            }
        }

        public static IEnumerator CheckRoutes()
        {
            while (checking)
            {
                if (routes.Count > 0)
                {
                    Timer timer = new Timer();
                    timer.Start();
                    
                    for (int i = 0; i < routes.Count; i++)
                    {
                        RouteWP route = routes[i];

                        int curr = 0, last = route.Count - 1;

                        while (curr != last)
                        {
                            if (route[curr] == null)
                            {
                                Timer t = new Timer();
                                t.Start();
                                
                                route.UpdateTempStartWP();

                                var newRoute = RouteCreator.CreatePathWP(route.startWP, route.endWP, !route.IsTemp);

                                route.Set(newRoute);

                                Debug.Log("<color=orange>Coroutine route " + route.RouteID + " recalculation:</color><color=green> " + t.ElapsedTime(Timer.Units.Milliseconds) + " ms.</color>");

                                break;
                                //yield return null;
                            }
                            else
                            {
                                curr++;
                            }
                        }
                    }

                    Debug.Log("Coroutine routes recalculation:<color=green> " + timer.ElapsedTime(Timer.Units.Milliseconds) + " ms.</color>");
                }

                HighLightAll(false);
                GizmosDrawer.Clear();
                HighLightAll(true);

                yield return new WaitUntil(() => { return Time.frameCount % 10 == 0; });
            }
        }
        
    }
}
