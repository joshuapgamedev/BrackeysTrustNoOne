using System.Collections;
using System.Collections.Generic;
using TMPro;
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

    [Header("Health")]
    [SerializeField] private float maxHealth = 5f;
    [SerializeField] private TextMeshProUGUI healthText;

    private CharacterController characterController;
    private Vector3 velocity;
    private bool isGrounded;
    private float cameraRotationX = 0f;

    // pickup / let go
    private bool hasObjectInRange = false;
    private ObjectController currentFocus = null;
    private ObjectController currentHolding = null;

    private bool isHoldingObject = false;

    private float health;
    private bool damangeCooldown = false;

    private float cooldownTime = 2f;
    private float cooldownTimer = 0f;

    private Coroutine regenRoutine = null;
    void Start()
    {
        characterController = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        health = maxHealth;
        UpdateHealthText();

    }

    void Update()
    {
        FPSMovement();
        
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            if(isHoldingObject && currentHolding != null)
            {
                currentHolding.ObjectInRange(false);
                currentHolding = null;
                isHoldingObject = false;
            }

            if(hasObjectInRange)
            {
                currentHolding = currentFocus;
                isHoldingObject = true;
            }
        }

        if(isHoldingObject && currentHolding != null)
        {
            HoldingObject();
        }

        if (damangeCooldown)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0)
            {
                damangeCooldown = false;
            }
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
        }

        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Holdable"))
        {
            Debug.Log("in range Holdable");
            hasObjectInRange = true;
            currentFocus = other.GetComponent<ObjectController>();
            currentFocus.ObjectInRange(true);
        }

        
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Holdable"))
        {
            Debug.Log("out of range Holdable");
            hasObjectInRange = false;
            if(currentFocus!= null)
                currentFocus.ObjectInRange(false);
            currentFocus = null;
            
        }
    }

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

    private void HoldingObject()
    {
        // size / 2 
        Vector3 targetPos = handPos.transform.position;
        targetPos.y = Mathf.Clamp(targetPos.y, .5f, handPos.transform.position.y);
        
        currentHolding.transform.position = targetPos;

        currentHolding.ObjectInRange(true);
    }

    private void TakeDamage(float amount)
    {
        health = Mathf.Clamp(health - amount, 0, maxHealth);
        Debug.Log($"health: {health}");
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
        Debug.Log($"RegeneratingHealth");
        while (health < maxHealth)
        {
            yield return new WaitForSeconds(3);
            health++;

            Debug.Log($"regen: {health}");
            UpdateHealthText();
        }

        regenRoutine = null;
    }

    private void UpdateHealthText()
    {
        healthText.text = health.ToString();
    }
}

