using UnityEngine;

namespace Spaceman.ConsoleCommands
{
    /// <summary>
    /// A simple free camera to be added to a Unity game object.
    /// 
    /// Full credit to Ashley Davis on GitHub for the inital code:
    /// https://gist.github.com/ashleydavis/f025c03a9221bc840a2b
    /// 
    /// </summary>
    public class Freecam : MonoBehaviour
    {
        /// <summary>
        /// Normal speed of camera movement.
        /// </summary>
        static public float MovementSpeed = 10f;

        /// <summary>
        /// Speed of camera movement when shift is held down.
        /// </summary>
        static public float FastMovementSpeed = 100f;

        /// <summary>
        /// Sensitivity for free look.
        /// </summary>
        static public float FreeLookSensitivity = 3f;

        /// <summary>
        /// Amount to zoom the camera when using the mouse wheel.
        /// </summary>
        static public float ZoomSensitivity = 10f;

        /// <summary>
        /// Amount to zoom the camera when using the mouse wheel (fast mode).
        /// </summary>
        static public float FastZoomSensitivity = 50f;

        public bool IsActive = false;

        void Update()
        {
            if (!IsActive)
            {
                return;
            }

            var fastMode = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            var movementSpeed = fastMode ? FastMovementSpeed : MovementSpeed;

            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            {
                transform.position += (-transform.right * movementSpeed * Time.deltaTime);
            }

            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            {
                transform.position += (transform.right * movementSpeed * Time.deltaTime);
            }

            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            {
                transform.position += (transform.forward * movementSpeed * Time.deltaTime);
            }

            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            {
                transform.position += (-transform.forward * movementSpeed * Time.deltaTime);
            }

            if (Input.GetKey(KeyCode.Q))
            {
                transform.position += (transform.up * movementSpeed * Time.deltaTime);
            }

            if (Input.GetKey(KeyCode.E))
            {
                transform.position += (-transform.up * movementSpeed * Time.deltaTime);
            }

            if (Input.GetKey(KeyCode.R) || Input.GetKey(KeyCode.PageUp))
            {
                transform.position += (Vector3.up * movementSpeed * Time.deltaTime);
            }

            if (Input.GetKey(KeyCode.F) || Input.GetKey(KeyCode.PageDown))
            {
                transform.position += (-Vector3.up * movementSpeed * Time.deltaTime);
            }

            float newRotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * FreeLookSensitivity;
            float newRotationY = transform.localEulerAngles.x - Input.GetAxis("Mouse Y") * FreeLookSensitivity;
            transform.localEulerAngles = new Vector3(newRotationY, newRotationX, 0f);

            float axis = Input.GetAxis("Mouse ScrollWheel");
            if (axis != 0)
            {
                var zoomSensitivity = fastMode ? FastZoomSensitivity : ZoomSensitivity;
                transform.position += transform.forward * axis * zoomSensitivity;
            }
        }
    }
}