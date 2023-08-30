using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    public int chunkEdgeLen;

    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
    public MeshCollider meshCollider;

    public MeshCollider convexCollider;

    // public Rigidbody rb;

    private List<Vector3> vertices;
    private List<int> triangles;
    private List<Vector2> uvs;

    bool[,,] voxelMap;

    public static Chunk singleton;


    private List<Chunk> collidedChunks;
    private BoxCollider contactCollider;

    public BlockUpdateDelegate OnBlockUpdate;
    public VoidDelegate OnCreateMesh;


    // Start is called before the first frame update
    void Start()
    {
        if (singleton == null) singleton = this;
        // collidedChunks = new List<Chunk>();
        // contactCollider = gameObject.AddComponent<BoxCollider>();
        // contactCollider.enabled = false;
        // rb = GetComponent<Rigidbody>();

        PopulateVoxelMap();
        CreateMeshData();
        
        
        CreateMesh();
    }

    // Update is called once per frame
    void Update()
    {
        


    }

    private void FixedUpdate()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("collided at "+collision.GetContact(0).point);
    }

    private void PopulateVoxelMap()
    {
        if(voxelMap==null) voxelMap = new bool[chunkEdgeLen, chunkEdgeLen, chunkEdgeLen];
        if (gameObject.name == "ship")
        {
            voxelMap[8, 8, 8] = true;
            voxelMap[8, 8, 7] = true;
            voxelMap[9, 8, 7] = true;
            voxelMap[7, 8, 7] = true;
            voxelMap[8, 8, 6] = true;
            voxelMap[9, 8, 6] = true;
            voxelMap[7, 8, 6] = true;
            return;
        }
        for (int x = 0; x < chunkEdgeLen; x++)
        {
            for (int y = 0; y < chunkEdgeLen; y++)
            {
                for (int z = 0; z < chunkEdgeLen; z++)
                {
                    if (Random.value > 0.5) // Random.value > 0.5 && z<7
                    {
                        voxelMap[x, y, z] = true;
                        // AddBlock(new Vector3Int(x, y, z));
                    }
                }
            }
        }
    }

    private void CreateMeshData()
    {
        int vertexIdx = 0;
        vertices = new List<Vector3>();
        triangles = new List<int>();
        uvs = new List<Vector2>();
        // collisionPoints = new List<CollisionPoint>();
        // worldCollisionPoints = new List<CollisionPoint>();
        //rb.mass = 0;
        for (int x = 0; x < chunkEdgeLen; x++)
        {
            for (int y = 0; y < chunkEdgeLen; y++)
            {
                for (int z = 0; z < chunkEdgeLen; z++)
                {
                    if (voxelMap[x,y,z])
                    {
                        AddVoxelDataToChunk(new Vector3(x, y, z), ref vertexIdx);
                        //rb.mass += 1;
                        // AddCollisionData(new Vector3Int(x, y, z));
                    }
                }
            }
        }
    }

    private void AddVoxelDataToChunk(Vector3 pos, ref int vertexIdx)
    {
        for (int i = 0; i < 6; i++)
        {
            if (!CheckVoxel(pos + VoxelData.faceChecks[i]))
            {
                vertices.Add(pos + VoxelData.voxelVerticies[VoxelData.voxelTriangles[i, 0]]);
                vertices.Add(pos + VoxelData.voxelVerticies[VoxelData.voxelTriangles[i, 1]]);
                vertices.Add(pos + VoxelData.voxelVerticies[VoxelData.voxelTriangles[i, 2]]);
                vertices.Add(pos + VoxelData.voxelVerticies[VoxelData.voxelTriangles[i, 3]]);
                uvs.Add(VoxelData.voxelUVs[0]);
                uvs.Add(VoxelData.voxelUVs[1]);
                uvs.Add(VoxelData.voxelUVs[2]);
                uvs.Add(VoxelData.voxelUVs[3]);
                triangles.Add(vertexIdx);
                triangles.Add(vertexIdx + 1);
                triangles.Add(vertexIdx + 2);
                triangles.Add(vertexIdx + 2);
                triangles.Add(vertexIdx + 1);
                triangles.Add(vertexIdx + 3);
                vertexIdx += 4;
                // collisionPoints.Add(transform.TransformPoint(pos+(Vector3.one+VoxelData.faceChecks[i])*0.5f));
                CollisionPoint c = new CollisionPoint(pos + (Vector3.one + VoxelData.faceChecks[i]) * 0.5f, VoxelData.faceChecks[i]);
                // collisionPoints.Add(c);
                // worldCollisionPoints.Add(new CollisionPoint(transform,c));
            }
            /*
        for (int j = 0; j < 6; j++)
        {
            int triangleIdx = VoxelData.voxelTriangles[i, j];
            vertices.Add(VoxelData.voxelVerticies[triangleIdx] + pos);
            triangles.Add(vertexIdx);
            uvs.Add(VoxelData.voxelUVs[i]);
            vertexIdx++;
        }*/
        }
    }

    private void CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.SetUVs(0, uvs); 
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
        if(convexCollider!=null) convexCollider.sharedMesh = mesh;

        OnCreateMesh?.Invoke();
        // Debug.Log(voxelMap[14, 14, 14]);
    }
    public bool CheckVoxel(Vector3 pos)
    {
        Vector3Int floorPos = Vector3Int.FloorToInt(pos);
        if (floorPos.x < 0 || floorPos.x > chunkEdgeLen - 1 || floorPos.y < 0 || floorPos.y > chunkEdgeLen - 1 || floorPos.z < 0 || floorPos.z > chunkEdgeLen - 1) return false;
        return voxelMap[floorPos.x, floorPos.y, floorPos.z];
    }

    public void BreakBlock(Vector3Int cords)
    {
        Debug.Log("breaking " + cords);
        voxelMap[cords.x, cords.y, cords.z] = false;
        OnBlockUpdate?.Invoke(cords, voxelMap[cords.x, cords.y, cords.z]);
        UpdateMesh();
    }
    public bool PlaceBlock(Vector3Int cords)
    {
        Debug.Log("placing " + cords);
        if (cords.x < 0 || cords.x > chunkEdgeLen - 1 || cords.y < 0 || cords.y > chunkEdgeLen - 1 || cords.z < 0 || cords.z > chunkEdgeLen - 1) return false;
        voxelMap[cords.x, cords.y, cords.z] = true;
        OnBlockUpdate?.Invoke(cords, voxelMap[cords.x, cords.y, cords.z]);
        UpdateMesh();
        return true;
    }
    private void AddBlock(Vector3Int cords)
    {
        voxelMap[cords.x, cords.y, cords.z] = true;
        
        
        // Vector3Int prevTriggerLoc = Vector3Int.zero;
        int iterations = (int)Mathf.Log(chunkEdgeLen, 2);
        Transform currTransform = transform.GetChild(0);
        currTransform.GetComponent<BoxTriggerObj>().numContainedBlocks++;
        int divBy = chunkEdgeLen/2;
        int blockIdx = ToIdx(cords);
        for (int i = 0; i < iterations - 1; i++)
        {
            // Vector3Int myPos = cords / (chunkEdgeLen / divBy) - prevTriggerLoc * (divBy / 2);
            // Debug.Log(myPos + " " +ToIdx(myPos));
            
            currTransform = currTransform.GetChild(blockIdx/(divBy*divBy*divBy)%8);
            // prevTriggerLoc = myPos;
            divBy /= 2;
            currTransform.GetComponent<BoxTriggerObj>().numContainedBlocks++;
        }
    }
    private void AddCollisionData(Vector3Int cords)
    {

        // Vector3Int prevTriggerLoc = Vector3Int.zero;
        
        int iterations = (int)Mathf.Log(chunkEdgeLen, 2);
        Transform currTransform = transform.GetChild(0);
        currTransform.GetComponent<BoxTriggerObj>().numContainedBlocks++;
        int divBy = chunkEdgeLen / 2;
        int blockIdx = ToIdx(cords);
        for (int i = 0; i < iterations - 1; i++)
        {
            // Vector3Int myPos = cords / (chunkEdgeLen / divBy) - prevTriggerLoc * (divBy / 2);
            // Debug.Log(myPos + " " +ToIdx(myPos));

            currTransform = currTransform.GetChild(blockIdx / (divBy * divBy * divBy) % 8);
            // prevTriggerLoc = myPos;
            divBy /= 2;
            currTransform.GetComponent<BoxTriggerObj>().numContainedBlocks++;
        }
    }
    public void UpdateMesh()
    {
        CreateMeshData();
        CreateMesh();
    }

    public Vector3Int WorldPosToBlock(Vector3 pos)
    {
        return Vector3Int.FloorToInt(transform.InverseTransformPoint(pos));
    }

    private void OnTriggerEnter(Collider collision)
    {
        //Chunk collidedChunk = collision.GetComponent<Chunk>();
        //if (collision != null) collidedChunks.Add(collidedChunk);
    }
    private void OnTriggerExit(Collider collision)
    {
        //Chunk collidedChunk = collision.GetComponent<Chunk>();
        //if (collision != null) collidedChunks.Remove(collidedChunk);
    }

    public BoxTriggerObj GetSpecificTrigger(Vector3Int blockPos)
    {
        // x: 0, 2, 4, 6
        // y: 0, 1, 2, 3
        // z: 0, 1, 4, 5
        // 0, 0, 0 => 0 // 1, 0, 0 => 1 // 0, 0, 1 ==> 2 // 1, 0, 1 ==> 3 // 0, 1, 0 => 4 // 1, 1, 0 => 5 // 0, 1, 1 => 6 // 1, 1, 1 => 7

        // 0, 0, 0 // 0, 0, 1 // 0, 1, 0 // 0, 1, 1 // 1, 0, 0 // 1, 0, 1 // 1, 1, 0 // 1, 1, 1
        // idk what i am doing

        Vector3Int prevTriggerLoc = Vector3Int.zero;
        int iterations = (int)Mathf.Log(chunkEdgeLen, 2);
        Transform currTransform = transform.GetChild(0);
        int divBy = 2;
        for(int i = 0; i < iterations-1; i++)
        {
            Vector3Int myPos = blockPos / (chunkEdgeLen / divBy) - prevTriggerLoc*(divBy/2);
            currTransform = currTransform.GetChild(ToIdx(myPos));
            prevTriggerLoc = myPos;
            divBy *= 2;
        }
        return currTransform.GetComponent<BoxTriggerObj>();
        // transform.GetChild();

    }

    private int ToIdx(Vector3Int v)
    {
        return v.z + v.y * chunkEdgeLen + v.x * chunkEdgeLen * chunkEdgeLen;
    }
   
}

