using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrajectoryHelper : MonoBehaviour
{


    // Update is called once per frame
    void Update()
    {
        float width = this.GetComponent<LineRenderer>().startWidth;
        this.GetComponent<LineRenderer>().material.mainTextureScale = new Vector2(1f / width, 1.0f);
    }
}
