using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Industry.Managers;
using Industry.World.Buildings;
using Industry.World.Roads;
using Industry.World.Map;
using Industry.World;

namespace Industry.Utilities
{
    public class TestingStuff : MonoBehaviour
    {
        void Start()
        {
            
        }

        void Update()
        {
            PlaceRoads();




            enabled = false;
        }

        private void PlaceRoads()
        {
            RoadManager.AutoCreate(new Vector3(-54.4f, 0.02f, 35.2f), new Vector3(  48f, 0.02f, 35.2f), true);
            RoadManager.AutoCreate(new Vector3(-54.4f, 0.02f,   16f), new Vector3(  48f, 0.02f,   16f), true);
            RoadManager.AutoCreate(new Vector3(-54.4f, 0.02f,  3.2f), new Vector3(  48f, 0.02f,  3.2f), true);
            RoadManager.AutoCreate(new Vector3(-41.6f, 0.02f, 35.2f), new Vector3(-41.6f, 0.02f, 3.2f), false);
            RoadManager.AutoCreate(new Vector3(-28.8f, 0.02f, 35.2f), new Vector3(-28.8f, 0.02f, 3.2f), false);
            RoadManager.AutoCreate(new Vector3(  -16f, 0.02f, 35.2f), new Vector3(  -16f, 0.02f, 3.2f), false);
            RoadManager.AutoCreate(new Vector3( -3.2f, 0.02f, 35.2f), new Vector3( -3.2f, 0.02f, 3.2f), false);
            RoadManager.AutoCreate(new Vector3(  9.6f, 0.02f, 35.2f), new Vector3(  9.6f, 0.02f, 3.2f), false);
            RoadManager.AutoCreate(new Vector3( 22.4f, 0.02f, 35.2f), new Vector3( 22.4f, 0.02f, 3.2f), false);
            RoadManager.AutoCreate(new Vector3( 35.2f, 0.02f, 35.2f), new Vector3( 35.2f, 0.02f, 3.2f), false);
            
        }
    }
}