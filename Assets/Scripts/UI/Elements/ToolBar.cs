using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Industry.UI.Elements
{
    public class ToolBar : MonoBehaviour
    {
        public static ToolBar Components
        {
            get; set;
        }

        public static bool IsShown
        {
            get
            {
                return Components.shown;
            }
            set
            {
                if (value != Components.shown)
                {
                    Components.shown = value;
                    Components.gameObject.SetActive(value);
                }
            }
        }

        private bool shown;

        public Sprite Background
        {
            get { return GetComponentInChildren<SpriteRenderer>().sprite; }
            set { GetComponentInChildren<SpriteRenderer>().sprite = value; }
        }
        
        public Button Road
        {
            get; private set;
        }
        public Button PathSelector
        {
            get; private set;
        }



        private Button[] buttons;


        void Start()
        {
            Components = this;
            buttons = GetComponentsInChildren<Button>();

            foreach (Button B in buttons)
            {
                if (B.gameObject.name.Equals("Road"))
                    Road = B;
                else if (B.gameObject.name.Equals("Path Selector"))
                    PathSelector = B;
            }

            IsShown = false;
            gameObject.SetActive(false);
        }
        
        public void EnableButtons()
        {
            foreach (Button b in buttons)
                if (!b.interactable)
                    b.interactable = true;
        }
        public void DisableButtons()
        {
            foreach (Button b in buttons)
                if (b.interactable)
                    b.interactable = false;
        }

        public void SetTransparency(float value)
        {
            if (value < 0.0f || value > 1.0f)
                throw new ArgumentOutOfRangeException("Transparency value must be from 0 to 1.");

            var color = GetComponentInChildren<SpriteRenderer>().color;
            color.a = value;
            GetComponentInChildren<SpriteRenderer>().color = color;
        }
    }
}
