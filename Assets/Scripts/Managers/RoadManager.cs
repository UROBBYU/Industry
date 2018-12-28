using System.Collections.Generic;
using UnityEngine;
using Industry.World;
using Industry.World.Buildings;
using Industry.World.Map;
using Industry.World.Roads;
using Industry.UI.Elements;
using Industry.Utilities;
using Industry.AI.Routing;

namespace Industry.Managers
{
    public static class RoadManager
    {
        /// <summary>
        /// Содержит полную информацию о участках дорог в перекрестке, их взаимное расположение и тип перекрестка.
        /// </summary>
        private class RoadPair
        {
            public RoadPair(Road owner, Road road, RoadType type, RoadDirection ownerDir, CrossRoadCreationType cross_type)
            {
                this.owner = owner;
                this.road = road;
                this.type = type;
                this.ownerDir = ownerDir;
                this.cross_type = cross_type;
            }

            public Road owner;
            public Road road;
            public RoadType type;
            public RoadDirection ownerDir;
            public CrossRoadCreationType cross_type;
        }
        /// <summary>
        /// Перечень способов взаимного расположения участков дорог.
        /// </summary>
        private enum CrossRoadCreationType
        {
            RoadOnEnd, StraightOnStraight,
            CornerOnCorner, CornerOnEnd,
            EndOnRoad, EndOnEnd, EndOnCorner, EndOnCrossT,
            EndOnEntrance
        }

        /// <summary>
        /// Список участков дороги (дорожный список)
        /// </summary>
        private static RoadArray roads;
        /// <summary>
        /// Список пар участков дорог для перекрестка.
        /// </summary>
        private static List<RoadPair> crossroadsList;

        private static Timer timer = new Timer();
        private static Road last_entrance;
        private static Vector3 mouseTilePosition;
        private static Vector3 startPos, endPos, lastEndPos, offset;


        public static bool Enabled
        {
            get; private set;
        }
        private static int last
        {
            get { return roads.Length - 1; }
        }

        private static bool placeSwitch = true;
        private static bool end_placed = false;
        private static bool firstDirAssigned = false;
        private static bool X_first = false;

        public static void Update()
        {
            if (Enabled)
            {
                if (placeSwitch) Place();
                else Extend();
            }
        }

        public static void Enable()
        {
            if (!Enabled)
                Enabled = true;
        }

        /// <summary>
        /// Начинает строительство дороги с одиночного участка, устанавливает начало дороги и прочие координаты.
        /// </summary>
        private static void Place()
        {
            if (Input.GetMouseButtonDown(1)) { Cancel(); return; } // ПКМ, отмена

            try { mouseTilePosition = WorldMap.GetTile().Position; }
            catch (System.ArgumentOutOfRangeException) { return; }

            startPos = mouseTilePosition;
            lastEndPos = startPos;

            if (roads == null) // начало с одиночной дороги
            {
                roads = new RoadArray(1);
                roads.Add(Object.Instantiate(Objects.GameObjects.single, startPos, Quaternion.Euler(90, 0, 0)));
            }
            Road single = roads[0];
            single.SetSortingOrder(2);
            single.Position = startPos;

            UnityEngine.Object owner = WorldMap.GetTile(single.transform.position).Owner;

            if (owner != null)
            {
                if (owner.GetType() == typeof(Road))
                {
                    single.SetColor(RoadColor.Transparent);

                    if (Input.GetMouseButtonDown(0)) // ЛКМ - продолжение дороги
                    {
                        placeSwitch = false;
                    }
                }
                else
                {
                    single.SetSortingOrder(5);
                    single.SetColor(RoadColor.Red);
                    end_placed = false;
                }
            }
            else
            {
                single.SetColor(RoadColor.Transparent);

                if (Input.GetMouseButtonDown(0)) // ЛКМ - продолжение дороги
                {
                    placeSwitch = false;
                }
            }

        }

