using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Unity.VisualScripting;
using UnityEngine;

public enum PlayerAction
{
    ToggleLever,
    Pickup,
    Drop,
    PressurePlate,
    PressButton
}

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

    [SerializeField] private Transform holdPoint;
    public Transform HoldPoint => holdPoint;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip sfxPickup;
    [SerializeField] private AudioClip sfxDrop;
    [SerializeField] private AudioClip sfxLever;
    [SerializeField] private AudioClip sfxPressurePlate;
    [SerializeField] private AudioClip sfxButton;

    [SerializeField] private GameObject leverUp;
    [SerializeField] private GameObject leverDown;

    private MeshRenderer mr;
    private bool isInRange = false;
    private Color oriColor;
    //private Color inRangeColor;

    private bool isActivated = false;

    private Animator animator;

    // pressure plate
    private Transform plateButton = null;
    private Vector3 plateButtonOriPos;
    private Vector3 plateButtonActivatedPos;
    private Color plateButtonActivatedColor;
    public static event System.Action OnActivatePressurePlate;

    // temp
    private bool isButton;
    private float cooldownTime = 25f;
    private float cooldownTimer = 0f;
    public static event System.Action<ObjectController> OnPressButton;
    public static event System.Action<ObjectController> OnButtonExpired;
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
            animator = transform.parent.GetComponent<Animator>();
        }

        if(gameObject.CompareTag("Toggleable"))
        {
            //leverUpPos = transform.localPosition;
            //leverDownPos = new Vector3(-110.497f, transform.localPosition.y, transform.localPosition.z);
            animator = transform.parent.GetComponent<Animator>();
        }

        if (gameObject.CompareTag("PressurePlate"))
        {
            mr = plateButton.GetComponent<MeshRenderer>();
            oriColor = mr.material.color;
            plateButtonActivatedColor = Color.green;
        }
        else
        {
            //mr = GetComponent<MeshRenderer>();
        }

        //oriColor = mr.material.color;
        //inRangeColor = new Color32(255, 133, 28, 255);

        audioSource = GetComponent<AudioSource>();

    }

    // Update is called once per frame
    void Update()
    {
        if(isInRange)
        {
            //if(!gameObject.CompareTag("PressurePlate"))
                //mr.material.color = inRangeColor;
        }
        else
        {
            /*
            if (!gameObject.CompareTag("PressurePlate"))
                mr.material.color = oriColor;
            */
        }

        // very temp logic
        if (isButton && isActivated)
        {
            cooldownTimer -= Time.deltaTime;

            if (cooldownTimer < 0f)
            {
                transform.position = new Vector3(transform.position.x, .5f,transform.position.z );

                cooldownTimer = cooldownTime;
                isActivated = false;

                OnButtonExpired?.Invoke(this);
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

    public void ToggleLeverAnimation()
    {

        StartCoroutine(WaitForAnimation());
    }

    private IEnumerator WaitForAnimation()
    {
        if (isActivated)
        {
            animator.SetBool("turningOn", true);
            
        }
        else
        {
            animator.SetBool("turningOn", false);
        }

        yield return new WaitForSeconds(1);
        /*
        if(isActivated)
        {
            leverUp.SetActive(false);
            leverDown.SetActive(true);
        }
        else
        {
            leverUp.SetActive( true);
            leverDown.SetActive(false);
        }*/
       
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

        PlaySFX(PlayerAction.PressurePlate);

        OnActivatePressurePlate?.Invoke();
    }

    private void PressurePlateActivated(bool activated)
    {
        isActivated = activated;

        plateButton.localPosition = isActivated ? plateButtonActivatedPos : plateButtonOriPos;
        mr.material.color = isActivated ? plateButtonActivatedColor : oriColor;

        PlaySFX(PlayerAction.PressurePlate);

        OnActivatePressurePlate?.Invoke();
    }

    public void PressButton()
    {
        if (isActivated)
            return;

        isActivated = true;

        PlaySFX(PlayerAction.PressButton);

        //transform.localPosition = new Vector3(transform.localPosition.x, .3f, transform.localPosition.z);

        animator.SetTrigger("Press");

        OnPressButton?.Invoke(this);
    }

    public void ResetButton()
    {
        isActivated = false;
        cooldownTimer = cooldownTime;
        PlaySFX(PlayerAction.PressButton);

        //transform.localPosition = new Vector3(transform.localPosition.x, .5f,transform.localPosition.z);
    }

    public void PlaySFX(PlayerAction action)
    {
        switch (action)
        {
            case PlayerAction.Pickup:
                audioSource.PlayOneShot(sfxPickup);
                break;
            case PlayerAction.Drop:
                audioSource.PlayOneShot(sfxDrop);
                break;
            case PlayerAction.ToggleLever:
                audioSource.PlayOneShot(sfxLever);
                break;
            case PlayerAction.PressurePlate:
                audioSource.PlayOneShot(sfxPressurePlate);
                break;
            case PlayerAction.PressButton:
                audioSource.PlayOneShot(sfxButton);
                break;
        }

    }

}
