using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Component-Object Model (COM System)

public class Fire : MonoBehaviour
{
    //public GameObject bullet;
    //public Transform spawnPoint; // 'Transform' is the whole component of Position, Rotation, Scale

    public LineRenderer fireLine;
    public float shootRate = 5f;
    public float shootDelay = .1f;
    [Header("Reference")]
    public Transform start;
    public Transform end;

    // Update is called once per frame
    void Update()
    {
        
        //if (Input.GetKey(KeyCode.Mouse0)) // Mouse0 = Left Click, Mouse1 = Right Click, Mouse2 = Middle Mouse
        //{
        //    Instantiate(bullet, spawnPoint.position, spawnPoint.rotation);
        //}
        
        // If mouse is pressed
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            // Start the shoot process
            StartCoroutine(Shoot());
        }
    }
    private void LateUpdate()
    {
        // Set positions of line (start and end)
        fireLine.positionCount = 2;
        fireLine.SetPosition(0, start.position);
        fireLine.SetPosition(1, end.position);
    }

    // Function that runs seperate to the Update
    IEnumerator Shoot()
    {
        // Enable the line
        fireLine.enabled = true;

        // Wait a few seconds
        yield return new WaitForSeconds(shootDelay);

        // Disable the line
        fireLine.enabled = false;
    }
}
