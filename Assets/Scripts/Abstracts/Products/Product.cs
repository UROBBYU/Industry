using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Industry.Abstracts.Products
{
    public class Product
    {
        public Product(string name)
        {
            Name = name;
        }

        public string Name
        {
            get; private set;
        }


        
    }
}
