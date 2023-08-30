using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Camera playerCam;
    // Stores an instance of a "Rigidbody" class. This class handles physics and collisions all on it's own, just apply forces are you're set!
    private Rigidbody rb;
    private ChunkCollider ridingChunk;

    private Vector3 localPosToRide;

    public Vector3 velocity;
    public bool hasGravity;

    private Vector3 targetVelocity;

    private Vector3 localVelocity { get { return transform.InverseTransformVector(velocity); } set { velocity = transform.TransformVector(value); } }


    PlayerCollider col;

    public bool canFly = false;
    private float jumpTimer = 0f;

    private bool craftControl = false;

    public GameObject chunkColliderObj;

    // Start is called before the first frame update
    void Start()
    {
        // Gets the "rigidbody" attached to the object this script is on. 
        // rb = GetComponent<Rigidbody>();
        col = GetComponent<PlayerCollider>();
        // Hides the cursor upon startup.
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {

        
        if (Input.GetKeyDown(KeyCode.Escape)) ToggleCursor();
        if (Input.GetKeyDown(KeyCode.C)) craftControl = !craftControl;
        if (Input.GetKeyDown(KeyCode.R)) ridingChunk = null;
        //if(ridingChunk!=null) Debug.Log(Vector3.Cross(transform.position-ridingChunk.transform.TransformPoint(ridingChunk.CoM),ridingChunk.angularVelocity));

        // This rotates our character around the y axis depending on horizontal mouse movement. Since the camera is "attached" as a child to this object, it rotates as well. 
        transform.Rotate(transform.up, Input.GetAxis("Mouse X"));

        // This rotates the camera around the "right axis" of whereever we are looking, aka up or down. I'm only rotating the camera because I don't want the entire character to rotate (no upsidedown legs)
        playerCam.transform.Rotate(transform.right, -Input.GetAxis("Mouse Y"), Space.World);
        jumpTimer += Time.deltaTime;

        if (ridingChunk != null && craftControl)
        {
            ridingChunk.ChangeVelocityTowards(ridingChunk.transform.TransformDirection(new Vector3(0, Input.GetAxis("Fly") * 10f, Input.GetAxis("Vertical") * 50f)),20*Time.deltaTime);
            ridingChunk.ChangeAngularVelocityTowards(Vector3.up*Input.GetAxis("Horizontal")*20f,40*Time.deltaTime);
        }
        else
        {
            if (Input.GetButtonDown("Jump"))
            {
                if (jumpTimer < 0.3f && canFly) hasGravity = !hasGravity;
                jumpTimer = 0;
                if (hasGravity && col.touchingGround) localVelocity += Vector3.up * 5;
                if (!canFly) hasGravity = true;
            }
            if (Input.GetButton("Jump"))
            {
                if (hasGravity && col.touchingGround && jumpTimer > 0.1f) { localVelocity += Vector3.up * 5; jumpTimer = 0; }
            }
            localVelocity = hasGravity ? new Vector3(Input.GetAxis("Horizontal") * 5f, localVelocity.y, Input.GetAxis("Vertical") * 5f) : new Vector3(Input.GetAxis("Horizontal") * 5f, Input.GetAxis("Fly") * 5f, Input.GetAxis("Vertical") * 5f);

        }

        if (Input.GetButtonDown("Fire1"))
        {
            foreach(ChunkCollider c in ChunkCollider.allChunkColliders)
            {
                Vector3 hit = c.VoxelRaycastTo(Camera.main.transform.position, Camera.main.transform.forward);
                if (hit.x != -1)
                {
                    c.attachedChunk.BreakBlock(Vector3Int.FloorToInt(hit));
                }
            }
        }
        if (Input.GetButtonDown("Fire2"))
        {
            foreach (ChunkCollider c in ChunkCollider.allChunkColliders)
            {
                Vector3 hit = c.VoxelRaycastTo(Camera.main.transform.position, Camera.main.transform.forward);
                if (hit.x != -1)
                {
                    c.attachedChunk.PlaceBlock(Vector3Int.FloorToInt(hit-Camera.main.transform.forward*0.5f));
                }
            }
        }

        /*
        if (Input.GetKeyDown(KeyCode.R))
        {
            RaycastHit hit;
            Physics.Raycast(playerCam.transform.position, playerCam.transform.forward, out hit);
            if (hit.transform == null) ridingRb = null;
            else
            {
                Debug.Log(hit.point);
                Chunk o = hit.transform.GetComponent<Chunk>();
                if (o != null)
                    ridingRb = o.rb;
                else
                    ridingRb = null;
                if (ridingRb != null) localPosToRide = ridingRb.transform.InverseTransformPoint(rb.position);
            }

        }
        if (Input.GetButtonDown("Fire1"))
        {
            RaycastHit hit;
            Physics.Raycast(playerCam.transform.position, playerCam.transform.forward, out hit);
            if (hit.transform != null)
            {
                Debug.Log(hit.point);
                Chunk hitChunk = hit.transform.GetComponent<Chunk>();
                if (hitChunk != null)
                {
                    Vector3Int hitBlock = hitChunk.WorldPosToBlock(hit.point + playerCam.transform.forward * 0.001f);
                    hitChunk.BreakBlock(hitBlock);
                }
            }
        }
        // if (ridingRb != null) transform.position = ridingRb.transform.TransformPoint(localPosToRide);
        // Adds a force of -1 / 1 along the x axis depending on a or d pressed, 1 along y axis when space pressed, and -1 / 1 along z axis when s or w pressed
        if (ridingRb != null)
        {
            localVelocity = transform.InverseTransformDirection(rb.velocity) + (ridingRb.transform.InverseTransformDirection(ridingRb.velocity) + Vector3.Cross(ridingRb.transform.InverseTransformPoint(rb.position), ridingRb.angularVelocity));
            targetVelocity = new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Jump"), Input.GetAxisRaw("Vertical")) * 5;
            Vector3 velDiff = targetVelocity - localVelocity;
            rb.AddRelativeForce(velDiff * 5 * Time.deltaTime, ForceMode.VelocityChange);
            // localPosToRide += ridingRb.transform.InverseTransformDirection(transform.TransformDirection(new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Jump"), Input.GetAxisRaw("Vertical")) * 2 * Time.deltaTime));
        }
        else 
        {
            localVelocity = transform.InverseTransformDirection(rb.velocity);
            // ridingRb.velocity + Vector3.Cross(ridingRb.angularVelocity,ridingRb.transform.InverseTransformPoint(rb.position))
            targetVelocity = new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Jump"), Input.GetAxisRaw("Vertical")) * 5;
            Vector3 velDiff = targetVelocity - localVelocity;
            rb.AddRelativeForce(velDiff * 5 * Time.deltaTime, ForceMode.VelocityChange);
        }


        // This rotates our character around the y axis depending on horizontal mouse movement. Since the camera is "attached" as a child to this object, it rotates as well. 
        transform.Rotate(transform.up, Input.GetAxis("Mouse X"));

        // This rotates the camera around the "right axis" of whereever we are looking, aka up or down. I'm only rotating the camera because I don't want the entire character to rotate (no upsidedown legs)
        playerCam.transform.Rotate(transform.right, -Input.GetAxis("Mouse Y"), Space.World);

        */
        /*if (ridingRb != null) {
            rb.Move(ridingRb.transform.TransformPoint(localPosToRide),transform.rotation);
            localPosToRide = ridingRb.transform.InverseTransformPoint(rb.position);
        }*/
        /*
        if (hasGravity) velocity += Vector3.down * 9.8f * Time.deltaTime;

        transform.position += velocity * Time.deltaTime;*/


        //if (ridingRb != null) localPosToRide = ridingRb.transform.InverseTransformPoint(rb.position);
        //else localPosToRide = rb.position;
        if (ridingChunk != null) transform.position = ridingChunk.transform.TransformPoint(localPosToRide);

        if (hasGravity) velocity += Vector3.down * 9.8f * Time.deltaTime;
        col.CalcCollisions();
        velocity = Vector3.ClampMagnitude(velocity, 10f);
        // GetComponent<PlayerCollider>().CalcCollisions();
        if (col.collided && ridingChunk != col.collidingChunks[0]) ridingChunk = col.collidingChunks[0];

        transform.position += velocity * Time.deltaTime;
        
        if (ridingChunk != null) localPosToRide = ridingChunk.transform.InverseTransformPoint(transform.position);
    }

    private void FixedUpdate()
    {
        /*
        if (hasGravity) velocity += Vector3.down * 9.8f * Time.fixedDeltaTime;
        col.CalcCollisions();
        // GetComponent<PlayerCollider>().CalcCollisions();
        transform.position += velocity * Time.fixedDeltaTime;*/
        /*
        if (hasGravity) velocity += Vector3.down * 9.8f * Time.fixedDeltaTime;

        transform.position += velocity*Time.fixedDeltaTime;
        //if (ridingRb != null) localPosToRide = ridingRb.transform.InverseTransformPoint(rb.position);
        //else localPosToRide = rb.position;
        GetComponent<PlayerCollider>().CalcCollisions();*/

        // if (ridingRb != null) transform.position = ridingRb.transform.TransformPoint(localPosToRide);
        //GetComponent<PlayerCollider>().CalcCollisions();
    }

    // Toggles cursor on and off.
    private void ToggleCursor()
    {
        Cursor.lockState = Cursor.lockState == CursorLockMode.None ? CursorLockMode.Locked : CursorLockMode.None;
    }
}
