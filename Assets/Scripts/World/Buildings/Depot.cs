using System;
using System.Collections.Generic;
using UnityEngine;
using Industry.World.Roads;

namespace Industry.World.Buildings
{
    public class Depot : EntranceBuilding
    {
        private Color defaultCol;
        private Material material;

        public int cost;
        

        protected override void _Start()
        {
            base._Start();
            transform.rotation = Quaternion.Euler(270, 180, 0);

            material = GetComponentInChildren<MeshRenderer>().material;
            defaultCol = material.color;
        }
        protected override void _Update()
        {
            base._Update();


        }

        public override void SetColor(BuildingColor color)
        {
            if (this.color != color)
            {
                RoadColor col;
                switch (color)
                {
                    case BuildingColor.Default: material.color = defaultCol; col = RoadColor.Default; break;
                    case BuildingColor.Red:     material.color = Color.red;  col = RoadColor.Red;     break;
                    default:                    material.color = defaultCol; col = RoadColor.Default; break;
                }

                entrance.SetColor(col);
                this.color = color;
            }
        }
    }
}
