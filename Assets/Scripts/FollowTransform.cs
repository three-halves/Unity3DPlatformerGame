using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTransform : MonoBehaviour
{
    [SerializeField] Transform toFollow;

    // Update is called once per frame
    void Update()
    {
        transform.position = toFollow.position;
    }
}
