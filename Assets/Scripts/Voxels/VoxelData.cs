using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VoxelData { 
    public static readonly Vector3[] voxelVerticies = new Vector3[8] { 
        Vector3.zero, 
        Vector3.right,
        new Vector3(1,1,0),
        Vector3.up,
        Vector3.forward,
        new Vector3(1,0,1),
        Vector3.one,
        new Vector3(0,1,1)
    };
    public static readonly int[,] voxelTriangles = new int[6, 4] {
        /*
        {0, 3, 1, 1, 3, 2}, // back
		{5, 6, 4, 4, 6, 7}, // front
		{3, 7, 2, 2, 7, 6}, // top
		{1, 5, 0, 0, 5, 4}, // bottom
		{4, 7, 0, 0, 7, 3}, // left
		{1, 2, 5, 5, 2, 6} // right
        */
        {0, 3, 1, 2}, // back
		{5, 6, 4, 7}, // front
		{3, 7, 2, 6}, // top
		{1, 5, 0, 4}, // bottom
		{4, 7, 0, 3}, // left
		{1, 2, 5, 6} // right
	};
    public static readonly Vector2[] voxelUVs = new Vector2[4] {
        Vector2.zero,
        Vector2.up,
        Vector2.right,
        // Vector2.right,
        // Vector2.up,
        Vector2.one
    };
    public static readonly Vector3[] faceChecks = new Vector3[6] { //prevent drawing hidden faces
        Vector3.back,
        Vector3.forward,
        Vector3.up,
        Vector3.down,
        Vector3.left,
        Vector3.right
    };
}
