using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Industry.World.Roads;

namespace Industry.AI.Routing
{
    public class WayPoint : MonoBehaviour
    {
        public bool onEdge;
        public WayPoint[] neighbours;

        public Road road
        {
            get; private set;
        }
        public WayPointContainer wpContainer
        {
            get; private set;
        }

        public Vector3 Position
        {
            get { return transform.position; }
            set { transform.position = value; }
        }

        void Awake()
        {
            road = GetComponentInParent<Road>();
            wpContainer = GetComponentInParent<WayPointContainer>();

            if (road == null)
                throw new MissingComponentException("Road not found in parent");
            if (wpContainer == null)
                throw new MissingComponentException("WPContainer not found in parent");

            enabled = false;
        }
    }
}
