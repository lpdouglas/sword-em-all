using Mirror;
using UnityEngine;

namespace Game
{
    public class Player : NetworkBehaviour
    {
        public float speed = 10;
        Vector3 startPosition;
        float horizontalPosition;

        float input_x = 0;

        private void Start() => startPosition = transform.position;

        void Update()
        {
            if (isLocalPlayer) { 
                input_x = Input.GetAxis("Horizontal");
            }

            if (input_x != 0)
            {
                float speedDelta = Time.deltaTime * speed * input_x;
                horizontalPosition += speedDelta;
            }
            transform.position = startPosition + transform.right * horizontalPosition;
                        
        }

    }
}