using UnityEngine;

public class Player : MonoBehaviour
{

    float speed = 5;
    float x = 0;
    float z = 0;
    
    void Start()
    {
        
    }

    
    void Update()
    {
        x = Input.GetAxis("Horizontal");
        z = Input.GetAxis("Vertical");

        
        float speedDelta = Time.deltaTime * speed;
        if (speedDelta>0)
        {
            transform.position = transform.position + new Vector3(x * speedDelta, 0, z * speedDelta);            
        }
    }

}
