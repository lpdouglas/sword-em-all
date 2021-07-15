using UnityEngine;

namespace Game
{
    public class Player : MonoBehaviour
    {
        public float speed = 10;
        public PlayerInput input;

        void Update()
        {            
            if (input.x != 0)
            {
                float speedDelta = Time.deltaTime * speed * input.x;
                transform.position +=  transform.right * speedDelta;
            }
        }
    }
}