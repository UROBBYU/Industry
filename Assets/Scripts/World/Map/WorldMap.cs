using UnityEngine;
using Industry.World.Roads;

namespace Industry.World.Map
{
    /// <summary>
    /// Карта Мира.
    /// </summary>
    public static class WorldMap
    {
        private static bool isInitialized = Initialize();
        private static int lengthX;
        private static int lengthZ;
        
        /// <summary>
        /// Двумерный массив Tile'ов, из которых состоит Карта Мира.
        /// </summary>
        private static Tile[,] Tiles;
        
        /*
        /// <summary>
        /// 
        /// </summary>
        public static Sprite Background
        {
            get { return GameObject.FindGameObjectWithTag("Background").GetComponent<SpriteRenderer>().sprite; }
            set { GameObject.FindGameObjectWithTag("Background").GetComponent<SpriteRenderer>().sprite = value; }
        }
        
             */

        /// <summary>
        /// Граничная координата Карты Мира.
        /// </summary>
        public static Vector3 UpLeft
        {
            get; private set;
        }
        /// <summary>
        /// Размер Карты Мира.
        /// </summary>
        public static Vector2 MapSize
        {
            get; private set;
        }
        /// <summary>
        /// Размер одного Tile'а.
        /// </summary>
        public static float TileSize
        {
            get; private set;
        }

        /// <summary>
        /// Метод первичной инициализации Карты Мира. Может быть выполнен только 1 раз.
        /// </summary>
        public static bool Initialize()
        {
            if (isInitialized) return true;

            lengthZ = 100;
            lengthX = 100;

            MapSize = new Vector2(640, 640);
            UpLeft = new Vector3(-320f, 0.02f, 320f);
            TileSize = 6.4f;

            Tiles = new Tile[lengthZ, lengthX];

            Vector3 pos = new Vector3(UpLeft.x + TileSize / 2, 0.02f, UpLeft.z - TileSize / 2);

            for (int i = 0; i < lengthZ; i++)
            {
                for (int j = 0; j < lengthX; j++)
                {
                    Tiles[i, j] = new Tile(pos);
                    pos.x += TileSize;
                }
                pos.x = UpLeft.x + TileSize / 2;
                pos.z -= TileSize;
            }

            isInitialized = true;
            return true;
        }
        /// <summary>
        /// Очищает Tile от Owner'а.
        /// </summary>
        /// <param name="pos">Позиция Tile'а.</param>
        public static void ClearTile(Vector3 pos)
        {
            GetTile(pos).Owner = null;
        }
        /// <summary>
        /// Очищает область ото всех Owner'ов.
        /// </summary>
        /// <param name="upLeftTilePosition">Позиция верхнего левого Tile'а области.</param>
        /// <param name="countX">Длина области по оси X.</param>
        /// <param name="countZ">Ширина области по оси Z.</param>
        public static void ClearArea(Vector3 upLeftTilePosition, int countX, int countZ)
        {
            Vector3 pos = upLeftTilePosition;
            for (int i = 0; i < countZ; i++)
            {
                for (int j = 0; j < countX; j++)
                {
                    GetTile(pos).Owner = null;
                    pos.x += TileSize;
                }
                pos.x = upLeftTilePosition.x;
                pos.z -= TileSize;
            }
        }
        /// <summary>
        /// Возвращает Tile, на который указывает мышь.
        /// </summary>
        public static Tile GetTile()
        {
            Vector3 pos = MouseToWorld();

            return GetTile(pos.x, pos.z);
        }
        /// <summary>
        /// Возвращает Tile, который находится по указанным координатам в пределах TileSize.
        /// </summary>
        /// <param name="x">Координата X.</param>
        /// <param name="z">Координата Z.</param>
        public static Tile GetTile(float x, float z)
        {
            if (x < -MapSize.x / 2 || x > MapSize.x / 2 || z < -MapSize.y / 2 || z > MapSize.y / 2)
                throw new System.ArgumentOutOfRangeException("x = " + x + "; z = " + z);

            int X = 
                (int)((x + MapSize.x / 2.0f) / TileSize);
            int Z = lengthZ - 1 - 
                (int)((z + MapSize.y / 2.0f) / TileSize);
            
            return Tiles[Z, X];
        }
        /// <summary>
        /// Возвращает Tile, который находится по указанным в векторе координатам в пределах TileSize.
        /// </summary>
        /// <param name="position">Вектор с координатами X и Z.</param>
        /// <returns></returns>
        public static Tile GetTile(Vector3 position)
        {
            return GetTile(position.x, position.z);
        }
        /// <summary>
        /// Возвращает область Tile'ов.
        /// </summary>
        /// <param name="upLeftTilePosition">Позиция верхнего левого Tile'а области.</param>
        /// <param name="countX">Длина области по оси X.</param>
        /// <param name="countZ">Ширина области по оси Z.</param>
        /// <returns>Заданную область.</returns>
        public static Tile[,] GetTilesInArea(Vector3 upLeftTilePosition, int countX, int countZ)
        {
            Tile[,] tiles = new Tile[countZ, countX];

            Vector3 pos = upLeftTilePosition;
            for (int i = 0; i < countZ; i++)
            {
                for (int j = 0; j < countX; j++)
                {
                    tiles[i, j] = GetTile(pos);
                    pos.x += TileSize;
                }
                pos.x = upLeftTilePosition.x;
                pos.z -= TileSize;
            }

            return tiles;
        }       
        /// <summary>
        /// Проверяет, является ли объект-Owner этого Tile'а определённого типа.
        /// </summary>
        /// <param name="type">Проверяемый тип.</param>
        /// <param name="position">Позиция Tile'а, на котором расположен объект.</param>
        /// <returns></returns>
        public static bool IsOwnerTypeOf(System.Type type, Vector3 position)
        {
            Tile tile = GetTile(position);

            return tile.Owner.GetType().IsSubclassOf(type);
        }
        /// <summary>
        /// Возвращает реальную точку-координату мира, на которую указывает мышь.
        /// </summary>
        public static Vector3 MouseToWorld()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
                return hit.point;
            else
                throw new System.ArgumentOutOfRangeException("Map.MouseToWorld(): Mouse pointing nowhere.");
        }
    }

    /// <summary>
    /// Структурная еденица Карты Мира.
    /// </summary>
    public class Tile
    {
        public enum Types
        {
            Ground, Water
        }
        /// <summary>
        /// Создает Tile на указанной позиции в Мире.
        /// </summary>
        /// <param name="position">Позиция в мире, на которой будет распологаться этот Tile.</param>
        public Tile(Vector3 position)
        {
            Position = position;
        }

        /// <summary>
        /// Объект, который занял этот Tile.
        /// </summary>
        public Object Owner
        {
            get; set;
        }
        /// <summary>
        /// Позиция в мире.
        /// </summary>
        public Vector3 Position
        {
            get; private set;
        }
        /// <summary>
        /// Тип.
        /// </summary>
        public Types Type
        {
            get; set;
        }

        /// <summary>
        /// Возвращает информацию о позиции и владельце этого Tile'а.
        /// </summary>
        public override string ToString()
        {
            string pos = Position.ToString();
            string own = Owner == null ? "Empty" : Owner.name;
            return pos + " | " + own;
        }
    }
}
