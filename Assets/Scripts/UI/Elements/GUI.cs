using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Industry.UI.Elements
{
    public class GUI : MonoBehaviour
    {
        public GameObject sphere;
        public GameObject cube;

        public Camera cam;

        private MethodInfo clear;

        private float elapsed;
        private int frames;
        private int fps;

        void Start()
        {
            var assembly = Assembly.GetAssembly(typeof(UnityEditor.Editor));
            var type = assembly.GetType("UnityEditor.LogEntries");
            //clear = type.GetMethod("Clear");
        }
        void OnGUI()
        {
            ShowGUI();
        }
        void Update()
        {
            UpdateFPS();
        }

        private void ClearLog()
        {
            clear.Invoke(new object(), null);
        }
        private void ShowGUI()
        {
            GUILayout.BeginArea(new Rect(20, 20, 250, 120));
            GUILayout.Label("Resolution: " + Camera.main.pixelWidth + "x" + Camera.main.pixelHeight);
            GUILayout.Label("FPS: " + fps);
            GUILayout.EndArea();
        }
        private void UpdateFPS()
        {
            frames++;
            elapsed += Time.deltaTime;

            if (elapsed >= 1f)
            {
                fps = Mathf.RoundToInt(frames / elapsed);

                frames = 0;
                elapsed = 0;
            }
        }
    }
}