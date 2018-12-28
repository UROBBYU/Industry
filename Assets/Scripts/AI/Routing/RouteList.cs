using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Industry.World.Roads;

namespace Industry.AI.Routing
{
    class RouteList
    {
        private class Node
        {
            public Node(Road road, int index)
            {
                this.road = road;
                this.index = index;
            }

            public int index;
            public Road road;

            public Node next;
            public Node prev;
        }
        
        private Node first, last;

        public int Count
        {
            get; private set;
        }

        public void Add(Road road)
        {
            if (first == null)
            {
                first = new Node(road, 0);
                last = first;
            }
            else
            {
                Node _last = last;
                last = new Node(road, _last.index + 1);

                _last.next = last;
                last.prev = _last;
            }
            Count++;
        }
        
        private Road Get(int index)
        {
            if (index < 0 || index >= Count)
                throw new System.IndexOutOfRangeException();

            if (index < Count / 2)
            {
                Node current = first;

                while (current.index != index)
                    current = current.next;

                return current.road;
            }
            else
            {
                Node curr = last;

                while (curr.index != index)
                    curr = curr.prev;

                return curr.road;
            }
        }
        
        public Road this[int index]
        {
            get
            {
                return Get(index);
            }
        }
    }
}
