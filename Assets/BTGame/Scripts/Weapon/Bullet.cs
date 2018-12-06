using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    float bulletSpeed = 100; // You can use int as well but float is more accurate

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        transform.position = (new Vector3(0, -bulletSpeed * Time.deltaTime, 0));
        //transform.position += new Vector3(0, -bulletSpeed * Time.deltaTime, 0);
        Destroy(gameObject, 3); // Destroying the game object that this script is attached to after 3 seconds
    }
}
