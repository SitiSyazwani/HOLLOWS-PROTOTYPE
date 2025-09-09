using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;
    public Vector3 camOffset;

    // using lateUpdate cause it will be executed after all update() done.
    void LateUpdate()
    {
        if (player != null) 
        { 
            transform.position = player.position + camOffset;
        }
    }
}
