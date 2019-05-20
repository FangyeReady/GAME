using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{

    [SerializeField] Transform player;
   
    void LateUpdate()
    {
        this.transform.position = player.position;
    }
}