public struct CollisionPoint{
    public Vector3 point;
    public Vector3 normal;
    public CollisionPoint(Vector3 point, Vector3 normal)
    {
        this.point = point;
        this.normal = normal;
    }
    public CollisionPoint(Transform localTransform, CollisionPoint convertPoint)
    {
        this.point = localTransform.TransformPoint(convertPoint.point);
        this.normal = localTransform.TransformDirection(convertPoint.normal);
    }
    public CollisionPoint TransformPoint(Transform from, Transform to)
    {
        return new CollisionPoint(to.InverseTransformPoint(from.TransformPoint(point)), to.InverseTransformDirection(from.TransformDirection(normal)));
    }
    public CollisionPoint ToTransform(Transform to)
    {
        return new CollisionPoint(to.InverseTransformPoint(point), to.InverseTransformDirection(normal));
    }

}

public struct AABBox
{
    public Vector3 min;
    public Vector3 max;
    public Vector3 center { get { return (max + min) * 0.5f; } }
    public Vector3 extents { get { return (max - min) * 0.5f; } }
    public AABBox(Vector3 corner1, Vector3 corner2)
    {
        this.min = Vector3.Min(corner1, corner2);
        this.max = Vector3.Max(corner1,corner2);
    }
    public AABBox(Vector3 corner1, Vector3 corner2, Vector3 corner3, Vector3 corner4)
    {
        this.min = Vector3.Min(Vector3.Min(Vector3.Min(corner1, corner2),corner3),corner4);
        this.max = Vector3.Max(Vector3.Max(Vector3.Max(corner1, corner2),corner3),corner4);
    }
    public static AABBox operator *(AABBox a, float b)
    {
        Vector3 diff = (a.max - a.min)*0.5f;
        Vector3 center = a.min + diff;
        return new AABBox(center-diff*b, center+diff*b); //change later
    }
    public static AABBox operator +(AABBox a, Vector3 b)
    {
        return new AABBox(a.min - b, a.max + b);
    }

