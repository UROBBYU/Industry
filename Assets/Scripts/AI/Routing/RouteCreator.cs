using System.Collections.Generic;
using UnityEngine;
using Industry.UI;
using Industry.World.Buildings;
using Industry.World.Map;
using Industry.World.Roads;
using Industry.Utilities;
using System.Linq;
using Industry.World;

namespace Industry.AI.Routing
{
    /// <summary>
    /// Класс, отвечающий за построение путей.
    /// </summary>
    public static class RouteCreator
    {
        /// <summary>
        /// Переключатель для логгирования длительности выполнения путей.
        /// </summary>
        /// <value>
        /// Свойство Logging включает/выключает логгирование длительности построения путей.
        /// </value>
        public static bool Logging
        {
            get; set;
        }

        /// <summary>
        /// Возвращает последовательность <typeparamref name="WayPoint"/>'ов, являющихся кратчайшим путём от <typeparamref name="WayPoint"/>'а <paramref name="from"/> к <typeparamref name="WayPoint"/>'у <paramref name="to"/>.
        /// </summary>
        /// <param name="from"><typeparamref name="WayPoint"/>, от которого строится путь.</param>
        /// <param name="to"><typeparamref name="WayPoint"/>, к которому строится путь.</param>
        /// <param name="back">Если <typeparamref name="true"/>, то построенный путь будет замкнутым, иначе - нет.</param>
        /// <returns>Возвращает последовательность <typeparamref name="WayPoint"/>'ов, являющихся кратчайшим путём.</returns>
        private static List<WayPoint> BFS(WayPoint from, WayPoint to, bool back)
        {
            Queue<WayPoint> queue = new Queue<WayPoint>();
            HashSet<WayPoint> explored = new HashSet<WayPoint>();
            Dictionary<WayPoint, WayPoint> parentsWP = new Dictionary<WayPoint, WayPoint>(64);
            queue.Enqueue(from);

            while (queue.Count > 0)
            {
                WayPoint current = queue.Dequeue();
                if (current == to) break;

                WayPoint[] links = current.neighbours.Where(wp => wp != null).ToArray();

                for (int i = 0; i < links.Length; i++)
                {
                    WayPoint link = links[i];
                    if (!explored.Contains(link))
                    {
                        explored.Add(link);
                        parentsWP.Add(link, current);
                        queue.Enqueue(link);
                    }
                }

                if (queue.Count == 0) return null;
            }

            List<WayPoint> path = new List<WayPoint>();

            WayPoint curr = to;

            try
            {
                while (curr != from)
                {
                    path.Add(curr);
                    curr = parentsWP[curr];
                }

                path.Add(from);
                path.Reverse();

                if (back)
                {
                    //parentsWP.Clear();

                    WayPoint last = path[path.Count - 1];
                    WayPoint _from = last.neighbours[0] ?? last.neighbours[1];

                    List<WayPoint> backPath = BFS(_from, path[0], false);
                    backPath.Remove(backPath[backPath.Count - 1]);

                    path.AddRange(backPath);
                }

                //parentsWP.Clear();

                return path;
            }
            catch (KeyNotFoundException)
            {
                return null;
            }
        }

        /// <summary>
        /// Возвращает последовательность <typeparamref name="WayPoint"/>'ов, являющейся кратчайшим путём 
        /// от конца дороги [<typeparamref name="Road"/>] <paramref name="from"/> 
        /// к концу дороги [<typeparamref name="Road"/>] <paramref name="to"/>.
        /// </summary>
        /// <param name="fromEnd">Дорога, от которой строится путь.</param>
        /// <param name="toEnd">Дорога, к которой строится путь.</param>
        /// <param name="back">Если <typeparamref name="true"/>, то построенный путь будет замкнутым, иначе - нет.</param>
        /// <returns>Возвращает последовательность Вэйпонтов, являющихся кратчайшим путём.</returns>
        public static List<WayPoint> CreatePathWP(Road fromEnd, Road toEnd, bool back = true)
        {
            if (fromEnd == null || toEnd == null)
                return null;

            if (fromEnd.type != RoadType.End || toEnd.type != RoadType.End)
                throw new System.ArgumentException("fromEnd's and/or toEnd's type is not RoadType.End");

            Timer timer = null;

            if (Logging)
                timer = new Timer().Start();

            WayPoint start = fromEnd.WPContainer[fromEnd.WPContainer.WPCount / 2];
            WayPoint end = toEnd.WPContainer[toEnd.WPContainer.WPCount / 2];

            List<WayPoint> shortestPath = BFS(start, end, back);

            if (Logging)
            {
                double time = timer.ElapsedTime(Timer.Units.Milliseconds);
                if (shortestPath != null)
                    Debug.Log("<color=green>Pathfinding:</color> Path length: " + shortestPath.Count + ". Elapsed time: " + time + " ms.");
                else
                    Debug.Log("<color=red>Pathfinding:</color> Path doesn't exist. Elapsed time: " + time + " ms.");
            }

            return shortestPath;
        }
        /// <summary>
        /// Возвращает последовательность <typeparamref name="WayPoint"/>'ов, являющейся кратчайшим путём 
        /// от конца дороги [<typeparamref name="Road"/>], находящейся по координатам <paramref name="from"/> 
        /// к концу дороги [<typeparamref name="Road"/>], находящейся по координатам <paramref name="to"/>.
        /// </summary>
        /// <param name="from">Ячейка дороги, от которой строится путь.</param>
        /// <param name="to">Ячейка дороги, к которой строится путь.</param>
        /// <returns>Возвращает список 'ов, если путь существует и <typeparamref name="null"/> - если путь не существует.</returns>
        public static List<WayPoint> CreatePathWP(Vector3 from, Vector3 to, bool back = true)
        {
            Object ownerFrom = WorldMap.GetTile(from).Owner;
            Object ownerTo = WorldMap.GetTile(to).Owner;

            Road rFrom = ownerFrom.GetType() == typeof(Road) ? ownerFrom as Road : ownerFrom.GetType().IsSubclassOf(typeof(EntranceBuilding)) ? (ownerFrom as EntranceBuilding).entrance : null;
            Road rTo = ownerTo.GetType() == typeof(Road) ? ownerTo as Road : ownerTo.GetType().IsSubclassOf(typeof(EntranceBuilding)) ? (ownerTo as EntranceBuilding).entrance : null;

            return CreatePathWP(rFrom, rTo, back);
        }
        /// <summary>
        /// Возвращает последовательность <typeparamref name="WayPoint"/>'ов, являющихся кратчайшим путём от <typeparamref name="WayPoint"/>'а <paramref name="from"/> к <typeparamref name="WayPoint"/>'у <paramref name="to"/>.
        /// </summary>
        /// <param name="from"><typeparamref name="WayPoint"/>, от которого строится путь.</param>
        /// <param name="to"><typeparamref name="WayPoint"/>, к которому строится путь.</param>
        /// <param name="back">Если <typeparamref name="true"/>, то построенный путь будет замкнутым, иначе - нет.</param>
        /// <returns>Возвращает последовательность <typeparamref name="WayPoint"/>'ов, являющихся кратчайшим путём.</returns>
        public static List<WayPoint> CreatePathWP(WayPoint from, WayPoint to, bool back = true)
        {
            if (from == null || to == null)
                throw new System.NullReferenceException("from and/or to is null");

            if (from == to)
                return null;

            return BFS(from, to, back);
        }
    }
}
