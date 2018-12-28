using System;
using System.IO;
using UnityEngine;
using Industry.World;
using Industry.World.Roads;
using Industry.World.Buildings;

namespace Industry.Managers
{
    public class SpriteLoader
    {
        public static Sprite GetRoad(RoadType type, RoadColor color)
        {
            RoadSprites R = Objects.GameObjects.roadSprites;

            switch (type)
            {
                case RoadType.Straight:
                    if (color == RoadColor.Default) return R.Road_D;
                    else if (color == RoadColor.Red) return R.Road_R;
                    else if (color == RoadColor.Transparent) return R.Road_T;
                        break;

                case RoadType.End:
                    if (color == RoadColor.Default) return R.End_D;
                    else if (color == RoadColor.Red) return R.End_R;
                    else if (color == RoadColor.Transparent) return R.End_T;
                        break;

                case RoadType.Corner:
                    if (color == RoadColor.Default) return R.Corner_D;
                    else if (color == RoadColor.Red) return R.Corner_R;
                    else if (color == RoadColor.Transparent) return R.Corner_T;
                    break;

                case RoadType.Cross_T:
                    if (color == RoadColor.Default) return R.CrossT_D;
                    else if (color == RoadColor.Red) return R.CrossT_R;
                    else if (color == RoadColor.Transparent) return R.CrossT_T;
                    break;

                case RoadType.Cross_X:
                    if (color == RoadColor.Default) return R.CrossX_D;
                    else if (color == RoadColor.Red) return R.CrossX_R;
                    else if (color == RoadColor.Transparent) return R.CrossX_T;
                    break;
                case RoadType.Single:
                    if (color == RoadColor.Default) return R.Single_D;
                    else if (color == RoadColor.Red) return R.Single_R;
                    else if (color == RoadColor.Transparent) return R.Single_T;
                    break;
            }

            throw new Exception("Sprite Loading Error.");
        }

        public static Sprite GetBuilding(BuildingColor color)
        {
            BuildingSprites B = Objects.GameObjects.buildingSprites;

            if (color == BuildingColor.Default) return B.Default;
            else if (color == BuildingColor.Red) return B.Red;

            throw new Exception("Sprite Loading Error.");
        }
    }
}
