using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OwnerChunk : MonoBehaviour
{
    public Chunk ownerChunk;
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("non convex collided with "+collision.collider);
    }
}
