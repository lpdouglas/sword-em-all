using UnityEngine;

namespace TGM
{
    public class Player : MonoBehaviour
    {
        public float speed = 10;
        public PlayerInput input;
        Vector3 collisionBox = new Vector3(1, 0.5f, 0.5f);


        void Update()
        {            
            if (input.x != 0)
            {
                float speedDelta = Time.deltaTime * speed * input.x;
                LayerMask cornerLine = 1 << 7;
                if (!Physics.CheckBox(transform.position + transform.right * speedDelta, collisionBox, transform.rotation, cornerLine))
                    transform.position +=  transform.right * speedDelta;
            }
        }
    }
}