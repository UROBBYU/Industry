using UnityEngine;
using Industry.World.Map;
using System.Reflection;

namespace Industry.UI
{
    [RequireComponent(typeof(Camera))]
    public class MainCamera : MonoBehaviour
    {
        private bool AtTop
        {
            get { return Input.mousePosition.y >= Screen.height - 4.0f; }
        }
        private bool AtBottom
        {
            get { return Input.mousePosition.y <= 4.0f; }
        }
        private bool AtRight
        {
            get { return Input.mousePosition.x >= Screen.width - 4.0f; }
        }
        private bool AtLeft
        {
            get { return Input.mousePosition.x <= 4.0f; }
        }

        private float last_zoom = 0;
        private float zoom_decrement;

        public float minHeight = 5f;
        public float maxHeight = 50f;
        public float moveSpeed = 1f;
        public float zoomSpeed = 1f;
        public float zoomGravity = 0.5f;
        public bool moving = true;
        

        void Update()
        {
            CheckRotation();

            if (moving)
                CheckMovement();

            CheckZoom();
        }

        private void CheckRotation()
        {
            if (Input.GetMouseButton(1))
            {
                float rotation    = Input.GetAxis("Mouse X");
                float inclination = Input.GetAxis("Mouse Y");

                transform.Rotate(new Vector3(-inclination, rotation, 0));

                float x = transform.rotation.eulerAngles.x;
                if (x > 269f) x -= 360f;
                if (x < 10f) x = 10f;
                else if (x > 80f) x = 80f;

                transform.rotation = Quaternion.Euler(new Vector3(x, transform.rotation.eulerAngles.y, 0));
            }
        }
        
        private void CheckMovement()
        {
            Vector3 direction = new Vector3();

            bool at_top = AtTop, at_bottom = AtBottom, at_right = AtRight, at_left = AtLeft;
            float limitX = -WorldMap.UpLeft.x, limitZ = WorldMap.UpLeft.z;

            direction.x = at_left ? -1 : at_right ? 1 : 0;
            direction.z = at_top ? 1 : at_bottom ? -1 : 0;
            
            direction = Quaternion.Euler(new Vector3(0f, transform.eulerAngles.y, 0f)) * (direction * moveSpeed * Time.deltaTime);
            direction = transform.InverseTransformDirection(direction);

            transform.Translate(direction, Space.Self);
            transform.position = new Vector3(
                Mathf.Clamp(transform.position.x, -limitX, limitX), 
                transform.position.y, 
                Mathf.Clamp(transform.position.z, -limitZ, limitZ));
            
        }
        
        private void CheckZoom()
        {
            bool zoom_num_in  = Input.GetKeyDown(KeyCode.KeypadPlus)  || Input.GetKey(KeyCode.KeypadPlus)  || Input.GetKeyDown(KeyCode.Plus)  || Input.GetKey(KeyCode.Plus);
            bool zoom_num_out = Input.GetKeyDown(KeyCode.KeypadMinus) || Input.GetKey(KeyCode.KeypadMinus) || Input.GetKeyDown(KeyCode.Minus) || Input.GetKey(KeyCode.Minus);
            
            float zoom = zoom_num_in ? 1 : zoom_num_out ? -1 : Input.GetAxis("Mouse ScrollWheel");
            
            if (zoom > 0)
                last_zoom = 10.0f;
            else if (zoom < 0)
                last_zoom = -10.0f;
            zoom_decrement = Mathf.Sign(last_zoom) * zoomGravity;

            Vector3 zPos = Vector3.forward * zoomSpeed * last_zoom * Time.deltaTime;

            if (last_zoom != 0f)
                transform.Translate(zPos);

            if (transform.position.y < minHeight || transform.position.y > maxHeight)
            {
                transform.Translate(-zPos);
                last_zoom = 0f;
            }

            if ((int)(last_zoom * 100) != 0)
                last_zoom -= zoom_decrement;
            else
                last_zoom = 0f;
        }
        
    }
}
