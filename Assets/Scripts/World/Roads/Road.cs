using System;
using System.Linq;
using UnityEngine;
using Industry.Managers;
using Industry.AI.Routing;

namespace Industry.World.Roads
{
    public enum RoadDirection
    {
        None,
        Horizontal, Vertical,
        North, South, East, West,
        NorthEast, NorthWest, SouthEast, SouthWest
    }
    public enum RoadType
    {
        Straight, End, Corner, Cross_T, Cross_X, Single
    }
    public enum RoadColor
    {
        Default, Red, Transparent
    }

    public class Road : MonoBehaviour
    {
        public Road north;
        public Road south;
        public Road east;
        public Road west;
        
        public Sprite sprite
        {
            get {  return GetComponent<SpriteRenderer>().sprite; }
            private set { GetComponent<SpriteRenderer>().sprite = value; }
        }
        public Vector3 Position
        {
            get { return transform.position; }
            set { transform.position = value; }
        }
        public WayPointContainer WPContainer
        {
            get; set;
        }

        private SpriteRenderer spriteRenderer;
        
        public RoadColor color;
        public RoadDirection direction;
        public RoadType type;

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            
            if (WPContainer == null)
                WPContainer = GetComponentInChildren<WayPointContainer>();

            if (straight == null)
            {
                straight = Objects.GameObjects.roadStraight.GetComponent<SpriteRenderer>().sprite;
                end      = Objects.GameObjects.roadEnd.     GetComponent<SpriteRenderer>().sprite;
                corner   = Objects.GameObjects.corner.      GetComponent<SpriteRenderer>().sprite;
                cross_T  = Objects.GameObjects.cross_T.     GetComponent<SpriteRenderer>().sprite;
                cross_X  = Objects.GameObjects.cross_X.     GetComponent<SpriteRenderer>().sprite;
                single   = Objects.GameObjects.single.      GetComponent<SpriteRenderer>().sprite;

                defaultColor = spriteRenderer.material.color;
            }
        }
        void Start()
        {
            gameObject.name = this.type.ToString();
            enabled = false;
        }

        public void LinkWayPoints()
        {
            if (north != null)
                LinkWayPoints(north);
            if (south != null)
                LinkWayPoints(south);
            if (east != null)
                LinkWayPoints(east);
            if (west != null)
                LinkWayPoints(west);
        }
        public void LinkWayPoints(Road other)
        {
            WPContainer.LinkWayPoints(other);
        }

