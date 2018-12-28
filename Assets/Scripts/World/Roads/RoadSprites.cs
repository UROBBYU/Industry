using System;
using UnityEngine;

namespace Industry.World.Roads
{
    public class RoadSprites : MonoBehaviour
    {
        public Sprite Road_D,   Road_R,   Road_T;
        public Sprite End_D,    End_R,    End_T;
        public Sprite Corner_D, Corner_R, Corner_T;
        public Sprite CrossX_D, CrossX_R, CrossX_T;
        public Sprite CrossT_D, CrossT_R, CrossT_T;
        public Sprite Single_D, Single_R, Single_T;

        public void Start()
        {
            enabled = false;
        }
    }
}
