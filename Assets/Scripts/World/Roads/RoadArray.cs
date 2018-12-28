using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Industry.World.Roads
{
    public class RoadArray
    {
        public RoadArray(int length)
        {
            array = new Road[length];
            Length = 0;
        }

        public int Length
        {
            get; private set;
        }

        private Road[] array;

        public void Add(Road item)
        {
            if (item == null) return;

            array[Length] = item;
            
            Length++;
        }
        /*
        public void UpdateRoads()
        {
            for (int i = 0; i < Length; i++)
                array[i].UpdateStatement();
        }
        */
        public Road this[int index]
        {
            get
            {
                if (index < 0 || index > Length)
                    throw new System.IndexOutOfRangeException("Road array: Length = " + Length + "; index = " + index);
                return array[index];
            }
        }
    }
}
