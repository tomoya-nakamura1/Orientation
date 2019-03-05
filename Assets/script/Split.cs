using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Split : MonoBehaviour {

	// Use this for initialization
	void Start () {
        string line = "jdlj hfdsh hdshfl dshfl kdj";
        string[] moji = line.Split(' ');
        Debug.Log(moji[0]);
        Debug.Log(moji[1]);
        Debug.Log(moji[2]);
        Debug.Log(moji[3]);
        Debug.Log(moji[4]);
        Debug.Log(moji[5]);
        Debug.Log(moji[6]);

    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