        public void SetDirection(RoadDirection direction)
        {
            ArgumentException AE = new ArgumentException("Direction \"" + direction.ToString() + "\" is not compatible with road type " + this.type.ToString() + ".");
            switch (this.type)
            {
                case RoadType.Straight:
                    if      (direction == RoadDirection.Vertical)   transform.localEulerAngles = new Vector3(90, 0, 0);
                    else if (direction == RoadDirection.Horizontal) transform.localEulerAngles = new Vector3(90, 0, 270);
                    else throw AE;
                    break;

                case RoadType.End:
                    if      (direction == RoadDirection.North) transform.localEulerAngles = new Vector3(90, 0, 180);
                    else if (direction == RoadDirection.South) transform.localEulerAngles = new Vector3(90, 0, 0);
                    else if (direction == RoadDirection.East)  transform.localEulerAngles = new Vector3(90, 0, 90);
                    else if (direction == RoadDirection.West)  transform.localEulerAngles = new Vector3(90, 0, 270);
                    else throw AE;
                    break;

                case RoadType.Corner:
                    if      (direction == RoadDirection.NorthEast) transform.localEulerAngles = new Vector3(90, 0, 90);
                    else if (direction == RoadDirection.NorthWest) transform.localEulerAngles = new Vector3(90, 0, 180);
                    else if (direction == RoadDirection.SouthEast) transform.localEulerAngles = new Vector3(90, 0, 0);
                    else if (direction == RoadDirection.SouthWest) transform.localEulerAngles = new Vector3(90, 0, 270);
                    else throw AE;
                    break;

                case RoadType.Cross_T:
                    if      (direction == RoadDirection.North) transform.localEulerAngles = new Vector3(90, 0, 90);
                    else if (direction == RoadDirection.South) transform.localEulerAngles = new Vector3(90, 0, 270);
                    else if (direction == RoadDirection.East)  transform.localEulerAngles = new Vector3(90, 0, 0);
                    else if (direction == RoadDirection.West)  transform.localEulerAngles = new Vector3(90, 0, 180);
                    else throw AE;
                    break;

                case RoadType.Cross_X:
                    if (direction != RoadDirection.None) throw AE;
                    break;
                case RoadType.Single:
                    if (direction == RoadDirection.None) transform.localEulerAngles = new Vector3(90, 0, 0);
                    else throw AE;
                    break;
            }
            this.direction = direction;

            //SetPoints();
        }       
        public void SetType(RoadType type)
        {
            if (type != this.type)
            {
                this.type = type;
                gameObject.name = type.ToString();

                WayPointContainer new_wpc = null;

                if (WPContainer != null)
                    Destroy(WPContainer.gameObject);

                switch (type)
                {
                    case RoadType.Straight:
                        sprite = straight;
                        new_wpc = Instantiate(Objects.GameObjects.wpc_straight, transform);
                        break;
                    case RoadType.End:
                        sprite = end;
                        new_wpc = Instantiate(Objects.GameObjects.wpc_end, transform);
                        break;
                    case RoadType.Corner:
                        sprite = corner;
                        new_wpc = Instantiate(Objects.GameObjects.wpc_corner, transform);
                        break;
                    case RoadType.Cross_T:
                        sprite = cross_T;
                        new_wpc = Instantiate(Objects.GameObjects.wpc_crossT, transform);
                        break;
                    case RoadType.Cross_X:
                        sprite = cross_X;
                        new_wpc = Instantiate(Objects.GameObjects.wpc_crossX, transform);
                        break;
                    case RoadType.Single:
                        sprite = single;
                        new_wpc = Instantiate(Objects.GameObjects.wpc_single, transform);
                        break;
                }

                WPContainer = new_wpc;
            }
        }
        public void SetTypeAndDirection(RoadType type, RoadDirection direction, bool link_wps = true)
        {
            SetType(type);
            SetDirection(direction);

            if (link_wps)
                LinkWayPoints();
        }
        public void SetColor(RoadColor color)
        {
            if (this.color != color)
            {
                sprite = SpriteLoader.GetRoad(type, color);
                this.color = color;
            }
        }
        public void SetColor(Color color)
        {
            spriteRenderer.material.color = color;
        }
        public void SetSortingOrder(int order)
        {
            spriteRenderer.sortingOrder = order;
        }

        public override string ToString()
        {
            return gameObject.name + ": " + direction.ToString();
        }

        private static Sprite straight, end, corner, cross_T, cross_X, single;
        public static Color defaultColor
        {
            get; private set;
        }
        
        public static RoadDirection Opposite(RoadDirection dir)
        {
            switch (dir)
            {
                case RoadDirection.Vertical:   return RoadDirection.Horizontal;
                case RoadDirection.Horizontal: return RoadDirection.Vertical;

                case RoadDirection.North: return RoadDirection.South;
                case RoadDirection.South: return RoadDirection.North;
                case RoadDirection.West:  return RoadDirection.East;
                case RoadDirection.East:  return RoadDirection.West;

                case RoadDirection.NorthWest: return RoadDirection.SouthEast;
                case RoadDirection.NorthEast: return RoadDirection.SouthWest;
                case RoadDirection.SouthWest: return RoadDirection.NorthEast;
                case RoadDirection.SouthEast: return RoadDirection.NorthWest;

                default: return RoadDirection.None;
            }
        }
    }
}