using System.Collections.Generic;
using UnityEngine;
using Industry.World;
using Industry.World.Buildings;
using Industry.World.Map;
using Industry.UI.Elements;
using Industry.World.Roads;

namespace Industry.Managers
{
    public static class BuildingManager
    {
        private static int rotation = 0;
        private static bool rotated = false;

        private static Vector3 mouseTilePosition;
        private static Building building;
        private static Building chosen = Objects.GameObjects.factory2x2;

        public static bool Enabled
        {
            get; private set;
        }


        public static void Update()
        {
            if (Enabled)
            {
                SetBuilding();
                MoveBuilding();
            }
        }
        
        public static void Enable()
        {
            if (!Enabled)
                Enabled = true;
        }

        /// <summary>
        /// Откатывает состояние строительства в начальное и деактивирует этот BuildingManager.
        /// </summary>
        private static void Cancel()
        {
            if (building != null)
                Object.Destroy(building.gameObject);
            building = null;

            rotated = false;
            Enabled = false;

            ToolBar.Components.EnableButtons();
        }

        private static bool CanPlace(Tile[,] tiles)
        {
            /*
            EntranceBuilding eb = (building as EntranceBuilding);

            for (int i = 0; i < building.countZ; i++)
            {
                for (int j = 0; j < building.countX; j++)
                {
                    if (tiles[i, j] != null)
                    {
                        if (eb != null)
                        {
                            Road road = Extract(tiles[i, j]);
                            if (road != null)
                            {
                                if (eb.entrance.type == road.type && eb.entrance.direction == road.direction)
                                {
                                    continue;
                                }
                                else return false;
                            }
                            else return false;
                        }
                        else return false;
                    }
                }
            }
            */
            return true;
        }

        private static void CheckRotation()
        {
            if (!rotated)
                Rotate();

            bool rotateClockWise = Input.GetKeyDown(KeyCode.E), rotateAntiClockWise = Input.GetKeyDown(KeyCode.Q);

            if ((rotateClockWise && rotateAntiClockWise) ||
                (!rotateClockWise && !rotateAntiClockWise)) { }
            else if (rotateClockWise)
            {
                building.Rotate(true, building.GetType());
                rotation++;
            }
            else if (rotateAntiClockWise)
            {
                building.Rotate(false, building.GetType());
                rotation--;
            }
        }

        private static Road Extract(Tile tile)
        {
            if (tile.Owner == null) return null;

            if (tile.Owner.GetType() == typeof(Road))
            {
                return tile.Owner as Road;
            }
            if (tile.Owner.GetType().IsSubclassOf(typeof(EntranceBuilding)))
            {
                Road entrance = (tile.Owner as EntranceBuilding).entrance;
                if (Mathf.Approximately(entrance.Position.x, tile.Position.x) && Mathf.Approximately(entrance.Position.z, tile.Position.z))
                    return entrance;
            }

            return null;
        }

        private static void MoveBuilding()
        {
            if (Input.GetMouseButtonDown(1)) { Cancel(); return; } // ПКМ, отмена

            try { mouseTilePosition = WorldMap.GetTile().Position; }
            catch (System.ArgumentOutOfRangeException) { return; }
            
            if (building == null)
            {
                building = Spawn(chosen, mouseTilePosition);

                return;
            }
            else if (building.code != chosen.code)
            {
                Object.Destroy(building.gameObject);
                building = Spawn(chosen, mouseTilePosition);

                return;
            }
            
            CheckRotation();

            building.SetPosition(mouseTilePosition);

            Tile[,] tiles = WorldMap.GetTilesInArea(building.UpLeft, building.countX, building.countZ);
            
            if (CanPlace(tiles))
            {
                building.SetColor(BuildingColor.Default);

                if (Input.GetMouseButtonDown(0)) // ЛКМ - установка постройки
                {
                    Place(tiles, Input.GetKey(KeyCode.LeftShift));
                }
            }
            else
            {
                building.SetColor(BuildingColor.Red);
            }
        }

        private static void Place(Tile[,] tiles, bool further_placement = false)
        {
            EntranceBuilding eb = building as EntranceBuilding;

            for (int i = 0; i < building.countZ; i++)
            {
                for (int j = 0; j < building.countX; j++)
                {
                    Road road = Extract(tiles[i, j]);
                    if (road != null && eb != null)
                    {
                        if (road.north != null) { eb.entrance.north = road.north; road.north.south = eb.entrance; }
                        if (road.south != null) { eb.entrance.south = road.south; road.south.north = eb.entrance; }
                        if (road.east  != null) { eb.entrance.east  = road.east;  road.east.west   = eb.entrance; }
                        if (road.west  != null) { eb.entrance.west  = road.west;  road.west.east   = eb.entrance; }

                        Object.Destroy(road.gameObject);
                    }

                    tiles[i, j].Owner = building;
                }
            }

            if (building.GetType() == typeof(Depot))
                building.transform.position = new Vector3(building.transform.position.x, -0.0825f, building.transform.position.z);

            building = null;
            rotated = false;

            //Debug.Log("MTPos = " + mouseTilePosition);

            if (!further_placement)
            {
                Enabled = false;
                ToolBar.Components.EnableButtons();
            }
        }

        private static void Rotate()
        {
            rotation = rotation % 4;
            if (rotation == 0)
            {
                //EntranceBuilding eB = building as EntranceBuilding;
                //if (eB != null) eB.entrance.direction = RoadDirection.South;
            }
            else if (rotation > 0)
            {
                for (int i = 0; i < rotation; i++)
                    building.Rotate(true, building.GetType());
            }
            else
            {
                for (int i = 0; i < -rotation; i++)
                    building.Rotate(false, building.GetType());
            }
            
            rotated = true;
        }

        private static void SetBuilding()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                chosen = Objects.GameObjects.factory2x2;
                return;
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                chosen = Objects.GameObjects.factory3x2;
                return;
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                chosen = Objects.GameObjects.factory3x3;
                return;
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                chosen = Objects.GameObjects.factory4x2;
                return;
            }
            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                chosen = Objects.GameObjects.factory4x3;
                return;
            }
            if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                chosen = Objects.GameObjects.factory4x4;
                return;
            }
            if (Input.GetKeyDown(KeyCode.Alpha7))
            {
                chosen = Objects.GameObjects.TruckDepot;
                return;
            }
        }
        
        private static Building Spawn(Building building, Vector3 pos)
        {
            rotated = false;
            return Object.Instantiate(building, pos, Quaternion.Euler(90, 0, 0));
        }


        public static void AutoCreate(Building b, Vector3 pos, int rot)
        {
            building = Spawn(b, pos);

            rotated = false;
            rotation = rot;
            Rotate();

            building.SetPosition(pos);

            Tile[,] tiles = WorldMap.GetTilesInArea(building.UpLeft, building.countX, building.countZ);

            Place(tiles);
        }
    }
}
