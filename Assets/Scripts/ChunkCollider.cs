using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkCollider : MonoBehaviour
{
    public Chunk attachedChunk;
    public AABBox boundingBox;

    public Vector3 velocity;
    public Vector3 angularVelocity;
    public float mass;
    public Vector3 CoM;

    // use custom list
    public List<CollisionPoint>[,,] pointPartition;

    public static List<ChunkCollider> allChunkColliders;


    public List<CollisionPoint> collisionPoints;
    private List<CollisionPoint> worldCollisionPoints;

    private Vector3 worldCollisionPointLoc;

    public bool freezeXRot;
    public bool freezeYRot;
    public bool freezeZRot;

    private void Awake()
    {
        attachedChunk = GetComponent<Chunk>();
        attachedChunk.OnCreateMesh += SetupPointColliders;
    }

    void Start()
    {
        if (allChunkColliders == null) allChunkColliders = new List<ChunkCollider>();
        allChunkColliders.Add(this);
        // attachedChunk.OnCreateMesh += SetupPointColliders;
    }

    // Update is called once per frame
    void Update()
    {
        /*
        if (singleton == this)
        {
            
            //if (Input.GetKeyDown(KeyCode.P))
            //{
            //    CreateMesh();
            //}
            if (Input.GetKey(KeyCode.I))
            {
                rb.AddRelativeForce(Vector3.forward * 2000, ForceMode.Force);
                // Debug.Log("adding force");
            }
            if (Input.GetKey(KeyCode.J))
            {
                rb.AddRelativeTorque(Vector3.down * Time.deltaTime, ForceMode.VelocityChange);
            }
            if (Input.GetKey(KeyCode.K))
            {
                rb.AddRelativeForce(Vector3.back * 2000, ForceMode.Force);
            }
            if (Input.GetKey(KeyCode.L))
            {
                rb.AddRelativeTorque(Vector3.up * Time.deltaTime, ForceMode.VelocityChange);
            }
        }
        else
        {
            if (Input.GetKey(KeyCode.I))
            {
                rb.AddRelativeForce(Random.onUnitSphere * 2000, ForceMode.Force);
                // Debug.Log("adding force");
            }
        }

        rb.velocity = Vector3.ClampMagnitude(rb.velocity, 100);
        if (Input.GetKeyDown(KeyCode.N))
        {
            rb.drag = rb.drag == 1 ? 0 : 1;
        }*/
    }

    private void FixedUpdate()
    {
        boundingBox = new AABBox(transform.position, transform.TransformPoint(new Vector3(0,1,1) * attachedChunk.chunkEdgeLen), transform.TransformPoint(new Vector3(1, 0, 1) * attachedChunk.chunkEdgeLen), transform.TransformPoint(new Vector3(1, 1, 0) * attachedChunk.chunkEdgeLen));
        List<ChunkCollider> collidingChunks = CheckBoundCollisions();
        if (collidingChunks.Count > 0)
        {
            Debug.DrawLine(boundingBox.min, new Vector3(boundingBox.min.x, boundingBox.min.y, boundingBox.max.z), Color.red);
            Debug.DrawLine(boundingBox.min, new Vector3(boundingBox.min.x, boundingBox.max.y, boundingBox.min.z), Color.red);
            Debug.DrawLine(boundingBox.min, new Vector3(boundingBox.max.x, boundingBox.min.y, boundingBox.min.z), Color.red);
            Debug.DrawLine(boundingBox.max, new Vector3(boundingBox.max.x, boundingBox.max.y, boundingBox.min.z), Color.red);
            Debug.DrawLine(boundingBox.max, new Vector3(boundingBox.max.x, boundingBox.min.y, boundingBox.max.z), Color.red);
            Debug.DrawLine(boundingBox.max, new Vector3(boundingBox.min.x, boundingBox.max.y, boundingBox.max.z), Color.red);
            ResolveCollisionWithChunk(collidingChunks[0]);
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
        velocity = Vector3.ClampMagnitude(velocity, 100);
        transform.position += velocity * Time.fixedDeltaTime;
        //transform.rotation *= Quaternion.Euler(angularVelocity*Time.fixedDeltaTime);
        angularVelocity = new Vector3(freezeXRot ? 0 : angularVelocity.x, freezeYRot ? 0 : angularVelocity.y, freezeZRot ? 0 : angularVelocity.z);
        transform.RotateAround(transform.TransformPoint(CoM), angularVelocity.normalized, (Mathf.Abs(angularVelocity.x) + Mathf.Abs(angularVelocity.y) + Mathf.Abs(angularVelocity.z))*Time.fixedDeltaTime);
        /*
        if ((worldCollisionPointLoc - transform.position).magnitude > Mathf.Epsilon)
        {
            Debug.Log("moving!!");
            for (int i = 0; i < collisionPoints.Count; i++)
            {
                //worldCollisionPoints[i] = new CollisionPoint(transform,collisionPoints[i]);
            }
            worldCollisionPointLoc = transform.position;
        }
        for (int i = 0; i < collisionPoints.Count; i++)
        {
            //Debug.DrawLine(worldCollisionPoints[i].point, worldCollisionPoints[i].point + worldCollisionPoints[i].normal);
            Debug.DrawLine(transform.TransformPoint(collisionPoints[i].point), transform.TransformPoint(collisionPoints[i].point + collisionPoints[i].normal));
        }*/
    }

    private List<ChunkCollider> CheckBoundCollisions()
    {
        List<ChunkCollider> collidingChunks = new List<ChunkCollider>();
        foreach(ChunkCollider c in allChunkColliders)
        {
            if (c != this && CheckBoundCollision(c)) collidingChunks.Add(c);
        }
        return collidingChunks;
    }

    private bool CheckBoundCollision(ChunkCollider other)
    {
        return boundingBox.min.x <= other.boundingBox.max.x && boundingBox.max.x >= other.boundingBox.min.x &&
            boundingBox.min.y <= other.boundingBox.max.y && boundingBox.max.y >= other.boundingBox.min.y &&
            boundingBox.min.z <= other.boundingBox.max.z && boundingBox.max.z >= other.boundingBox.min.z;
    }
    private bool CheckPointBoundCollision(Vector3 point)
    {
        return boundingBox.min.x <= point.x && boundingBox.max.x >= point.x &&
            boundingBox.min.y <= point.y && boundingBox.max.y >= point.y &&
            boundingBox.min.z <= point.z && boundingBox.max.z >= point.z;
    }

    private void SetupPointColliders()
    {
        Debug.Log("setup!!!!");
        collisionPoints = new List<CollisionPoint>();
        pointPartition = new List<CollisionPoint>[attachedChunk.chunkEdgeLen, attachedChunk.chunkEdgeLen, attachedChunk.chunkEdgeLen];
        CoM = Vector3.zero;
        mass = 0;
        for (int x = 0; x < attachedChunk.chunkEdgeLen; x++)
        {
            for (int y = 0; y < attachedChunk.chunkEdgeLen; y++)
            {
                for (int z = 0; z < attachedChunk.chunkEdgeLen; z++)
                {
                    if(attachedChunk.CheckVoxel(new Vector3(x, y, z)))
                    {
                        Vector3Int pos = new Vector3Int(x, y, z);
                        CoM += pos;
                        mass++;
                        for (int i = 0; i < 6; i++)
                        {
                            if (!attachedChunk.CheckVoxel(pos + VoxelData.faceChecks[i]))
                            {
                                CollisionPoint c = new CollisionPoint(pos + (Vector3.one + VoxelData.faceChecks[i]) * 0.5f, VoxelData.faceChecks[i]);
                                collisionPoints.Add(c);
                                if (pointPartition[x, y, z] == null) pointPartition[x, y, z] = new List<CollisionPoint>();
                                pointPartition[x, y, z].Add(c);
                            }
                        }
                    }
                }
            }
        }
        CoM /= mass;
    }
    public static float PointToPlaneTest(Vector3 point, CollisionPoint plane)
    {
        return Vector3.Dot(plane.normal, point - plane.point);
    }

    private bool ResolveCollisionWithChunk(ChunkCollider other)
    {
        int count = 0;
        /*
        List<CollisionPoint> otherCollisionPointsInBounds = new List<CollisionPoint>();
        foreach(CollisionPoint p in other.collisionPoints)
        {
            if (CheckPointBoundCollision(other.transform.TransformPoint(p.point))) otherCollisionPointsInBounds.Add(new CollisionPoint(other.transform,p));
        }*/

        Vector3 transformedCOM = transform.TransformPoint(CoM);
        for(int i = 0; i < collisionPoints.Count; i++)
        {
            CollisionPoint myPoint = new CollisionPoint(transform, collisionPoints[i]);
            if(other.CheckPointBoundCollision(myPoint.point))
            {
                CollisionPoint myPointToOther = myPoint.ToTransform(other.transform);
                Vector3Int posOnOther = Vector3Int.FloorToInt(myPointToOther.point);
                if (posOnOther.x < 0 || posOnOther.x > other.attachedChunk.chunkEdgeLen-1 || posOnOther.y < 0 || posOnOther.y > other.attachedChunk.chunkEdgeLen - 1 || posOnOther.z < 0 || posOnOther.z > other.attachedChunk.chunkEdgeLen - 1) continue;

                CollisionPoint nearestCollidingPoint = new CollisionPoint(Vector3.one*-1,Vector3.one*-1);
                float nearestDist = 0.5f;
                if (other.pointPartition[posOnOther.x, posOnOther.y, posOnOther.z] == null) continue;
                foreach(CollisionPoint otherPt in other.pointPartition[posOnOther.x,posOnOther.y,posOnOther.z])
                {
                    if ((otherPt.point - myPointToOther.point).magnitude < nearestDist && PointToPlaneTest(myPointToOther.point, otherPt) <= 0)
                    {
                        CollisionPoint otherPtWorld = new CollisionPoint(other.transform, otherPt);
                        //nearestDist = (otherCollisionPointsInBounds[j].point - myPoint.point).magnitude;
                        //nearestCollidingPoint = otherCollisionPointsInBounds[j];


                        count++;
                        Debug.DrawLine(myPoint.point, otherPtWorld.point, Color.yellow);
                        if (count >= 256) return true;
                        // transform.position += myPoint.normal * PointToPlaneTest(otherCollisionPointsInBounds[j].point, myPoint);
                        transform.position -= otherPtWorld.normal * PointToPlaneTest(otherPtWorld.point, myPoint);
                        velocity -= Vector3.Dot(otherPtWorld.normal, velocity) < 0 ? Vector3.Dot(otherPtWorld.normal, velocity) * otherPtWorld.normal : Vector3.zero;
                        //Vector3 velAtPt = Vector3.Cross(angularVelocity, myPoint.point - transformedCOM);
                        //if(velAtPt.magnitude>0.001) Debug.Log(velAtPt);
                        //angularVelocity += Vector3.Dot(otherCollisionPointsInBounds[j].normal, velAtPt) < 0 ? Vector3.Cross(Vector3.Dot(otherCollisionPointsInBounds[j].normal, velAtPt) * otherCollisionPointsInBounds[j].normal,myPoint.point- transformedCOM) : Vector3.zero;


                        myPoint = new CollisionPoint(transform, collisionPoints[i]);

                        // transform.position += nearestCollidingPoint.point-myPoint.point;

                    }
                }

                /*
                for (int j = 0; j < otherCollisionPointsInBounds.Count; j++)
                {
                    if ((otherCollisionPointsInBounds[j].point - myPoint.point).magnitude < nearestDist && PointToPlaneTest(myPoint.point, otherCollisionPointsInBounds[j]) <=0)
                    {
                        //nearestDist = (otherCollisionPointsInBounds[j].point - myPoint.point).magnitude;
                        //nearestCollidingPoint = otherCollisionPointsInBounds[j];


                        count++;
                        Debug.DrawLine(myPoint.point, otherCollisionPointsInBounds[j].point, Color.yellow);
                        if (count >= 256) return true;
                        // transform.position += myPoint.normal * PointToPlaneTest(otherCollisionPointsInBounds[j].point, myPoint);
                        transform.position -= otherCollisionPointsInBounds[j].normal * PointToPlaneTest(otherCollisionPointsInBounds[j].point, myPoint);
                        velocity -= Vector3.Dot(otherCollisionPointsInBounds[j].normal, velocity) < 0 ? Vector3.Dot(otherCollisionPointsInBounds[j].normal, velocity) * otherCollisionPointsInBounds[j].normal : Vector3.zero;
                        //Vector3 velAtPt = Vector3.Cross(angularVelocity, myPoint.point - transformedCOM);
                        //if(velAtPt.magnitude>0.001) Debug.Log(velAtPt);
                        //angularVelocity += Vector3.Dot(otherCollisionPointsInBounds[j].normal, velAtPt) < 0 ? Vector3.Cross(Vector3.Dot(otherCollisionPointsInBounds[j].normal, velAtPt) * otherCollisionPointsInBounds[j].normal,myPoint.point- transformedCOM) : Vector3.zero;


                        myPoint = new CollisionPoint(transform, collisionPoints[i]);

                        // transform.position += nearestCollidingPoint.point-myPoint.point;
                        
                    }
                }*/
                // deal with the collision
                /*
                if (nearestCollidingPoint.point != Vector3.one * -1)
                {
                    count++;
                    Debug.DrawLine(myPoint.point, nearestCollidingPoint.point,Color.yellow);
                    if (count >= 256) return true;
                    // transform.position += myPoint.normal * PointToPlaneTest(nearestCollidingPoint.point, myPoint);
                    transform.position -= nearestCollidingPoint.normal * PointToPlaneTest(nearestCollidingPoint.point, myPoint);
                    myPlayer.velocity -= Vector3.Dot(otherCollisionPointsInBounds[i].normal, myPlayer.velocity) < 0 ? Vector3.Dot(otherCollisionPointsInBounds[i].normal, myPlayer.velocity)* otherCollisionPointsInBounds[i].normal : Vector3.zero;
                    // angularVelocity-=Vector3.Cross(angularVelocity,)
                    // transform.position += nearestCollidingPoint.point-myPoint.point;
                }*/
            }
            /*
            if (other.CheckPointBoundCollision(transform.TransformPoint(collisionPoints[i].point)))
            {
                count++;
                Debug.DrawLine(transform.TransformPoint(collisionPoints[i].point), transform.TransformPoint(collisionPoints[i].point+collisionPoints[i].normal), Color.yellow);
            }*/
        }
        return count>0;
    } 

    private void OnDestroy()
    {
        allChunkColliders.Remove(this);
    }

    public static Vector3 ClampVector(Vector3 point, Vector3 min, Vector3 max)
    {
        return new Vector3(Mathf.Clamp(point.x, min.x, max.x), Mathf.Clamp(point.y, min.y, max.y), Mathf.Clamp(point.z, min.z, max.z));
    }

    public static Vector3 AbsVector(Vector3 vec)
    {
        return new Vector3(Mathf.Abs(vec.x), Mathf.Abs(vec.y), Mathf.Abs(vec.z));
    }

    public static bool CheckPointBoundCollision(AABBox bound, Vector3 point)
    {
        return bound.min.x <= point.x && bound.max.x >= point.x &&
            bound.min.y <= point.y && bound.max.y >= point.y &&
            bound.min.z <= point.z && bound.max.z >= point.z;
    }


    public void ChangeVelocityTowards(Vector3 vel, float amount)
    {
        velocity += Vector3.ClampMagnitude(vel - velocity, amount);
    }
    public void ChangeAngularVelocityTowards(Vector3 vel, float amount)
    {
        angularVelocity += Vector3.ClampMagnitude(vel - angularVelocity, amount);
    }

    public Vector3 VoxelRaycastTo(Vector3 origin, Vector3 direction, int maxIter = 10)
    {
        Vector3 testPos = transform.InverseTransformPoint(origin);
        Vector3 localDir = transform.InverseTransformDirection(direction);
        for(int i = 0; i < maxIter; i++)
        {
            if (attachedChunk.CheckVoxel(testPos)) return testPos;
            testPos += localDir * 0.5f;
        }
        return -Vector3.one;
    }

}
