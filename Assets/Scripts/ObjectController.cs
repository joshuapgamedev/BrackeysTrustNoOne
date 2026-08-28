using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Unity.VisualScripting;
using UnityEngine;

public enum PuzzleRoomType
{
    Lever, 
    PressurePlate,
    SquareHole,
    Button,
    Final,
    None
}

public class ObjectController : MonoBehaviour
{
    [SerializeField] public PuzzleRoomType roomType;
    [SerializeField] public string objectId;

    private MeshRenderer mr;
    private bool isInRange = false;
    private Color oriColor;
    private Color inRangeColor;

    private bool isActivated = false;

    // pressure plate
    private bool isPressurePlateActivated = false;
    private Transform plateButton = null;
    private Vector3 plateButtonOriPos;
    private Vector3 plateButtonActivatedPos;
    private Color plateButtonActivatedColor;
    public static event System.Action OnActivatePressurePlate;


    // temp
    private bool isButton;
    private float cooldownTime = 1f;
    private float cooldownTimer = 0f;
    // Start is called before the first frame update
    void Start()
    {
        if (gameObject.CompareTag("PressurePlate"))
        {
            plateButton = transform.GetChild(1);
            plateButtonOriPos = plateButton.localPosition;
            plateButtonActivatedPos = new Vector3(plateButtonOriPos.x, -10f, plateButtonOriPos.z);
        }

        if (gameObject.CompareTag("Interactable"))
        {
            isButton = true;
            cooldownTimer = cooldownTime;
        }


        
        if (gameObject.CompareTag("PressurePlate"))
        {
            mr = plateButton.GetComponent<MeshRenderer>();
            plateButtonActivatedColor = Color.green;
        }
        else
        {
            mr = GetComponent<MeshRenderer>();
        }

        oriColor = mr.material.color;
        inRangeColor = new Color32(255, 133, 28, 255);



    }

    // Update is called once per frame
    void Update()
    {
        if(isInRange)
        {
            if(!gameObject.CompareTag("PressurePlate"))
                mr.material.color = inRangeColor;
        }
        else
        {
            if (!gameObject.CompareTag("PressurePlate"))
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

    public bool IsActivated()
    {
        return isActivated;
    }

    public void Activated()
    {
        isActivated = !isActivated;
    }

    private void OnTriggerStay(Collider other)
    {
        if(!isActivated && CompareTag("PressurePlate"))
        {
            if (other.CompareTag("ObjectCollider"))
            {
                PressurePlateActivated(true);
            }
        }
        
    }

    private void OnTriggerExit(Collider other)
    {
        if (CompareTag("PressurePlate"))
        {
            if (other.CompareTag("ObjectCollider"))
            {
                PressurePlateActivated(false);
            }
        }
    }

    public void TriggerPressurePlate()
    {
        Debug.Log("TriggerPressurePlate call");
        isActivated = !isActivated;

        plateButton.localPosition = isActivated ? plateButtonActivatedPos : plateButtonOriPos;
        mr.material.color = isActivated ? plateButtonActivatedColor : oriColor;

        OnActivatePressurePlate?.Invoke();
    }

    private void PressurePlateActivated(bool activated)
    {
        isActivated = activated;

        plateButton.localPosition = isActivated ? plateButtonActivatedPos : plateButtonOriPos;
        mr.material.color = isActivated ? plateButtonActivatedColor : oriColor;

        OnActivatePressurePlate?.Invoke();
    }

}
