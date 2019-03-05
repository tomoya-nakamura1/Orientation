using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResizeBoxCollier : MonoBehaviour {

	// Use this for initialization
	void Start () {
        

    }

    // Update is called once per frame
    void Update () {
        //コライダーのサイズをオブジェクトに合わせる
        Vector3 objectSize = gameObject.GetComponent<RectTransform>().sizeDelta;
        BoxCollider collider = GetComponent<BoxCollider>();
        collider.size = objectSize;
    }
}
