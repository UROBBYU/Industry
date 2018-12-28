using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Industry.World.Roads;

namespace Industry.AI.Routing
{
    public class WayPointContainer : MonoBehaviour
    {
        public bool draw_lines;

        public int WPCount
        {
            get { return wayPoints.Length; }
        }
        private WayPoint[] wayPoints;
        
        void Awake()
        {
            wayPoints = GetComponentsInChildren<WayPoint>();

            if (transform.parent != null)
                gameObject.name = transform.parent.gameObject.name + " WPC: " + wayPoints.Length;

            enabled = false;
        }
        
        void OnDrawGizmos()
        {
            if (draw_lines && wayPoints != null)
                foreach (WayPoint WP in wayPoints)
                    foreach (WayPoint wp in WP.neighbours)
                        if (wp != null)
                            Gizmos.DrawLine(WP.Position, wp.Position);
        }
        
        public void LinkWayPoints(Road other)
        {
            WayPoint[] thisEdgeWPs = wayPoints.Where((wp) => wp.onEdge).ToArray();
            WayPoint[] otherEdgeWPs = other.WPContainer.wayPoints.Where((wp) => wp.onEdge).ToArray();
            
            for (int i = 0; i < thisEdgeWPs.Length; i++)
            {
                Vector3 thisPos = thisEdgeWPs[i].transform.position;

                for (int j = 0; j < otherEdgeWPs.Length; j++)
                {
                    Vector3 otherPos = otherEdgeWPs[j].transform.position;

                    if (Vector3.Distance(thisPos, otherPos) < 0.1f)
                    {
                        WayPoint[] thisNeighbours = thisEdgeWPs[i].neighbours;
                        WayPoint[] otherNeighbours = otherEdgeWPs[j].neighbours;

                        if (thisNeighbours. Length == 1)
                            thisNeighbours [0] = otherEdgeWPs[j];
                        if (otherNeighbours.Length == 1)
                            otherNeighbours[0] = thisEdgeWPs[i];


                        break;
                    }
                }
            }
        }

        public WayPoint Get(Func<WayPoint, bool> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException("predicate");
            
            WayPoint[] WPs = wayPoints.Where(predicate).ToArray();

            return WPs.Length > 0 ? WPs[0] : null;
        }

        public WayPoint GetClosestWP(Vector3 from)
        {
            WayPoint closest = wayPoints[0];
            
            float min = Vector3.Distance(closest.Position, from);
            
            for (int i = 1; i < wayPoints.Length; i++)
            {
                float dist = Vector3.Distance(wayPoints[i].Position, from);

                if (dist < min)
                {
                    min = dist;
                    closest = wayPoints[i];
                }
            }
            
            return closest;
        }
        public WayPoint GetClosestWPExcept(Vector3 from, WayPoint ex_wp)
        {
            WayPoint closest = wayPoints[0];

            if (closest == ex_wp)
                return closest.neighbours[0];

            float min = Vector3.Distance(closest.Position, from);

            for (int i = 1; i < wayPoints.Length; i++)
            {
                float dist = Vector3.Distance(wayPoints[i].Position, from);

                if (dist < min)
                {
                    min = dist;

                    if (closest != ex_wp)
                        closest = wayPoints[i];
                }
            }

            return closest;
        }

        public WayPoint this[int index]
        {
            get
            {
                return wayPoints[index];
            }
        }
    }
}
