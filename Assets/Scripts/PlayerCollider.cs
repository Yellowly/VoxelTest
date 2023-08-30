using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollider : MonoBehaviour
{
    public AABBox boundingBox;
    private PlayerController myPlayer;
    public Vector3 collisionVec;

    public bool collided { get; private set; }
    public bool touchingGround { get; private set; }

    public List<ChunkCollider> collidingChunks;

    public Vector3 boundExtents;
    public AABBox localBoundingBox;

    // Start is called before the first frame update
    void Start()
    {
        myPlayer = GetComponent<PlayerController>();
        localBoundingBox = new AABBox(-boundExtents, boundExtents);
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void FixedUpdate()
    {

    }

    public void CalcCollisions()
    {
        boundingBox = new AABBox(transform.position -boundExtents, transform.position + boundExtents);
        collidingChunks = CheckBoundCollisions();
        collided = false;
        touchingGround = false;
        if (collidingChunks.Count > 0)
        {
            Debug.DrawLine(boundingBox.min, new Vector3(boundingBox.min.x, boundingBox.min.y, boundingBox.max.z), Color.red);
            Debug.DrawLine(boundingBox.min, new Vector3(boundingBox.min.x, boundingBox.max.y, boundingBox.min.z), Color.red);
            Debug.DrawLine(boundingBox.min, new Vector3(boundingBox.max.x, boundingBox.min.y, boundingBox.min.z), Color.red);
            Debug.DrawLine(boundingBox.max, new Vector3(boundingBox.max.x, boundingBox.max.y, boundingBox.min.z), Color.red);
            Debug.DrawLine(boundingBox.max, new Vector3(boundingBox.max.x, boundingBox.min.y, boundingBox.max.z), Color.red);
            Debug.DrawLine(boundingBox.max, new Vector3(boundingBox.min.x, boundingBox.max.y, boundingBox.max.z), Color.red);
            foreach(ChunkCollider c in collidingChunks)
            {
                collided = ResolveCollisionWithChunk(c);
            }
        }
        else
        {
            Debug.DrawLine(boundingBox.min, new Vector3(boundingBox.min.x, boundingBox.min.y, boundingBox.max.z));
            Debug.DrawLine(boundingBox.min, new Vector3(boundingBox.min.x, boundingBox.max.y, boundingBox.min.z));
            Debug.DrawLine(boundingBox.min, new Vector3(boundingBox.max.x, boundingBox.min.y, boundingBox.min.z));
            Debug.DrawLine(boundingBox.max, new Vector3(boundingBox.max.x, boundingBox.max.y, boundingBox.min.z));
            Debug.DrawLine(boundingBox.max, new Vector3(boundingBox.max.x, boundingBox.min.y, boundingBox.max.z));
            Debug.DrawLine(boundingBox.max, new Vector3(boundingBox.min.x, boundingBox.max.y, boundingBox.max.z));

        }


    }

    private List<ChunkCollider> CheckBoundCollisions()
    {
        List<ChunkCollider> collidingChunks = new List<ChunkCollider>();
        foreach (ChunkCollider c in ChunkCollider.allChunkColliders)
        {
            if (c != this && CheckBoundCollision(c)) collidingChunks.Add(c);
        }
        return collidingChunks;
    }
    private bool CheckBoundCollision(ChunkCollider other)
    {
        return (boundingBox+Vector3.one*0.5f).CheckCollision(other.boundingBox);
    }
    private bool ResolveCollisionWithChunk(ChunkCollider other)
    {
        List<CollisionPoint> otherCollisionPointsInBounds = new List<CollisionPoint>();
        AABBox boxToOther = boundingBox.ToTransform(other.transform);
        foreach (CollisionPoint p in other.collisionPoints)
        {
            // if (CheckPointBoundCollision(other.transform.TransformPoint(p.point))) otherCollisionPointsInBounds.Add(new CollisionPoint(other.transform, p));
            // if ((boundingBox + Vector3.one * 0.45f).CheckCollision(other.transform.TransformPoint(p.point)) && boundingBox.CheckCollision(new CollisionPoint(other.transform, p))) otherCollisionPointsInBounds.Add(new CollisionPoint(other.transform, p));

            // CollisionPoint pt = p.TransformPoint(other.transform, transform);
            // if ((localBoundingBox + Vector3.one * 0.45f).CheckCollision(pt.point) && localBoundingBox.CheckCollision(pt)) otherCollisionPointsInBounds.Add(new CollisionPoint(other.transform, p));

            if ((boxToOther + Vector3.one * 0.45f).CheckCollision(p.point) && boxToOther.CheckCollision(p)) otherCollisionPointsInBounds.Add(new CollisionPoint(other.transform, p));

        }
        collisionVec = Vector3.zero;
        for(int i = 0; i < otherCollisionPointsInBounds.Count; i++)
        {
            Debug.DrawLine(otherCollisionPointsInBounds[i].point, otherCollisionPointsInBounds[i].point + otherCollisionPointsInBounds[i].normal, Color.yellow);
            //Debug.Log(otherCollisionPointsInBounds[i].normal * ChunkCollider.PointToPlaneTest(ClosestPointOnSurface(otherCollisionPointsInBounds[i]), otherCollisionPointsInBounds[i]));
            //transform.position -= otherCollisionPointsInBounds[i].normal * ChunkCollider.PointToPlaneTest(transform.TransformPoint(new AABBox(-boundExtents,boundExtents).ClosestPointOnSurface(otherCollisionPointsInBounds[i].ToTransform(transform))), otherCollisionPointsInBounds[i]);
            transform.position -= otherCollisionPointsInBounds[i].normal * ChunkCollider.PointToPlaneTest(other.transform.TransformPoint(boxToOther.ClosestPointOnSurface(otherCollisionPointsInBounds[i].ToTransform(other.transform))), otherCollisionPointsInBounds[i]);

            //collisionVec += Vector3.Scale(otherCollisionPointsInBounds[i].normal, myPlayer.velocity);
            //Debug.Log("normal = " + otherCollisionPointsInBounds[i].normal);
            // Debug.Log("normal = " + otherCollisionPointsInBounds[i].normal + " scale = " + Vector3.Scale(otherCollisionPointsInBounds[i].normal, myPlayer.velocity));

            myPlayer.velocity -= Vector3.Dot(otherCollisionPointsInBounds[i].normal, myPlayer.velocity) < 0 ? Vector3.Dot(otherCollisionPointsInBounds[i].normal, myPlayer.velocity)* otherCollisionPointsInBounds[i].normal : Vector3.zero;

            /*if (boundingBox.CheckCollision(otherCollisionPointsInBounds[i].point))
            {
                transform.position -= otherCollisionPointsInBounds[i].normal * ChunkCollider.PointToPlaneTest(ClosestPointOnSurface(otherCollisionPointsInBounds[i]), otherCollisionPointsInBounds[i]);
                //collisionVec += Vector3.Scale(otherCollisionPointsInBounds[i].normal, myPlayer.velocity);
                //Debug.Log("normal = " + otherCollisionPointsInBounds[i].normal);
                // Debug.Log("normal = " + otherCollisionPointsInBounds[i].normal + " scale = " + Vector3.Scale(otherCollisionPointsInBounds[i].normal, myPlayer.velocity));

                myPlayer.velocity -= Vector3.Dot(otherCollisionPointsInBounds[i].normal, myPlayer.velocity) < 0 ? Vector3.Scale(otherCollisionPointsInBounds[i].normal, myPlayer.velocity) : Vector3.zero;
            }*/
            if (otherCollisionPointsInBounds[i].normal.y > 0.2f) touchingGround = true;
        }
        return otherCollisionPointsInBounds.Count > 0;
    }
    private bool CheckPointBoundCollision(Vector3 point)
    {
        return boundingBox.min.x <= point.x && boundingBox.max.x >= point.x &&
            boundingBox.min.y <= point.y && boundingBox.max.y >= point.y &&
            boundingBox.min.z <= point.z && boundingBox.max.z >= point.z;
    }

    private bool CheckPlaneBoundCollision(CollisionPoint p)
    {
        // Convert AABB to center-extents representation
        Vector3 center = (boundingBox.max + boundingBox.min) * 0.5f; // Compute AABB center
        Vector3 extents = boundingBox.max - center; // Compute positive extents

        // Compute the projection interval radius of b onto L(t) = b.c + t * p.n

        // float r = extends.x * Abs(p.normal.x) + extents.y * Abs(p.normal.y) + extents.z * Abs(p.normal.z);
        float projRadius = Vector3.Dot(extents,ChunkCollider.AbsVector(p.normal));
        // Compute distance of box center from plane
        float dist = Vector3.Dot(p.normal, center) - Vector3.Dot(p.point,p.normal);
        // Intersection occurs when distance s falls within [-r,+r] interval
        return Mathf.Abs(dist) <= projRadius;
    }
    
    private Vector3 ClosestPointOnSurface(CollisionPoint p)
    {
        Vector3 point = p.point - p.normal;
        return ChunkCollider.ClampVector(point, boundingBox.min, boundingBox.max);

    }
}