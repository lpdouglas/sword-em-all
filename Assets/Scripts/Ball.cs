using UnityEngine;

namespace Game
{
    public class Ball : MonoBehaviour
    {
        Rigidbody rb;
        public LayerMask floor;
        float speed = 10;
        bool isBouncing;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.AddForce(transform.forward * 50 * rb.mass, ForceMode.Impulse);
        }

        void FixedUpdate()
        {
            if (!isBouncing) rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z).normalized * speed + new Vector3(0, rb.velocity.y, 0);
            
        }


        void OnCollisionEnter(Collision collision)
        {
            if ( 1<<collision.gameObject.layer != floor )
            {
                float bounce = rb.mass * speed * 50; //amount of force to apply
                rb.AddForce(collision.contacts[0].normal * bounce);
                isBouncing = true;
                Invoke("StopBounce", 0.3f);
            }
        }
        void StopBounce()
        {
            isBouncing = false;
        }
    }
}