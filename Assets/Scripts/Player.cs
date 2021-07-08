using UnityEngine;

public class Player : MonoBehaviour
{
    public Bullet bulletPrefab;
    public GameObject bulletPosition;
    public float speed = 10;

    float x = 0;
    float z = 0;
    bool fire;
    float fireLastTime;
    float fireInterval = 0.5f;
    
    
    
    void Start()
    {
        
    }

    
    void Update()
    {
        x = Input.GetAxis("Horizontal");
        z = Input.GetAxis("Vertical");
        fire = Input.GetButton("Fire1");

        
        float speedDelta = Time.deltaTime * speed;
        if (x!=0 || z!=0)
        {
            Vector3 movement = new Vector3(x * speedDelta, 0, z * speedDelta);
            transform.position = transform.position + movement;
            transform.rotation = Quaternion.LookRotation(movement);            
        }

        if (fire && fireLastTime < Time.time)
        {
            fireLastTime = Time.time + fireInterval;
            Instantiate(bulletPrefab, bulletPosition.transform.position, bulletPosition.transform.rotation);
        }
    }

}
