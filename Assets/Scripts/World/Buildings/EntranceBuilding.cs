using System;
using System.Collections.Generic;
using UnityEngine;
using Industry.Managers;
using Industry.World.Roads;

namespace Industry.World.Buildings
{
    public abstract class EntranceBuilding : Building
    {
        public Road entrance;
        public Vector3 EntrancePos
        {
            get { return entrance.transform.position; }
        }

        public override void SetColor(BuildingColor color)
        {
            if (this.color != color)
            {
                sprite = SpriteLoader.GetBuilding(color);

                RoadColor col;
                switch (color)
                {
                    case BuildingColor.Default: col = RoadColor.Default; break;
                    case BuildingColor.Red: col = RoadColor.Red; break;
                    default: col = RoadColor.Default; break;
                }

                entrance.SetColor(col);
                this.color = color;
            }
        }

        public override void Rotate(bool clockwise, Type building)
        {
            base.Rotate(clockwise, building);
            
            switch (entrance.direction)
            {
                case RoadDirection.North: entrance.direction = clockwise ? RoadDirection.East  : RoadDirection.West;  break;
                case RoadDirection.South: entrance.direction = clockwise ? RoadDirection.West  : RoadDirection.East;  break;
                case RoadDirection.West:  entrance.direction = clockwise ? RoadDirection.North : RoadDirection.South; break;
                case RoadDirection.East:  entrance.direction = clockwise ? RoadDirection.South : RoadDirection.North; break;
            }
        }

        protected override void _Awake()
        {
            entrance.direction = RoadDirection.South;
        }
        protected override void _Start()
        {
            //entrance.SetType(RoadType.End);
            //entrance.SetDirection(RoadDirection.Backward);
            entrance.direction = RoadDirection.South;
            entrance.transform.localRotation = Quaternion.Euler(0, 0, 0);
            entrance.gameObject.name = "Entrance: " + gameObject.name;
        }
        protected override void _Update()
        {

        }

        public static implicit operator Road(EntranceBuilding eBuilding)
        {
            return eBuilding.entrance;
        }
    }
}
