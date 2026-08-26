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

    // temp
    private bool isButton;
    private float cooldownTime = 1f;
    private float cooldownTimer = 0f;
    // Start is called before the first frame update
    void Start()
    {
        mr = GetComponent<MeshRenderer>();
        oriColor = mr.material.color;
        inRangeColor = new Color32(255, 133, 28, 255);

        if(gameObject.CompareTag("Interactable"))
        {
            isButton = true;
            cooldownTimer = cooldownTime;
        }
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

        // very temp logic
        if (isButton)
        {
            if(transform.position.y < .5f)
            {
                cooldownTimer -= Time.deltaTime;

                if(cooldownTimer < 0f)
                {
                    transform.position = new Vector3(transform.position.x, .5f, transform.position.z);
                    cooldownTimer = cooldownTime;
                }
            }
            
        }
    }

    public void ObjectInRange(bool inRange)
    {
        isInRange = inRange;
    }

}
