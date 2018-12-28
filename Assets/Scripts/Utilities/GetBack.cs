using Industry.World.Roads;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Industry.Utilities
{
    public static class GetBack
    {
        public static List<Road> getWayToClosest(Road currentRoad, List<Road> mainRoad)
        {
            HashSet<RoadNode> roadToCheck = new HashSet<RoadNode>();
            List<Road> path = new List<Road>();
            path.Add(currentRoad);
            RoadNode currRoad = new RoadNode(currentRoad, path);
            return loop(roadToCheck, null, mainRoad, null);
        }

        private static List<Road> loop(HashSet<RoadNode> checkingRoads, HashSet<Road> checkedRoads, List<Road> mainRoad, HashSet<List<Road>> paths)
        {
            HashSet<RoadNode> newCheckingRoads = new HashSet<RoadNode>();
            foreach(RoadNode currentRoad in checkingRoads)
            {
                checkedRoads.Add(currentRoad.Road);
                if (!mainRoad.Contains(currentRoad.Road))
                {
                    if (!(currentRoad.Road.type == RoadType.End) && (checkedRoads.Count > 1)) paths.Remove(currentRoad.Path);
                    if (currentRoad.Road.north != null && !checkedRoads.Contains(currentRoad.Road.north))
                    {
                        List<Road> newPath = new List<Road>();
                        newPath.AddRange(currentRoad.Path);
                        newPath.Add(currentRoad.Road.north);
                        newCheckingRoads.Add(new RoadNode(currentRoad.Road.north, newPath));
                        paths.Add(newPath);
                    }
                    if (currentRoad.Road.east != null && !checkedRoads.Contains(currentRoad.Road.east))
                    {
                        List<Road> newPath = new List<Road>();
                        newPath.AddRange(currentRoad.Path);
                        newPath.Add(currentRoad.Road.east);
                        newCheckingRoads.Add(new RoadNode(currentRoad.Road.east, newPath));
                        paths.Add(newPath);
                    }
                    if (currentRoad.Road.south != null && !checkedRoads.Contains(currentRoad.Road.south))
                    {
                        List<Road> newPath = new List<Road>();
                        newPath.AddRange(currentRoad.Path);
                        newPath.Add(currentRoad.Road.south);
                        newCheckingRoads.Add(new RoadNode(currentRoad.Road.south, newPath));
                        paths.Add(newPath);
                    }
                    if (currentRoad.Road.west != null && !checkedRoads.Contains(currentRoad.Road.west))
                    {
                        List<Road> newPath = new List<Road>();
                        newPath.AddRange(currentRoad.Path);
                        newPath.Add(currentRoad.Road.west);
                        newCheckingRoads.Add(new RoadNode(currentRoad.Road.west, newPath));
                        paths.Add(newPath);
                    }
                }
            }
            if (newCheckingRoads.Count == 0)
            {
                List<Road> result = new List<Road>();
                int length = -1;
                foreach(List<Road> currentPath in paths)
                {
                    if (length == -1)
                    {
                        result = currentPath;
                        for (int i = 0; i < mainRoad.Count; i++)
                        {
                            if (mainRoad[i] == currentPath.Last())
                            {
                                length = mainRoad.Count - i - 1 + currentPath.Count;
                                break;
                            }
                        }
                    }
                    else
                    {
                        int tmpLength = 0;
                        for (int i = 0; i < mainRoad.Count; i++)
                        {
                            if (mainRoad[i] == currentPath.Last())
                            {
                                tmpLength = mainRoad.Count - i - 1 + currentPath.Count;
                                break;
                            }
                        }
                        if (length > tmpLength)
                        {
                            length = tmpLength;
                            result = currentPath;
                        }
                    }
                }
                return result;
            }
            else
            {
                return loop(newCheckingRoads, checkedRoads, mainRoad, paths);
            }
        }
    }

    public class RoadNode
    {
        public Road Road
        {
            get; private set;
        }
        public List<Road> Path
        {
            get; private set;
        }
        public RoadNode(Road road, List<Road> path)
        {
            Road = road;
            Path = path;
        }
    }
}
