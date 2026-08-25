using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ObjectController : MonoBehaviour
{
    private MeshRenderer mr;
    private bool isInRange = false;
    private Color oriColor;
    private Color inRangeColor;
    // Start is called before the first frame update
    void Start()
    {
        mr = GetComponent<MeshRenderer>();
        oriColor = mr.material.color;
        inRangeColor = new Color32(255, 133, 28, 255);
    }

    // Update is called once per frame
    void Update()
    {
        if(isInRange)
        {
            mr.material.color = inRangeColor;
        }
        else
        {
            mr.material.color = oriColor;
        }
    }

    public void ObjectInRange(bool inRange)
    {
        isInRange = inRange;
    }
    
}
