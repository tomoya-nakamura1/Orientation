using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnCollisonStay : MonoBehaviour {

    bool col = false;

    private void OnCollisionStay(Collision collision)
    {
        col = true;
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        GameObject cube = GameObject.Find("VoxelBase(Clone)");
        if (col)
        {
            GameObject.Destroy(cube,4.0f);
            //VoxelBase(Clone)
        }
    }
}