        /// <summary>
        /// Продолжает строительство дороги с начального участка, ожидая подтверждения (ЛКМ)
        /// </summary>
        private static void Extend()
        {
            if (Input.GetMouseButtonDown(1)) { Cancel(); return; } // ПКМ - отмена

            try { endPos = WorldMap.GetTile().Position; } // координаты последнего участка дороги
            catch (System.ArgumentOutOfRangeException) { return; }

            if (endPos != startPos && endPos != lastEndPos) // если конец не в начале и менялся
            {
                //timer.Start();
                PlaceRoad(); // перестроить дорогу
                //Debug.Log("PlaceRoad(): <color=yellow>" + timer.ElapsedTime(Timer.Units.Milliseconds) + " ms.</color>");
            }
            else if (roads == null || (endPos == startPos && last > 0)) // возвращение курсора на начальную клетку
            {
                ResetRoad();
                roads = new RoadArray(1);
                
                Road single = Object.Instantiate(Objects.GameObjects.single, mouseTilePosition, Quaternion.Euler(90, 0, 0));
                roads.Add(single);

                startPos = mouseTilePosition;
                lastEndPos = startPos;

                firstDirAssigned = false;
                end_placed = false;
            }

            if (end_placed)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    //timer.Start();
                    ApplyRoad(Input.GetKey(KeyCode.LeftShift));
                    //Debug.Log("ApplyRoad(): <color=green>" + timer.ElapsedTime(Timer.Units.Milliseconds) + " ms.</color>");
                }
            }
            else
            {
                for (int i = 0; i < roads.Length; i++)
                {
                    if (roads[i] != null)
                    {
                        roads[i].SetColor(RoadColor.Red);
                        roads[i].SetSortingOrder(5);
                    }
                }
            }
        }

        /// <summary>
        /// Переносит дорогу из дорожного списка на карту, строя перекрестки.
        /// </summary>
        /// <param name="further_extension">true, если после переноса дороги на карту нужно продолжить строительство с последнего участка.</param>
        private static void ApplyRoad(bool further_extension = false)
        {
            for (int i = 0; i < roads.Length; i++)
            {
                Road road = roads[i];

                #region Создание перекрестков
                foreach (RoadPair rp in crossroadsList)
                {
                    if (rp.road == road)
                    {
                        #region Cross_T
                        if (rp.type == RoadType.Cross_T)
                        {
                            if (rp.cross_type == CrossRoadCreationType.EndOnRoad)
                            {
                                switch (rp.ownerDir)
                                {
                                    case RoadDirection.North: rp.owner.north = rp.road.north; rp.road.north.south = rp.owner; break;
                                    case RoadDirection.South: rp.owner.south = rp.road.south; rp.road.south.north = rp.owner; break;
                                    case RoadDirection.East:  rp.owner.east  = rp.road.east;  rp.road.east.west   = rp.owner; break;
                                    case RoadDirection.West:  rp.owner.west  = rp.road.west;  rp.road.west.east   = rp.owner; break;
                                    default: throw new System.ArgumentException("in rp.ownerDir");
                                }
                            }
                            else if (rp.cross_type == CrossRoadCreationType.RoadOnEnd)
                            {
                                switch (rp.ownerDir)
                                {
                                    case RoadDirection.North:
                                        rp.owner.west = rp.road.west; rp.road.west.east = rp.owner;
                                        rp.owner.east = rp.road.east; rp.road.east.west = rp.owner;
                                        break;
                                    case RoadDirection.South:
                                        rp.owner.west = rp.road.west; rp.road.west.east = rp.owner;
                                        rp.owner.east = rp.road.east; rp.road.east.west = rp.owner;
                                        break;
                                    case RoadDirection.East:
                                        rp.owner.north = rp.road.north; rp.road.north.south = rp.owner;
                                        rp.owner.south = rp.road.south; rp.road.south.north = rp.owner;
                                        break;
                                    case RoadDirection.West:
                                        rp.owner.north = rp.road.north; rp.road.north.south = rp.owner;
                                        rp.owner.south = rp.road.south; rp.road.south.north = rp.owner;
                                        break;
                                    default: throw new System.ArgumentException("in rp.ownerDir");
                                }
                            }
                            else if (rp.cross_type == CrossRoadCreationType.CornerOnEnd)
                            {
                                RoadDirection roadDir = rp.road.direction;

                                switch (rp.owner.direction)
                                {
                                    case RoadDirection.North:
                                        if (roadDir == RoadDirection.SouthEast)
                                        {
                                            rp.owner.south = rp.road.south; rp.road.south.north = rp.owner;
                                            rp.owner.east  = rp.road.east;  rp.road.east.west   = rp.owner;
                                        }
                                        else if (roadDir == RoadDirection.SouthWest)
                                        {
                                            rp.owner.south = rp.road.south; rp.road.south.north = rp.owner;
                                            rp.owner.west  = rp.road.west;  rp.road.west.east   = rp.owner;
                                        }
                                        break;
                                    case RoadDirection.South:
                                        if (roadDir == RoadDirection.NorthEast)
                                        {
                                            rp.owner.north = rp.road.north; rp.road.north.south = rp.owner;
                                            rp.owner.east  = rp.road.east;  rp.road.east.west   = rp.owner;
                                        }
                                        else if (roadDir == RoadDirection.NorthWest)
                                        {
                                            rp.owner.north = rp.road.north; rp.road.north.south = rp.owner;
                                            rp.owner.west  = rp.road.west;  rp.road.west.east   = rp.owner;
                                        }
                                        break;
                                    case RoadDirection.West:
                                        if (roadDir == RoadDirection.NorthEast)
                                        {
                                            rp.owner.north = rp.road.north; rp.road.north.south = rp.owner;
                                            rp.owner.east  = rp.road.east;  rp.road.east.west   = rp.owner;
                                        }
                                        else if (roadDir == RoadDirection.SouthEast)
                                        {
                                            rp.owner.south = rp.road.south; rp.road.south.north = rp.owner;
                                            rp.owner.east  = rp.road.east;  rp.road.east.west   = rp.owner;
                                        }
                                        break;
                                    case RoadDirection.East:
                                        if (roadDir == RoadDirection.NorthWest)
                                        {
                                            rp.owner.north = rp.road.north; rp.road.north.south = rp.owner;
                                            rp.owner.west  = rp.road.west;  rp.road.west.east   = rp.owner;
                                        }
                                        else if (roadDir == RoadDirection.SouthWest)
                                        {
                                            rp.owner.south = rp.road.south; rp.road.south.north = rp.owner;
                                            rp.owner.west  = rp.road.west;  rp.road.west.east   = rp.owner;
                                        }
                                        break;
                                    default: throw new System.ArgumentException("in rp.ownerDir");
                                }
                            }
                            else if (rp.cross_type == CrossRoadCreationType.EndOnCorner)
                            {
                                RoadDirection roadDir = rp.road.direction;

                                switch (rp.owner.direction)
                                {
                                    case RoadDirection.NorthWest:
                                        if (roadDir == RoadDirection.South)
                                        {
                                            rp.owner.south = rp.road.south; rp.road.south.north = rp.owner;
                                        }
                                        else if (roadDir == RoadDirection.East)
                                        {
                                            rp.owner.east = rp.road.east; rp.road.east.west = rp.owner;
                                        }
                                        break;
                                    case RoadDirection.NorthEast:
                                        if (roadDir == RoadDirection.South)
                                        {
                                            rp.owner.south = rp.road.south; rp.road.south.north = rp.owner;
                                        }
                                        else if (roadDir == RoadDirection.West)
                                        {
                                            rp.owner.west = rp.road.west; rp.road.west.east = rp.owner;
                                        }
                                        break;
                                    case RoadDirection.SouthWest:
                                        if (roadDir == RoadDirection.North)
                                        {
                                            rp.owner.north = rp.road.north; rp.road.north.south = rp.owner;
                                        }
                                        else if (roadDir == RoadDirection.East)
                                        {
                                            rp.owner.east = rp.road.east; rp.road.east.west = rp.owner;
                                        }
                                        break;
                                    case RoadDirection.SouthEast:
                                        if (roadDir == RoadDirection.North)
                                        {
                                            rp.owner.north = rp.road.north; rp.road.north.south = rp.owner;
                                        }
                                        else if (roadDir == RoadDirection.West)
                                        {
                                            rp.owner.west = rp.road.west; rp.road.west.east = rp.owner;
                                        }
                                        break;
                                    default: throw new System.ArgumentException("in rp.ownerDir");
                                }
                            }

                            rp.owner.SetType(rp.type);
                            rp.owner.SetDirection(rp.ownerDir);

                            rp.owner.LinkWayPoints();

                            Object.Destroy(rp.road.gameObject);
                            crossroadsList.Remove(rp);
                            break;
                        }
                        #endregion

                        #region CrossX
                        else if (rp.type == RoadType.Cross_X)
                        {
                            if (rp.cross_type == CrossRoadCreationType.StraightOnStraight)
                            {
                                RoadDirection old_dir = rp.owner.direction;

                                switch (old_dir)
                                {
                                    case RoadDirection.Vertical:
                                        rp.owner.west = rp.road.west; rp.road.west.east = rp.owner;
                                        rp.owner.east = rp.road.east; rp.road.east.west = rp.owner;
                                        break;
                                    case RoadDirection.Horizontal:
                                        rp.owner.north = rp.road.north; rp.road.north.south = rp.owner;
                                        rp.owner.south = rp.road.south; rp.road.south.north = rp.owner;
                                        break;
                                    default: throw new System.ArgumentException();
                                }
                            }
                            else if (rp.cross_type == CrossRoadCreationType.CornerOnCorner)
                            {
                                switch (rp.owner.direction)
                                {
                                    case RoadDirection.NorthWest:
                                        rp.owner.east  = rp.road.east;  rp.road.east.west   = rp.owner;
                                        rp.owner.south = rp.road.south; rp.road.south.north = rp.owner;
                                        break;
                                    case RoadDirection.NorthEast:
                                        rp.owner.west  = rp.road.west ; rp.road.west.east   = rp.owner;
                                        rp.owner.south = rp.road.south; rp.road.south.north = rp.owner;
                                        break;
                                    case RoadDirection.SouthWest:
                                        rp.owner.east  = rp.road.east;  rp.road.east.west   = rp.owner;
                                        rp.owner.north = rp.road.north; rp.road.north.south = rp.owner;
                                        break;
                                    case RoadDirection.SouthEast:
                                        rp.owner.west  = rp.road.west;  rp.road.west.east   = rp.owner;
                                        rp.owner.north = rp.road.north; rp.road.north.south = rp.owner;
                                        break;
                                }
                            }
                            else if (rp.cross_type == CrossRoadCreationType.EndOnCrossT)
                            {
                                switch (rp.owner.direction)
                                {
                                    case RoadDirection.North:
                                        rp.owner.south = rp.road.south; rp.road.south.north = rp.owner;
                                        break;
                                    case RoadDirection.South:
                                        rp.owner.north = rp.road.north; rp.road.north.south = rp.owner;
                                        break;
                                    case RoadDirection.West:
                                        rp.owner.east = rp.road.east; rp.road.east.west = rp.owner;
                                        break;
                                    case RoadDirection.East:
                                        rp.owner.west = rp.road.west; rp.road.west.east = rp.owner;
                                        break;
                                }
                            }

                            rp.owner.SetType(rp.type);
                            rp.owner.SetDirection(rp.ownerDir);

                            rp.owner.LinkWayPoints();

                            Object.Destroy(rp.road.gameObject);
                            crossroadsList.Remove(rp);
                            break;
                        }
                        #endregion

                        #region Straight
                        else if (rp.type == RoadType.Straight)
                        {
                            rp.owner.SetType(rp.type);

                            switch (rp.ownerDir)
                            {
                                case RoadDirection.North:
                                    rp.owner.north = rp.road.north; rp.road.north.south = rp.owner;
                                    rp.owner.SetDirection(RoadDirection.Vertical); break;
                                case RoadDirection.South:
                                    rp.owner.south = rp.road.south; rp.road.south.north = rp.owner;
                                    rp.owner.SetDirection(RoadDirection.Vertical); break;
                                case RoadDirection.East:
                                    rp.owner.east = rp.road.east; rp.road.east.west = rp.owner;
                                    rp.owner.SetDirection(RoadDirection.Horizontal); break;
                                case RoadDirection.West:
                                    rp.owner.west = rp.road.west; rp.road.west.east = rp.owner;
                                    rp.owner.SetDirection(RoadDirection.Horizontal); break;
                                default: throw new System.ArgumentException("in rp.ownerDir");
                            }

                            rp.owner.LinkWayPoints();

                            Object.Destroy(rp.road.gameObject);
                            crossroadsList.Remove(rp);
                            break;
                        }
                        #endregion

                        #region Corner
                        else if (rp.type == RoadType.Corner)
                        {
                            RoadDirection oldDir = rp.owner.direction;

                            rp.owner.SetType(rp.type);
                            rp.owner.SetDirection(rp.ownerDir);

                            switch (rp.ownerDir)
                            {
                                case RoadDirection.NorthWest:
                                    if (oldDir == RoadDirection.North)
                                    { rp.owner.west = rp.road.west; rp.road.west.east = rp.owner; }
                                    else if (oldDir == RoadDirection.West)
                                    { rp.owner.north = rp.road.north; rp.road.north.south = rp.owner; }
                                    break;
                                case RoadDirection.NorthEast:
                                    if (oldDir == RoadDirection.North)
                                    { rp.owner.east = rp.road.east; rp.road.east.west = rp.owner; }
                                    else if (oldDir == RoadDirection.East)
                                    { rp.owner.north = rp.road.north; rp.road.north.south = rp.owner; }
                                    break;
                                case RoadDirection.SouthWest:
                                    if (oldDir == RoadDirection.South)
                                    { rp.owner.west = rp.road.west; rp.road.west.east = rp.owner; }
                                    else if (oldDir == RoadDirection.West)
                                    { rp.owner.south = rp.road.south; rp.road.south.north = rp.owner; }
                                    break;
                                case RoadDirection.SouthEast:
                                    if (oldDir == RoadDirection.South)
                                    { rp.owner.east = rp.road.east; rp.road.east.west = rp.owner; }
                                    else if (oldDir == RoadDirection.East)
                                    { rp.owner.south = rp.road.south; rp.road.south.north = rp.owner; }
                                    break;
                                default: throw new System.ArgumentException("in rp.ownerDir");
                            }

                            rp.owner.LinkWayPoints();

                            Object.Destroy(rp.road.gameObject);
                            crossroadsList.Remove(rp);
                            break;
                        }
                        #endregion

                        #region End (Entrance)

                        if (rp.type == RoadType.End && rp.cross_type == CrossRoadCreationType.EndOnEntrance)
                        {
                            switch (rp.ownerDir)
                            {
                                case RoadDirection.North: rp.owner.north = rp.road.north; rp.road.north.south = rp.owner; break;
                                case RoadDirection.South: rp.owner.south = rp.road.south; rp.road.south.north = rp.owner; break;
                                case RoadDirection.East:  rp.owner.east  = rp.road.east;  rp.road.east.west   = rp.owner; break;
                                case RoadDirection.West:  rp.owner.west  = rp.road.west;  rp.road.west.east   = rp.owner; break;
                                default: throw new System.ArgumentException("in rp.ownerDir");
                            }

                            rp.owner.SetSortingOrder(3);
                            rp.owner.gameObject.name = "Entrance";

                            last_entrance = null;

                            rp.owner.LinkWayPoints();

                            Object.Destroy(rp.road.gameObject);
                            crossroadsList.Remove(rp);
                            break;
                        }

                        #endregion                        
                    }
                }
                #endregion
                
                road.SetSortingOrder(1);
                road.SetColor(RoadColor.Default);

                Tile tile = WorldMap.GetTile(road.transform.position);
                if (tile.Owner == null) tile.Owner = road;
            }

            //Debug.Log(startPos + "; " + endPos + "; " + X_first);

            if (further_extension)
            {
                startPos = roads[last].transform.position;
                lastEndPos = startPos;
                mouseTilePosition = startPos;
            }
            else
            {
                placeSwitch = true;
            }

            roads = null;
            end_placed = false;

            //RouteSetWP.RecalculateRoutes();
        }
        
        /// <summary>
        /// Строит дорогу от начального участка в конечный, создает список перекрестков.
        /// </summary>
        private static void PlaceRoad()
        {
            #region Вычесление ячеек и прямых участков

            ResetRoad();

            Vector3 difference = endPos - startPos;
            offset = startPos; lastEndPos = endPos;

            int X_times = Mathf.RoundToInt(difference.x / WorldMap.TileSize);
            int Z_times = Mathf.RoundToInt(difference.z / WorldMap.TileSize);

            RoadDirection X_Dir = X_times > 0 ? RoadDirection.East : RoadDirection.West;
            RoadDirection Z_Dir = Z_times > 0 ? RoadDirection.North : RoadDirection.South;

            X_times = Mathf.Abs(X_times);
            Z_times = Mathf.Abs(Z_times);

            if (X_times == 0)
            {
                Z_times++;
                roads = new RoadArray(Z_times);
            }
            else if (Z_times == 0)
            {
                X_times++;
                roads = new RoadArray(X_times);
            }

            #endregion

            // --- определено общее к-во ячеек по горизонтали и вертикали, 
            // --- а также направления прямых участков относительно первой ячейки.

            #region Определение наличия угла и порядка построения участков

            bool corner_exist = (X_times != 0 && Z_times != 0);

            if (corner_exist) roads = new RoadArray(X_times + Z_times + 1);

            if (!firstDirAssigned)
            {
                X_first = Z_times == 0;
                firstDirAssigned = true;
            }

            #endregion

            // --- определено наличие/отсутствие угла (поворота), 
            // --- а также порядок построения прямых участков.

            #region Построение прямых участков и угла
            
            if (X_first)
            {
                PlaceLine(X_times, X_Dir);
                if (corner_exist) PlaceCorner(X_Dir, Z_Dir);
                PlaceLine(Z_times, Z_Dir);
            }
            else
            {
                PlaceLine(Z_times, Z_Dir);
                if (corner_exist) PlaceCorner(X_Dir, Z_Dir);
                PlaceLine(X_times, X_Dir);
            }

            #endregion

            // --- учитывая порядок построения, построены 2 прямых участка дороги, 
            // --- внутри обоих ячейки связаны ссылками между собой. Участки между собой не связаны.
            // --- угол создан, ссылками ни с чем не связан.

            #region Связка ссылками угла с двумя участками

            if (corner_exist) // если есть угол
            {
                int c = X_first ? X_times : Z_times;
                Road corner = roads[c];
                Road last   = roads[c - 1];
                Road first  = roads[c + 1];

                if (X_first)
                {
                    switch (corner.direction) // связка ссылок
                    {
                        case RoadDirection.NorthEast: corner.east = last; last.west = corner; corner.north = first; first.south = corner; break;
                        case RoadDirection.NorthWest: corner.west = last; last.east = corner; corner.north = first; first.south = corner; break;
                        case RoadDirection.SouthEast: corner.east = last; last.west = corner; corner.south = first; first.north = corner; break;
                        case RoadDirection.SouthWest: corner.west = last; last.east = corner; corner.south = first; first.north = corner; break;
                    }
                }
                else
                {
                    switch (corner.direction) // связка ссылок
                    {
                        case RoadDirection.NorthEast: corner.north = last; last.south = corner; corner.east = first; first.west = corner; break;
                        case RoadDirection.NorthWest: corner.north = last; last.south = corner; corner.west = first; first.east = corner; break;
                        case RoadDirection.SouthEast: corner.south = last; last.north = corner; corner.east = first; first.west = corner; break;
                        case RoadDirection.SouthWest: corner.south = last; last.north = corner; corner.west = first; first.east = corner; break;
                    }
                }

                corner.LinkWayPoints(first);
                corner.LinkWayPoints(last);
            }

            #endregion

            // --- если угол есть: построены два прямых участка дороги, ячейки которых связаны между собой.
            // --- два прямых участка связаны между собой через угол (поворот).
            // --- если угла нет: построен один прямой участок дороги, ячейки которого связаны между собой.

            #region Закругление концов прямых участков

            roads[0].   SetType(RoadType.End);
            roads[last].SetType(RoadType.End);

            if (Z_times == 0) // если один прямой горизонтальный участок
            {
                roads[0].SetDirection(X_Dir);
                roads[last].SetDirection(Road.Opposite(X_Dir));
            }
            if (X_times == 0) // если один прямой вертикальный участок
            {
                roads[0].SetDirection(Z_Dir);
                roads[last].SetDirection(Road.Opposite(Z_Dir));
            }
            if (corner_exist) // если два участка с углом (поворотом)
            {
                roads[0].SetDirection(X_first ? X_Dir : Z_Dir);
                roads[last].SetDirection(Road.Opposite(X_first ? Z_Dir : X_Dir));
            }

            roads[0].LinkWayPoints(roads[1]);
            roads[last].LinkWayPoints(roads[last - 1]);

            #endregion

            // --- построение дороги завершено.

            #region Определение перекрестков

            if (roads.Length < 2)
            {
                end_placed = false;
                return;
            }

            if (last_entrance != null)
            {
                last_entrance.SetSortingOrder(3);
                last_entrance = null;
            }

            crossroadsList = new List<RoadPair>();

            for (int i = 0; i < roads.Length; i++)
            {
                Road road = roads[i];
                road.SetColor(RoadColor.Transparent);

                Tile tile = WorldMap.GetTile(road.Position);
                if (tile.Owner != null)
                {
                    System.Type ownerType = tile.Owner.GetType();

                    if (ownerType == typeof(Road))
                    {
                        Road owner = tile.Owner as Road;

                        switch (owner.type)
                        {
                            case RoadType.Straight: // ..OnRoad
                                #region Roadtype.Road

                                if (road.type == RoadType.Straight) // Cross_X
                                {
                                    if (owner.direction == RoadDirection.Vertical)
                                    {
                                        if (road.direction == RoadDirection.Horizontal)
                                        {
                                            crossroadsList.Add(new RoadPair(owner, road, RoadType.Cross_X, RoadDirection.None, CrossRoadCreationType.StraightOnStraight));
                                        }
                                        else
                                        {
                                            end_placed = false;
                                            return;
                                        }
                                    }
                                    else // Horizontal
                                    {
                                        if (road.direction == RoadDirection.Vertical)
                                        {
                                            crossroadsList.Add(new RoadPair(owner, road, RoadType.Cross_X, RoadDirection.None, CrossRoadCreationType.StraightOnStraight));
                                        }
                                        else
                                        {
                                            end_placed = false;
                                            return;
                                        }
                                    }
                                }
                                else if (road.type == RoadType.End) // Cross_T
                                {
                                    if (owner.direction == RoadDirection.Vertical)
                                    {
                                        if (road.direction == RoadDirection.East || road.direction == RoadDirection.West)
                                        {
                                            crossroadsList.Add(new RoadPair(owner, road, RoadType.Cross_T, road.direction, CrossRoadCreationType.EndOnRoad));
                                        }
                                        else
                                        {
                                            end_placed = false;
                                            return;
                                        }
                                    }
                                    else // Horizontal
                                    {
                                        if (road.direction == RoadDirection.North || road.direction == RoadDirection.South)
                                        {
                                            crossroadsList.Add(new RoadPair(owner, road, RoadType.Cross_T, road.direction, CrossRoadCreationType.EndOnRoad));
                                        }
                                        else
                                        {
                                            end_placed = false;
                                            return;
                                        }
                                    }
                                }
                                else
                                {
                                    end_placed = false;
                                    return;
                                }
                                #endregion
                                break;
                            case RoadType.End: // ..OnEnd
                                #region Roadtype.End

                                if (road.type == RoadType.End)
                                {
                                    if (owner.direction == RoadDirection.North)
                                    {
                                        if (road.direction == RoadDirection.North)
                                        {
                                            end_placed = false;
                                            return;
                                        }
                                        else if (road.direction == RoadDirection.South) // Vertical
                                        {
                                            crossroadsList.Add(new RoadPair(owner, road, RoadType.Straight, RoadDirection.South, CrossRoadCreationType.EndOnEnd));
                                        }
                                        else if (road.direction == RoadDirection.West) // UpLeft
                                        {
                                            crossroadsList.Add(new RoadPair(owner, road, RoadType.Corner, RoadDirection.NorthWest, CrossRoadCreationType.EndOnEnd));
                                        }
                                        else if (road.direction == RoadDirection.East) // UpRight
                                        {
                                            crossroadsList.Add(new RoadPair(owner, road, RoadType.Corner, RoadDirection.NorthEast, CrossRoadCreationType.EndOnEnd));
                                        }
                                    }
                                    else if (owner.direction == RoadDirection.South)
                                    {
                                        if (road.direction == RoadDirection.North) // Vertical
                                        {
                                            crossroadsList.Add(new RoadPair(owner, road, RoadType.Straight, RoadDirection.North, CrossRoadCreationType.EndOnEnd));
                                        }
                                        else if (road.direction == RoadDirection.South)
                                        {
                                            end_placed = false;
                                            return;
                                        }
                                        else if (road.direction == RoadDirection.West) // DownLeft
                                        {
                                            crossroadsList.Add(new RoadPair(owner, road, RoadType.Corner, RoadDirection.SouthWest, CrossRoadCreationType.EndOnEnd));
                                        }
                                        else if (road.direction == RoadDirection.East) // DownRight
                                        {
                                            crossroadsList.Add(new RoadPair(owner, road, RoadType.Corner, RoadDirection.SouthEast, CrossRoadCreationType.EndOnEnd));
                                        }
                                    }
                                    else if (owner.direction == RoadDirection.West)
                                    {
                                        if (road.direction == RoadDirection.North) // UpLeft
                                        {
                                            crossroadsList.Add(new RoadPair(owner, road, RoadType.Corner, RoadDirection.NorthWest, CrossRoadCreationType.EndOnEnd));
                                        }
                                        else if (road.direction == RoadDirection.South) // DownLeft
                                        {
                                            crossroadsList.Add(new RoadPair(owner, road, RoadType.Corner, RoadDirection.SouthWest, CrossRoadCreationType.EndOnEnd));
                                        }
                                        else if (road.direction == RoadDirection.West)
                                        {
                                            end_placed = false;
                                            return;
                                        }
                                        else if (road.direction == RoadDirection.East) // Horizontal
                                        {
                                            crossroadsList.Add(new RoadPair(owner, road, RoadType.Straight, RoadDirection.East, CrossRoadCreationType.EndOnEnd));
                                        }
                                    }
                                    else if (owner.direction == RoadDirection.East)
                                    {
                                        if (road.direction == RoadDirection.North) // UpRight
                                        {
                                            crossroadsList.Add(new RoadPair(owner, road, RoadType.Corner, RoadDirection.NorthEast, CrossRoadCreationType.EndOnEnd));
                                        }
                                        else if (road.direction == RoadDirection.South) // DownRight
                                        {
                                            crossroadsList.Add(new RoadPair(owner, road, RoadType.Corner, RoadDirection.SouthEast, CrossRoadCreationType.EndOnEnd));
                                        }
                                        else if (road.direction == RoadDirection.West) // Horizontal
                                        {
                                            crossroadsList.Add(new RoadPair(owner, road, RoadType.Straight, RoadDirection.West, CrossRoadCreationType.EndOnEnd));
                                        }
                                        else if (road.direction == RoadDirection.East)
                                        {
                                            end_placed = false;
                                            return;
                                        }
                                    }
                                }
                                else if (road.type == RoadType.Straight) // RoadOnEnd
                                {
                                    if (owner.direction == RoadDirection.North || owner.direction == RoadDirection.South)
                                    {
                                        if (road.direction == RoadDirection.Horizontal)
                                        {
                                            crossroadsList.Add(new RoadPair(owner, road, RoadType.Cross_T, owner.direction, CrossRoadCreationType.RoadOnEnd));
                                        }
                                        else // Vertical
                                        {
                                            end_placed = false;
                                            return;
                                        }
                                    }
                                    else // owner: Left || Right
                                    {
                                        if (road.direction == RoadDirection.Vertical)
                                        {
                                            crossroadsList.Add(new RoadPair(owner, road, RoadType.Cross_T, owner.direction, CrossRoadCreationType.RoadOnEnd));
                                        }
                                        else // Horizontal
                                        {
                                            end_placed = false;
                                            return;
                                        }
                                    }
                                }
                                else if (road.type == RoadType.Corner) // CornerOnEnd
                                {
                                    if (owner.direction == RoadDirection.North)
                                    {
                                        if (road.direction == RoadDirection.NorthWest || road.direction == RoadDirection.NorthEast)
                                        {
                                            end_placed = false;
                                            return;
                                        }
                                        else if (road.direction == RoadDirection.SouthWest)
                                        {
                                            crossroadsList.Add(new RoadPair(owner, road, RoadType.Cross_T, RoadDirection.West, CrossRoadCreationType.CornerOnEnd));
                                        }
                                        else if (road.direction == RoadDirection.SouthEast)
                                        {
                                            crossroadsList.Add(new RoadPair(owner, road, RoadType.Cross_T, RoadDirection.East, CrossRoadCreationType.CornerOnEnd));
                                        }
                                    }
                                    else if (owner.direction == RoadDirection.South)
                                    {
                                        if (road.direction == RoadDirection.SouthWest || road.direction == RoadDirection.SouthEast)
                                        {
                                            end_placed = false;
                                            return;
                                        }
                                        else if (road.direction == RoadDirection.NorthWest)
                                        {
                                            crossroadsList.Add(new RoadPair(owner, road, RoadType.Cross_T, RoadDirection.West, CrossRoadCreationType.CornerOnEnd));
                                        }
                                        else if (road.direction == RoadDirection.NorthEast)
                                        {
                                            crossroadsList.Add(new RoadPair(owner, road, RoadType.Cross_T, RoadDirection.East, CrossRoadCreationType.CornerOnEnd));
                                        }
                                    }
                                    else if (owner.direction == RoadDirection.West)
                                    {
                                        if (road.direction == RoadDirection.NorthWest || road.direction == RoadDirection.SouthWest)
                                        {
                                            end_placed = false;
                                            return;
                                        }
                                        else if (road.direction == RoadDirection.NorthEast)
                                        {
                                            crossroadsList.Add(new RoadPair(owner, road, RoadType.Cross_T, RoadDirection.North, CrossRoadCreationType.CornerOnEnd));
                                        }
                                        else if (road.direction == RoadDirection.SouthEast)
                                        {
                                            crossroadsList.Add(new RoadPair(owner, road, RoadType.Cross_T, RoadDirection.South, CrossRoadCreationType.CornerOnEnd));
                                        }
                                    }
                                    else if (owner.direction == RoadDirection.East)
                                    {
                                        if (road.direction == RoadDirection.NorthEast || road.direction == RoadDirection.SouthEast)
                                        {
                                            end_placed = false;
                                            return;
                                        }
                                        else if (road.direction == RoadDirection.NorthWest)
                                        {
                                            crossroadsList.Add(new RoadPair(owner, road, RoadType.Cross_T, RoadDirection.North, CrossRoadCreationType.CornerOnEnd));
                                        }
                                        else if (road.direction == RoadDirection.SouthWest)
                                        {
                                            crossroadsList.Add(new RoadPair(owner, road, RoadType.Cross_T, RoadDirection.South, CrossRoadCreationType.CornerOnEnd));
                                        }
                                    }
                                }
                                else
                                {
                                    end_placed = false;
                                    return;
                                }
                                #endregion
                                break;
                            case RoadType.Corner: // ..OnCorner
                                #region Roadtype.Corner

                                if (road.type == RoadType.Corner) // CornerOnCorner
                                {
                                    if ((owner.direction == RoadDirection.NorthWest && road.direction == RoadDirection.SouthEast) ||
                                        (owner.direction == RoadDirection.NorthEast && road.direction == RoadDirection.SouthWest) ||
                                        (owner.direction == RoadDirection.SouthWest && road.direction == RoadDirection.NorthEast) ||
                                        (owner.direction == RoadDirection.SouthEast && road.direction == RoadDirection.NorthWest))
                                        crossroadsList.Add(new RoadPair(owner, road, RoadType.Cross_X, RoadDirection.None, CrossRoadCreationType.CornerOnCorner));
                                    else { end_placed = false; return; }
                                }
                                else if (road.type == RoadType.End) // EndOnCorner
                                {
                                    if (owner.direction == RoadDirection.NorthWest)
                                    {
                                        if (road.direction == RoadDirection.South)
                                        {
                                            crossroadsList.Add(new RoadPair(owner, road, RoadType.Cross_T, RoadDirection.West, CrossRoadCreationType.EndOnCorner));
                                        }
                                        else if (road.direction == RoadDirection.East)
                                        {
                                            crossroadsList.Add(new RoadPair(owner, road, RoadType.Cross_T, RoadDirection.North, CrossRoadCreationType.EndOnCorner));
                                        }
                                        else
                                        {
                                            end_placed = false;
                                            return;
                                        }
                                    }
                                    else if (owner.direction == RoadDirection.NorthEast)
                                    {
                                        if (road.direction == RoadDirection.South)
                                        {
                                            crossroadsList.Add(new RoadPair(owner, road, RoadType.Cross_T, RoadDirection.East, CrossRoadCreationType.EndOnCorner));
                                        }
                                        else if (road.direction == RoadDirection.West)
                                        {
                                            crossroadsList.Add(new RoadPair(owner, road, RoadType.Cross_T, RoadDirection.North, CrossRoadCreationType.EndOnCorner));
                                        }
                                        else
                                        {
                                            end_placed = false;
                                            return;
                                        }
                                    }
                                    else if (owner.direction == RoadDirection.SouthWest)
                                    {
                                        if (road.direction == RoadDirection.North)
                                        {
                                            crossroadsList.Add(new RoadPair(owner, road, RoadType.Cross_T, RoadDirection.West, CrossRoadCreationType.EndOnCorner));
                                        }
                                        else if (road.direction == RoadDirection.East)
                                        {
                                            crossroadsList.Add(new RoadPair(owner, road, RoadType.Cross_T, RoadDirection.South, CrossRoadCreationType.EndOnCorner));
                                        }
                                        else
                                        {
                                            end_placed = false;
                                            return;
                                        }
                                    }
                                    else if (owner.direction == RoadDirection.SouthEast)
                                    {
                                        if (road.direction == RoadDirection.North)
                                        {
                                            crossroadsList.Add(new RoadPair(owner, road, RoadType.Cross_T, RoadDirection.East, CrossRoadCreationType.EndOnCorner));
                                        }
                                        else if (road.direction == RoadDirection.West)
                                        {
                                            crossroadsList.Add(new RoadPair(owner, road, RoadType.Cross_T, RoadDirection.South, CrossRoadCreationType.EndOnCorner));
                                        }
                                        else
                                        {
                                            end_placed = false;
                                            return;
                                        }
                                    }
                                    else
                                    {
                                        end_placed = false;
                                        return;
                                    }

                                }
                                else
                                {
                                    end_placed = false;
                                    return;
                                }

                                #endregion
                                break;
                            case RoadType.Cross_T: // ..OnCrossT
                                #region Roadtype.Cross_T

                                if (road.type == RoadType.End)
                                {
                                    if ((owner.direction == RoadDirection.North && road.direction == RoadDirection.South) ||
                                        (owner.direction == RoadDirection.South && road.direction == RoadDirection.North) ||
                                        (owner.direction == RoadDirection.West && road.direction == RoadDirection.East) ||
                                        (owner.direction == RoadDirection.East && road.direction == RoadDirection.West))
                                        crossroadsList.Add(new RoadPair(owner, road, RoadType.Cross_X, RoadDirection.None, CrossRoadCreationType.EndOnCrossT));
                                    else { end_placed = false; return; }
                                }
                                else
                                {
                                    end_placed = false;
                                    return;
                                }

                                #endregion
                                break;
                            case RoadType.Cross_X:
                                end_placed = false;
                                break;
                            case RoadType.Single:
                                end_placed = false;
                                break;
                        }

                    }
                    else if (ownerType.IsSubclassOf(typeof(EntranceBuilding)))
                    {
                        Road entrance = (tile.Owner as EntranceBuilding).entrance;

                        if (WorldMap.GetTile(road.transform.position) != WorldMap.GetTile(entrance.transform.position))
                        {
                            end_placed = false;
                            return;
                        }

                        #region RoadType.End
                        if (road.type == RoadType.End)
                        {
                            if (entrance.direction == RoadDirection.North)
                            {
                                if (road.direction == RoadDirection.North)
                                {
                                    entrance.SetSortingOrder(1);
                                    road.SetSortingOrder(3);
                                    last_entrance = entrance;
                                    crossroadsList.Add(new RoadPair(entrance, road, RoadType.End, RoadDirection.North, CrossRoadCreationType.EndOnEntrance));
                                }
                                else
                                {
                                    end_placed = false;
                                    return;
                                }
                            }
                            else if (entrance.direction == RoadDirection.South)
                            {
                                if (road.direction == RoadDirection.South)
                                {
                                    entrance.SetSortingOrder(1);
                                    road.SetSortingOrder(3);
                                    last_entrance = entrance;
                                    crossroadsList.Add(new RoadPair(entrance, road, RoadType.End, RoadDirection.South, CrossRoadCreationType.EndOnEntrance));
                                }
                                else
                                {
                                    end_placed = false;
                                    return;
                                }
                            }
                            else if (entrance.direction == RoadDirection.West)
                            {
                                if (road.direction == RoadDirection.West)
                                {
                                    entrance.SetSortingOrder(1);
                                    road.SetSortingOrder(3);
                                    last_entrance = entrance;
                                    crossroadsList.Add(new RoadPair(entrance, road, RoadType.End, RoadDirection.West, CrossRoadCreationType.EndOnEntrance));
                                }
                                else
                                {
                                    end_placed = false;
                                    return;
                                }
                            }
                            else if (entrance.direction == RoadDirection.East)
                            {
                                if (road.direction == RoadDirection.East)
                                {
                                    entrance.SetSortingOrder(1);
                                    road.SetSortingOrder(3);
                                    last_entrance = entrance;
                                    crossroadsList.Add(new RoadPair(entrance, road, RoadType.End, RoadDirection.East, CrossRoadCreationType.EndOnEntrance));
                                }
                                else
                                {
                                    end_placed = false;
                                    return;
                                }
                            }
                        }
                        #endregion
                    }
                    else
                    {
                        end_placed = false;
                        return;
                    }
                }
            }

            end_placed = true;

            #endregion

            // --- определение перекрестков завершено. 
            // --- образованы пары участков, типы перекрестков, 
            // --- а также способы взаимного размещения пересекающихся дорог.
        }

        /// <summary>
        /// Строит прямую линию дороги.
        /// </summary>
        /// <param name="times">Количество участков, которые последовательно выстраиваются по прямой.</param>
        /// <param name="dir">Направление, по которому выстраеваются участки.</param>
        private static void PlaceLine(int times, RoadDirection dir)
        {
            Vector3 adding = GetAddingVector(dir);

            for (int i = 0; i < times; i++)
            {
                Road newRoad = Object.Instantiate(Objects.GameObjects.roadStraight, offset, Quaternion.identity);
                RoadDirection newRoadDir =
                    (dir == RoadDirection.North || dir == RoadDirection.South)
                    ? RoadDirection.Vertical : RoadDirection.Horizontal;

                newRoad.SetDirection(newRoadDir);
                newRoad.SetSortingOrder(2);

                if (roads.Length > 0)
                {
                    if (dir == RoadDirection.East)
                    {
                        roads[last].east = newRoad; newRoad.west = roads[last];
                    }
                    else if (dir == RoadDirection.West)
                    {
                        roads[last].west = newRoad; newRoad.east = roads[last];
                    }
                    else if (dir == RoadDirection.North)
                    {
                        roads[last].north = newRoad; newRoad.south = roads[last];
                    }
                    else if (dir == RoadDirection.South)
                    {
                        roads[last].south = newRoad; newRoad.north = roads[last];
                    }

                    if (i != 0 && i != times)
                        newRoad.LinkWayPoints(roads[last]);
                }

                roads.Add(newRoad);
                offset += adding;
            }
        }

        /// <summary>
        /// Строит угол между 2 прямыми линиями дорог, направлеными по <paramref name="X_Dir"/> и <paramref name="Y_Dir"/> соответственно.
        /// </summary>
        /// <param name="X_Dir">Направление горизонтальной линии дороги.</param>
        /// <param name="Y_Dir">Направление вертикальной линии дороги.</param>
        /// <returns>Ссылка на построенный угол.</returns>
        private static Road PlaceCorner(RoadDirection X_Dir, RoadDirection Y_Dir)
        {
            Road corner = Object.Instantiate(Objects.GameObjects.corner, offset, Quaternion.identity);
            corner.SetSortingOrder(2);

            if      (X_Dir == RoadDirection.East && Y_Dir == RoadDirection.North) corner.SetDirection(X_first ? RoadDirection.NorthWest : Road.Opposite(RoadDirection.NorthWest));
            else if (X_Dir == RoadDirection.East && Y_Dir == RoadDirection.South) corner.SetDirection(X_first ? RoadDirection.SouthWest : Road.Opposite(RoadDirection.SouthWest));
            else if (X_Dir == RoadDirection.West && Y_Dir == RoadDirection.North) corner.SetDirection(X_first ? RoadDirection.NorthEast : Road.Opposite(RoadDirection.NorthEast));
            else if (X_Dir == RoadDirection.West && Y_Dir == RoadDirection.South) corner.SetDirection(X_first ? RoadDirection.SouthEast : Road.Opposite(RoadDirection.SouthEast));
            
            roads.Add(corner);
            offset += X_first ? GetAddingVector(Y_Dir) : GetAddingVector(X_Dir);

            return corner;
        }

        /// <summary>
        /// Возвращает вектор в зависимости от направления <paramref name="dir"/>.
        /// </summary>
        /// <param name="dir">Направление для вектора.</param>
        /// <returns>Расчитанный вектор.</returns>
        private static Vector3 GetAddingVector(RoadDirection dir)
        {
            Vector3 adding = Vector3.zero;

            if      (dir == RoadDirection.North) adding = new Vector3(0, 0,  WorldMap.TileSize);
            else if (dir == RoadDirection.South) adding = new Vector3(0, 0, -WorldMap.TileSize);
            else if (dir == RoadDirection.East) adding  = new Vector3( WorldMap.TileSize, 0, 0);
            else if (dir == RoadDirection.West) adding  = new Vector3(-WorldMap.TileSize, 0, 0);

            return adding;
        }

        /// <summary>
        /// Очищает дорожный список и уничтожает GameObject записанных участков.
        /// </summary>
        private static void ResetRoad()
        {
            if (roads != null)
            {
                for (int i = 0; i < roads.Length; i++)
                    Object.Destroy(roads[i].gameObject);

                roads = null;
            }
            offset = startPos;
        }

        /// <summary>
        /// Откатывает состояние строительства дороги в начальное и деактивирует этот RoadCreator.
        /// </summary>
        private static void Cancel()
        {
            ResetRoad();

            Enabled = false;
            placeSwitch = true;
            end_placed = false;
            firstDirAssigned = false;

            ToolBar.Components.EnableButtons();
        }

        /// <summary>
        /// ТОЛЬКО ДЛЯ ТЕСТИРОВАНИЯ! Строит дорогу от from до to.
        /// </summary>
        public static void AutoCreate(Vector3 from, Vector3 to, bool x_first = true)
        {
            if (WorldMap.GetTile(from) == WorldMap.GetTile(to))
                return;

            startPos = from;
            endPos = to;
            X_first = x_first;

            PlaceRoad();
            ApplyRoad();
        }
    }
}