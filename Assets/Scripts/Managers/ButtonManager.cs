using System;
using System.Collections.Generic;
using UnityEngine;
using Industry.UI.Elements;

namespace Industry.Managers
{
    public class ButtonManager : MonoBehaviour
    {
        public void StartBuildingManager()
        {
            ToolBar.Components.DisableButtons();
            BuildingManager.Enable();
        }
        public void StartDestructionManager()
        {
            ToolBar.Components.DisableButtons();
            DestructionManager.Enable();
        }
        public void StartRoadManager()
        {
            ToolBar.Components.DisableButtons();
            RoadManager.Enable();
        }
        public void StartRoutingManager()
        {
            ToolBar.Components.DisableButtons();
            RoutingManager.Enable();
        }
        
        void Update()
        {
            BuildingManager.Update();
            DestructionManager.Update();
            RoadManager.Update();
            RoutingManager.Update();
        }
    }
}
