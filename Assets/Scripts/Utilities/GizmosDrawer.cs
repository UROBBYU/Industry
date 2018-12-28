using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Industry.Utilities
{
    public class GizmosDrawer : MonoBehaviour
    {
        private static List<KeyValuePair<Color, Vector3[]>> vecPairs = new List<KeyValuePair<Color, Vector3[]>>();


        public static void AddVectorPair(Vector3[] pair, Color color)
        {
            if (pair.Length != 2)
                return;

            vecPairs.Add(new KeyValuePair<Color, Vector3[]>(color, pair));
        }

        public static void RemoveVectorPair(Vector3[] pair)
        {
            vecPairs.Remove(vecPairs.Find(x => x.Value[0] == pair[0] && x.Value[1] == pair[1]));
        }
        
        public static void Clear()
        {
            vecPairs.Clear();
        }

        void OnDrawGizmos()
        {
            foreach (var pair in vecPairs)
            {
                Gizmos.color = pair.Key;
                Gizmos.DrawLine(pair.Value[0], pair.Value[1]);
            }
        }

    }
}
