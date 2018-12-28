using System.Collections;
using UnityEngine;
using Industry.AI.Routing;
using Industry.Managers;
using Industry.World.Buildings;
using Industry.World.Map;
using Industry.World.Roads;
using Industry.World.Vehicles;
using Industry.UI.Elements;
using Industry.Utilities;

namespace Industry.World
{
    public class Objects : MonoBehaviour
    {
        private Objects() { }

        #region Backgrounds

        #endregion
        #region Roads

        public RoadSprites roadSprites;
        public Road roadStraight;
        public Road roadEnd;
        public Road corner;
        public Road cross_T;
        public Road cross_X;
        public Road single;

        public WayPointContainer wpc_straight;
        public WayPointContainer wpc_corner;
        public WayPointContainer wpc_end;
        public WayPointContainer wpc_crossT;
        public WayPointContainer wpc_crossX;
        public WayPointContainer wpc_single;

        #endregion
        #region Vehicles

        public Truck truck;

        #endregion
        #region Buildings
        public BuildingSprites buildingSprites;
        
        public Factory factory2x2;
        public Factory factory3x2;
        public Factory factory3x3;
        public Factory factory4x2;
        public Factory factory4x3;
        public Factory factory4x4;
        public Depot TruckDepot;

        #endregion

        public ToolBar toolbar;

        public static Objects GameObjects
        {
            get; private set;
        }



        void Start()
        {
            GameObjects = this;
            WorldMap.Initialize();
            
            StartCoroutine(RouteSet.CheckRoutes());
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.B))
            {
                ToolBar.IsShown = !ToolBar.IsShown;
            }
        }

        private IEnumerator ReDrawRoutes()
        {
            while (true)
            {
                RouteSet.HighLightAll(false);
                GizmosDrawer.Clear();
                RouteSet.HighLightAll(true);

                yield return new WaitForSeconds(2);
            }
        }
    }
}
