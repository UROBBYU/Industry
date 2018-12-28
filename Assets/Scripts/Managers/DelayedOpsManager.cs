using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Industry.World;
using Industry.World.Buildings;
using Industry.World.Map;
using Industry.UI.Elements;
using Industry.World.Roads;

namespace Industry.Managers
{
    public class DelayedOpsManager : MonoBehaviour
    {
        private class Operation
        {
            public Operation(Action op)
            {
                can_execute = false;
                executed = false;
                action = op;
            }

            public bool can_execute;
            public bool executed;
            public Action action;
        }

        private List<Operation> delayedOps;

        void Awake()
        {
            delayedOps = new List<Operation>();
            manager = this;
        }

        void LateUpdate()
        {
            foreach (Operation op in delayedOps)
            {
                //if (op.can_execute)
                //{
                //    op.action();
                //    op.executed = true;
                //}
                //else
                //{
                //    op.can_execute = true;
                //}
                op.action();
            }
            delayedOps.Clear();
            //delayedOps.RemoveAll((op) => op.executed);
        }

        private IEnumerator DelayedAction(Action action)
        {
            yield return new WaitForEndOfFrame();

            action();

            
        }

        private static DelayedOpsManager manager;
        public static void AddOperation(Action operation)
        {
            if (manager != null)
                manager.delayedOps.Add(new Operation(operation));
        }

    }
}
