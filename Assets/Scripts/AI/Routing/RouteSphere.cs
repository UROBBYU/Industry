using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Industry.AI.Routing
{
    class RouteSphere
    {
        public RouteSphere(Vector3 pos)
        {
            sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);

            material = sphere.GetComponent<MeshRenderer>().material;
            material.color = Color.green;

            transform = sphere.transform;
            transform.position = pos;
        }
        public RouteSphere(Vector3 pos, float size, Color color)
        {
            sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);

            material = sphere.GetComponent<MeshRenderer>().material;
            material.color = Color.green;

            transform = sphere.transform;
            transform.position = pos;

            SetSize(size);
            SetColor(color);
        }

        private GameObject sphere;
        private Transform transform;
        private Material material;

        public void Destroy()
        {
            UnityEngine.Object.Destroy(sphere.gameObject);
        }
        public void SetColor(Color col)
        {
            material.color = col;
        }
        public void SetPosition(Vector3 pos)
        {
            transform.position = pos;
        }
        public void SetSize(float size)
        {
            if (size > 0)
                transform.localScale *= size;
        }
    }
}