    public bool CheckCollision(AABBox other)
    {
        return min.x <= other.max.x && max.x >= other.min.x &&
            min.y <= other.max.y && max.y >= other.min.y &&
            min.z <= other.max.z && max.z >= other.min.z;
    }
    public bool CheckCollision(Vector3 point)
    {
        return min.x <= point.x && max.x >= point.x &&
            min.y <= point.y && max.y >= point.y &&
            min.z <= point.z && max.z >= point.z;
    }
    public bool CheckCollision(CollisionPoint p)
    {
        // Convert AABB to center-extents representation
        Vector3 center = (max + min) * 0.5f; // Compute AABB center
        Vector3 extents = max - center; // Compute positive extents

        // Compute the projection interval radius of b onto L(t) = b.c + t * p.n

        // float r = extends.x * Abs(p.normal.x) + extents.y * Abs(p.normal.y) + extents.z * Abs(p.normal.z);
        float projRadius = Vector3.Dot(extents, ChunkCollider.AbsVector(p.normal));
        // Compute distance of box center from plane
        float dist = Vector3.Dot(p.normal, center) - Vector3.Dot(p.point, p.normal);
        // Intersection occurs when distance s falls within [-r,+r] interval
        return Mathf.Abs(dist) <= projRadius;
    }
    public bool CheckCollision(CollisionPoint p, float maxDist)
    {
        // Convert AABB to center-extents representation
        Vector3 center = (max + min) * 0.5f; // Compute AABB center
        Vector3 extents = max - center; // Compute positive extents

        // Compute the projection interval radius of b onto L(t) = b.c + t * p.n

        // float r = extends.x * Abs(p.normal.x) + extents.y * Abs(p.normal.y) + extents.z * Abs(p.normal.z);
        float projRadius = Mathf.Clamp(Vector3.Dot(extents, ChunkCollider.AbsVector(p.normal)),-maxDist, maxDist);
        // Compute distance of box center from plane
        float dist = Vector3.Dot(p.normal, center) - Vector3.Dot(p.point, p.normal);
        // Intersection occurs when distance s falls within [-r,+r] interval
        return Mathf.Abs(dist) <= projRadius;
    }

    public Vector3 ClosestPointOnSurface(CollisionPoint p)
    {
        Vector3 point = p.point - p.normal;
        return ChunkCollider.ClampVector(point, min, max);
    }

    public AABBox ToTransform(Transform to)
    {
        // return new AABBox(to.InverseTransformPoint(min), to.InverseTransformPoint(max));
        Vector3 transformedCenter = to.InverseTransformPoint(center);
        return new AABBox(transformedCenter-extents,transformedCenter+extents);
    }
}

public delegate void VoidDelegate();
public delegate void BlockUpdateDelegate(Vector3Int pos, bool state);