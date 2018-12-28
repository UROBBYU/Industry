using System;
using System.Collections.Generic;
using UnityEngine;
using Industry.Abstracts.Products;

namespace Industry.World.Buildings
{
    public class Factory : EntranceBuilding
    {
        public int cost;
        
        public Product[] products;


        protected override void _Start()
        {
            base._Start();

            products = new Product[1];
        }
        protected override void _Update()
        {
            base._Update();


        }
    }
}
