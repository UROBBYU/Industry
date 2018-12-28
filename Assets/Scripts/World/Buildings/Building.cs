using System;
using System.Collections.Generic;
using UnityEngine;
using Industry.World.Map;

namespace Industry.World.Buildings
{
    public enum BuildingColor
    {
        Default, Red
    }

    public abstract class Building : MonoBehaviour
    {
        /*
        public TileArea Area
        {
            get
            {
                float mts2 = Map.TileSize / 2;

                float X = transform.position.x - mts2 * (countX - 1);
                float Z = transform.position.z + mts2 * (countZ - 1);
                
                return new TileArea(Map.GetTile(X, Z), countX, countZ);
            }
        }
        */
        
        public BuildingColor color;

        public int code;
        public int countX;
        public int countZ;
        private Vector3 upLeftOffset;

        public Vector3 UpLeft
        {
            get { return transform.position + upLeftOffset; }
        }
        public Sprite sprite
        {
            get { return GetComponent<SpriteRenderer>().sprite; }
            protected set { GetComponent<SpriteRenderer>().sprite = value; }
        }
        
        void Awake()
        {
            if (code < 1)
                throw new ArgumentException(gameObject.name + ": Code is not set!");
            if (countX == 0 || countZ == 0)
                throw new ArgumentException(gameObject.name + ": Size is not set!");

            SetUpLeftOffset();
            _Awake();
        }
        void Start()
        {
            
            _Start();
        }
        void Update()
        {
            
            _Update();
        }
        
        public void SetPosition(Vector3 pos)
        {
            int mX = countX % 2 == 0 ? 1 : 0;
            int mZ = countZ % 2 == 0 ? 1 : 0;
            float mts2 = WorldMap.TileSize / 2;

            float X = pos.x + mts2 * mX;
            float Z = pos.z + mts2 * mZ;

            transform.position = new Vector3(X, pos.y, Z);
        }

        private void SetUpLeftOffset()
        {
            float mts2 = WorldMap.TileSize / 2;

            float X = mts2 * (countX - 1);
            float Z = mts2 * (countZ - 1);

            upLeftOffset = new Vector3(-X, 0, Z);
        }
        
        public virtual void Rotate(bool clockwise, Type building)
        {
            int temp = countX;
            countX = countZ;
            countZ = temp;

            float angle = clockwise ? 90 : -90;


            transform.rotation = building == typeof(Depot) ?
                Quaternion.Euler(270.0f, Mathf.RoundToInt(transform.rotation.eulerAngles.y + angle), 0.0f) :
                Quaternion.Euler(90.0f,  Mathf.RoundToInt(transform.rotation.eulerAngles.y + angle), 0.0f);

            SetUpLeftOffset();
        }
        
        public abstract void SetColor(BuildingColor color);
        protected abstract void _Awake();
        protected abstract void _Start();
        protected abstract void _Update();
    }
}
