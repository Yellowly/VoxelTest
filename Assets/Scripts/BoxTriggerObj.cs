using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxTriggerObj : MonoBehaviour
{
    private int numCollisions = 0;
    private bool lastLayer = false;
    private Chunk attachedChunk;
    private BoxTriggerObj attachedBoxTrigger;

    private BoxTriggerObj[] childTriggers;

    private Vector3 localPosToChunk;

    //only for last level triggers
    private Transform[] containedColliders;

    public int numContainedBlocks = 0;
    // Start is called before the first frame update
    void Awake()
    {
        attachedChunk = GetComponentInParent<Chunk>();
        localPosToChunk = attachedChunk.transform.InverseTransformPoint(transform.position);
        if (gameObject.layer == 9)
        {
            lastLayer = true;
            containedColliders = new Transform[8];
            int idx = 0;
            foreach (Transform t in transform)
            {
                if (t != transform) { containedColliders[idx] = t; idx++; }
            }
        }
        else
        {
            childTriggers = new BoxTriggerObj[8];
            int idx = 0;
            if (gameObject.layer == 8) Debug.Log(transform.childCount);
            foreach (Transform t in transform)
            {
                if (t != transform) t.gameObject.layer = gameObject.layer + 1;
                childTriggers[idx] = t.GetComponent<BoxTriggerObj>();
                idx++;
            }
        }
        
        /*
        attachedBoxTrigger = GetComponentInParent<BoxTriggerObj>();
        if (attachedBoxTrigger == null) gameObject.layer = 6;
        else gameObject.layer = attachedBoxTrigger.gameObject.layer + 1;*/
    }

    // Update is called once per frame
    void Update()
    {
    }
    private void OnTriggerEnter(Collider other)
    {
        // if (other.attachedRigidbody == attachedChunk.rb) return;
        numCollisions++;
        Debug.Log(numCollisions + " " + other+ " layer: "+gameObject.layer);
        if (numCollisions == 1 && numContainedBlocks>0)
        {
            if (lastLayer)
            {
                foreach (Transform containedCollider in containedColliders)
                {
                    if (attachedChunk.CheckVoxel(localPosToChunk + containedCollider.localPosition))
                    {
                        containedCollider.gameObject.SetActive(true);
                    }
                }
            }
            else
            {
                foreach (Transform t in transform)
                {
                    if (t != transform) t.gameObject.SetActive(true);
                }
            }
        }
    }
    private void OnTriggerStay(Collider other)
    {
        // Debug.Log("touching " + other);
    }

    private void OnTriggerExit(Collider other)
    {
        // if (other.attachedRigidbody == attachedChunk.rb) return;
        numCollisions--;
        if (numCollisions <= 0)
        {
            numCollisions = 0;
            if (lastLayer)
            {
                foreach (Transform t in transform)
                {
                    if (t != transform) t.gameObject.SetActive(false);
                }
            }
            else
            {
                foreach(BoxTriggerObj b in childTriggers)
                {
                    b.Deactivate();
                }
            }
        }
    }

    public void Deactivate()
    {
        if (lastLayer)
        {
            foreach (Transform t in transform)
            {
                if (t != transform) t.gameObject.SetActive(false);
            }
        }
        else
        {
            Debug.Log(gameObject.layer);
            foreach (BoxTriggerObj b in childTriggers)
            {
                b.Deactivate();
            }
            gameObject.SetActive(false);
        }
    }

}
