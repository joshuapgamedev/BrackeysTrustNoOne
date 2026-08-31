using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 6.0f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;

    [Header("Look Settings")]
    public Transform playerCamera;
    public float mouseSensitivity = 2.0f;
    public float lookUpLimit = -85f;
    public float lookDownLimit = 85f;

    [Header("Pickup/Letgo")]
    [SerializeField] private Transform handPos;
    [SerializeField] private LayerMask groundLayer;

    [SerializeField] private float holdFollowSpeed = 12f;
    [SerializeField] private float maxHoldSpeed = 8f;

    private Rigidbody heldRb;
    private bool heldOriginalUseGravity;

    /*
    [Header("Health")]
    [SerializeField] private float maxHealth = 5f;
    [SerializeField] private TextMeshProUGUI healthText;
    */

    [Header("UI")]
    [SerializeField] private GameObject crosshair;
    [SerializeField] private GameObject grabCrosshair;


    private CharacterController characterController;
    private Vector3 velocity;
    private bool isGrounded;
    private float cameraRotationX = 0f;

    // pickup / let go
    private bool hasObjectInRange = false;
    private ObjectController currentFocus = null;
    private ObjectController currentHolding = null;

    private bool isHoldingObject = false;

    //private float health;
    private bool damangeCooldown = false;

   // private float cooldownTime = 2f;
    private float cooldownTimer = 0f;

    //private Coroutine regenRoutine = null;

    public static bool CanMove { get; set; } = false;

    public static event System.Action OnPlayerJumped;
    public static event System.Action OnPlayerToggleLever;

    public static PlayerController Instance { get; private set; }
    void Start()
    {
        Instance = this;

        characterController = GetComponent<CharacterController>();

        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;

        //health = maxHealth;
        //UpdateHealthText();

    }

    void Update()
    {
        if (CanMove)
        {
            FPSMovement();
        }


        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (isHoldingObject && currentHolding != null)
            {
                ReleaseHeldObject();

                return;
            }

            if (hasObjectInRange)
            {
                CheckObjectTag();
                //currentHolding = currentFocus;
                //isHoldingObject = true;
            }
        }

        /*
        if (isHoldingObject && currentHolding != null)
        {
            HoldingObject();
        }
        */

        if (damangeCooldown)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0)
            {
                damangeCooldown = false;
            }
        }
    }

    private void FixedUpdate()
    {
        if (isHoldingObject && currentHolding != null && heldRb != null)
        {
            //HoldingObject();
            HoldObject();
        }
    }

    private void FPSMovement()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        cameraRotationX -= mouseY;
        cameraRotationX = Mathf.Clamp(cameraRotationX, lookUpLimit, lookDownLimit);

        playerCamera.localRotation = Quaternion.Euler(cameraRotationX, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);

        isGrounded = characterController.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 moveDirection = transform.right * moveX + transform.forward * moveZ;
        characterController.Move(moveDirection * walkSpeed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            OnPlayerJumped?.Invoke();
        }

        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Holdable")
            || other.gameObject.CompareTag("Toggleable")
            || other.gameObject.CompareTag("Interactable"))
        {
            //Debug.Log("in range Holdable");
            hasObjectInRange = true;
            currentFocus = other.GetComponent<ObjectController>();
            currentFocus.ObjectInRange(true);

            ToggleCrosshair(true);

        }
        if (other.gameObject.CompareTag("PressurePlate"))
        {
            currentFocus = other.GetComponent<ObjectController>();
            currentFocus.TriggerPressurePlate();

        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Holdable")
            || other.gameObject.CompareTag("Toggleable")
            || other.gameObject.CompareTag("Interactable"))
        {
            //Debug.Log("out of range Holdable");
            hasObjectInRange = false;
            if(currentFocus!= null)
                currentFocus.ObjectInRange(false);
            currentFocus = null;

            ToggleCrosshair(false);
        }
        if (other.gameObject.CompareTag("PressurePlate"))
        {
            currentFocus.TriggerPressurePlate();
            currentFocus = null;

        }
    }

    /*
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Danger"))
        {
            if(!damangeCooldown)
            {
                TakeDamage(1f);
                cooldownTimer = cooldownTime;
                damangeCooldown = true;
            }
        }
    }
    */

    private void HoldObject()
    {
        ToggleCrosshair(true, true);

        Vector3 holdOffset = currentHolding.HoldPoint.position - currentHolding.transform.position;

        Vector3 targetRootPosition = handPos.position - holdOffset;

        Vector3 direction = targetRootPosition - heldRb.position;

        Vector3 targetVelocity = direction * holdFollowSpeed;

        // Prevent ridiculous physics launches
        targetVelocity = Vector3.ClampMagnitude(targetVelocity, maxHoldSpeed);

        heldRb.velocity = targetVelocity;
    }

    private void HoldingObject()
    {
        ToggleCrosshair(true, true);
        Vector3 targetPos = handPos.position;

        bool touchingWall = false;

        /*
        Collider[] colliders = currentHolding.GetComponents<Collider>();

        Collider objectCollider = null;

        foreach (Collider col in colliders)
        {
            if (!col.isTrigger)
            {
                objectCollider = col;
                break;
            }
        }*/

        if (currentHolding.transform.childCount > 2)
        {
            Collider[] colliders = currentHolding.GetComponentsInChildren<Collider>();

            float lowestColliderY = float.MaxValue;

            foreach (Collider col in colliders)
            {
                if (col.isTrigger)
                    continue;

                lowestColliderY = Mathf.Min(
                    lowestColliderY,
                    col.bounds.min.y
                );
            }

            Vector3 rayOrigin = targetPos + Vector3.up * 2f;

            /**
            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 5f, groundLayer, QueryTriggerInteraction.Ignore))
            {
                float pivotToBottom = currentHolding.transform.position.y - lowestColliderY;

                float minimumRootY = hit.point.y + pivotToBottom;

                targetPos.y = Mathf.Max(targetPos.y, minimumRootY);
            }*/

            RaycastHit wallHit;

            if (Physics.Raycast(playerCamera.position, playerCamera.forward, out wallHit,  3f, groundLayer, QueryTriggerInteraction.Ignore))
            {
                if (Mathf.Abs(wallHit.normal.y) < 0.3f)
                {
                    touchingWall = true;

                    Debug.Log($"Detected wall: {wallHit.collider.name}");
                }
            }

            if (Mathf.Abs(wallHit.normal.y) < 0.3f)
            {
                Debug.Log($"Detected wall: {wallHit.collider.name}");
            }
            else
            {
                Debug.Log($"{wallHit.collider.name}");
            }

            if (touchingWall)
            {
                targetPos = wallHit.point + wallHit.normal * 0.3f;
            }

            currentHolding.transform.position = targetPos;
            currentHolding.ObjectInRange(true);
        }
        else
        {
            Collider objectCollider = currentHolding.transform.GetChild(0).GetComponent<Collider>();

            Vector3 holdOffset = currentHolding.HoldPoint.position - currentHolding.transform.position;

            Vector3 rootPos = targetPos - holdOffset;

            // floor
            Vector3 rayOrigin = targetPos + Vector3.up * 2f;

            
            if (Physics.Raycast(rayOrigin,Vector3.down, out RaycastHit groundHit, 5f, groundLayer, QueryTriggerInteraction.Ignore))
            {
                //Debug.Log($"Ray hit: {hit.collider.name}, Y: {hit.point.y}");

                float objectHalfHeight = objectCollider.bounds.extents.y;

                float minimumY = groundHit.point.y + objectHalfHeight;

                rootPos.y = Mathf.Max(rootPos.y, minimumY);
                //targetPos.y = Mathf.Max(targetPos.y, minimumY);

                //Debug.Log($"TargetY: {targetPos.y}, " + $"ExtentsY: {objectCollider.bounds.extents.y}, " + $"MinY: {minimumY}");
            }

            // wall
            if (Physics.Raycast(playerCamera.position, playerCamera.forward, out RaycastHit wallHit,  3f, groundLayer, QueryTriggerInteraction.Ignore))
            {
                if (Mathf.Abs(wallHit.normal.y) < 0.3f)
                {
                    Bounds bounds = objectCollider.bounds;

                    float extentTowardWall = Mathf.Abs(wallHit.normal.x) * bounds.extents.x + Mathf.Abs(wallHit.normal.y) * bounds.extents.y + Mathf.Abs(wallHit.normal.z) * bounds.extents.z;

                    float distanceFromWall = Vector3.Dot(rootPos - wallHit.point, wallHit.normal);

                    float padding = 0.02f;
                    float requiredDistance = extentTowardWall + padding;

                    // push it if it gonna enter the wall
                    if (distanceFromWall < requiredDistance - 0.02f)
                    {
                        rootPos += wallHit.normal * (requiredDistance - distanceFromWall);
                    }
                }
            }

            //currentHolding.transform.position = targetPos;

            currentHolding.transform.position = rootPos;
            //currentHolding.transform.position = targetPos - holdOffset;

            currentHolding.ObjectInRange(true);
        }

            

    }

    public void ReleaseHeldObject(bool playDropSFX = true)
    {
        StartCoroutine(ToggleCrosshairWhenReleased());

        if (!isHoldingObject || currentHolding == null)
            return;

        if (playDropSFX)
            currentHolding.PlaySFX(PlayerAction.Drop);

        currentHolding.ObjectInRange(false);

        // new
        if (heldRb != null)
        {
            heldRb.useGravity = heldOriginalUseGravity;

            heldRb.velocity = Vector3.zero;
            heldRb.angularVelocity = Vector3.zero;
        }

        heldRb = null;
        //

        currentHolding = null;
        isHoldingObject = false;
    }

    /*
    private void TakeDamage(float amount)
    {
        health = Mathf.Clamp(health - amount, 0, maxHealth);
        //Debug.Log($"health: {health}");
        UpdateHealthText();

        if(regenRoutine != null)
        {
            StopCoroutine(regenRoutine);
            regenRoutine = StartCoroutine(RegeneratingHealth());
        }
        else
        {
            regenRoutine = StartCoroutine(RegeneratingHealth());
        }
        
    }
    

    private IEnumerator RegeneratingHealth()
    {
        //Debug.Log($"RegeneratingHealth");
        while (health < maxHealth)
        {
            yield return new WaitForSeconds(3);
            health++;

            //Debug.Log($"regen: {health}");
            UpdateHealthText();
        }

        regenRoutine = null;
    }

    private void UpdateHealthText()
    {
        healthText.text = health.ToString();
    }
    */
    private void ToggleObject()
    {
        StartCoroutine(ToggleCrosshairForSecond());
        /*
        // very temp logic
        if (currentFocus.transform.localEulerAngles.z < 300 )
            currentFocus.transform.localEulerAngles = new Vector3(0f, 0f, -45f);
        else if (currentFocus.transform.localEulerAngles.z > 300)
            currentFocus.transform.localEulerAngles = new Vector3(0f, 0f, 45f);
        */

        currentFocus.Activated();
        currentFocus.ToggleLeverAnimation();

        OnPlayerToggleLever?.Invoke();
        currentFocus.PlaySFX(PlayerAction.ToggleLever);
    }

    

    private void CheckObjectTag()
    {
        if(currentFocus.CompareTag("Holdable"))
        {
            currentHolding = currentFocus;
            isHoldingObject = true;

            // new

            heldRb = currentHolding.GetComponent<Rigidbody>();
            if (heldRb == null)
            {
                heldRb = currentHolding.GetComponentInChildren<Rigidbody>();
            }
            if (heldRb != null)
            {
                heldOriginalUseGravity = heldRb.useGravity;

                // While we're actively pulling it toward the hand,
                // gravity isn't particularly useful.
                heldRb.useGravity = false;

                heldRb.velocity = Vector3.zero;
                heldRb.angularVelocity = Vector3.zero;
            }
            else
            {
                Debug.LogWarning( $"where is {currentHolding.name}'s rigibody?????" );
            }
            //

            currentHolding.PlaySFX(PlayerAction.Pickup);
        } 
        else if(currentFocus.CompareTag("Toggleable"))
        {
            ToggleObject();
        }
        else if (currentFocus.CompareTag("Interactable"))
        {
            StartCoroutine(ToggleCrosshairForSecond());
            currentFocus.PressButton();
            //InteractWithObject();
        }
    }

    public bool IsHolding(ObjectController obj)
    {
        return isHoldingObject && currentHolding == obj;
    }

    private void ToggleCrosshair(bool on, bool isGrabbing = false)
    {
        if(!on)
        {
            crosshair.SetActive(false);
            grabCrosshair.SetActive(false);
            return;
        }

        crosshair.SetActive(isGrabbing?false:true);
        grabCrosshair.SetActive(isGrabbing?true:false);
    }

    private IEnumerator ToggleCrosshairForSecond()
    {
        ToggleCrosshair(true, true);
        yield return new WaitForSeconds(.5f);
        ToggleCrosshair(true, false);
    }

    private IEnumerator ToggleCrosshairWhenReleased()
    {
        ToggleCrosshair(true, false);
        yield return new WaitForSeconds(.2f);
        ToggleCrosshair(false);
    }

}

