using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

using System.Collections;
using System.Collections.Generic;
using TMPro;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Variables")]
    public PlayerInput playerInput;
    public Vector2 moveInput;
    public Vector2 lookInput;
    public Vector2 sprintInput;
    private bool isCrouching = false;
    private bool isSprinting = false;
    public float crouchDepth = 2f;
    public float maxStamina, currentStamina = 100f;
    public float sprintCost;
    public Image staminaBar;

    [Header("Sens Variables")]
    public float walkSpeed = 5f;
    public float crouchSpeed = 2.5f;
    public float sprintSpeed = 8f;
    public float mouseSensitivity = 15f;

    // references
    public Rigidbody rb;
    public Transform cameraTransform;

    [Header("Collider Variables")]
    public CapsuleCollider playerCollider;
    public float crouchColliderHeight = 1f; 
    private float defaultColliderHeight;
    private Vector3 defaultColliderCenter;

    [Header("Flashlight Variables")]
    public Light spotlight;
    private bool flashlightOn;
    public float maxBattery, currentBattery = 100f;
    public float batteryDrain;
    public Image batteryBar;
    public float chargeRate;

    [Header("Coin Variables")]
    public int coinsCollected = 0;
    public int coinsToWin = 10; // Change this to how many coins you want in your maze
    public WinMenu winMenuScript;
    public TextMeshProUGUI coinText;

    // recharge
    private Coroutine rechargeStam;
    private Coroutine rechargeBattery;

    private float defaultCameraY;
    private float xRotation = 0f;

    void Start()
    {
        // hides cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        UpdateCoinUI(); //set the starting text

        // disables flash on start
        spotlight.enabled = false;

        // starting height
        if (cameraTransform != null)
        {
            defaultCameraY = cameraTransform.localPosition.y;
        }

        // store default collider settings so we can stand back up properly
        if (playerCollider != null)
        {
            defaultColliderHeight = playerCollider.height;
            defaultColliderCenter = playerCollider.center;
        }
    }

    void Update()
    {
        // rot logic
        if (lookInput.sqrMagnitude > 0.01f)
        {
            float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
            transform.Rotate(Vector3.up * mouseX);

            float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            if (cameraTransform != null)
            {
                cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            }
        }

        // camera crouch
        if (cameraTransform != null)
        {
            float targetY = isCrouching ? (defaultCameraY - crouchDepth) / 2 : defaultCameraY;

            // set new camera pos
            Vector3 newLocalPos = cameraTransform.localPosition;
            newLocalPos.y = targetY;
            cameraTransform.localPosition = newLocalPos;
        }

        // collider crouch
        if (playerCollider != null)
        {
            //  new height
            playerCollider.height = isCrouching ? crouchColliderHeight : defaultColliderHeight;
            float heightDifference = defaultColliderHeight - crouchColliderHeight;
            float targetCenterY = isCrouching ? (defaultColliderCenter.y - (heightDifference / 2f)) : defaultColliderCenter.y;

            playerCollider.center = new Vector3(defaultColliderCenter.x, targetCenterY, defaultColliderCenter.z);
        }

        // drains battery if flash on
        if (flashlightOn)
        {
            // curr level depends on how long its been on
            currentBattery = Mathf.Clamp(currentBattery - batteryDrain * Time.deltaTime, 0, maxBattery);
            // adjust the bar accordingly
            batteryBar.fillAmount = currentBattery / maxBattery;

            // bat recharge
            if (rechargeBattery != null)
            {
                StopCoroutine(rechargeBattery);
            }
            rechargeBattery = StartCoroutine(RechargeBattery());
        }
        // 0 bat -> disable flash
        if (currentBattery <= 0)
        {
            flashlightOn = false;
            spotlight.enabled = false;
        }
    }

    void FixedUpdate()
    {
        // speed on if we are walking or crouching
        float currentSpeed = isCrouching ? crouchSpeed : walkSpeed;

        // cond to check
        if (isSprinting && currentStamina > 0)
        {
            Vector3 moveDirection = (transform.right * moveInput.x + transform.forward * moveInput.y).normalized;
            Vector3 targetVelocity = moveDirection * sprintSpeed;
            // set speed to sprint
            rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);
            // adjust stam based on sprinting duration
            currentStamina -= sprintCost * Time.deltaTime;
            // to prevent going into negatives
            if (currentStamina < 0)
            {
                currentStamina = 0;
            }
            // adjust bar accordingly
            staminaBar.fillAmount = currentStamina / maxStamina;

            // stam recharge
            if (rechargeStam != null)
            {
                StopCoroutine(rechargeStam);
            }
            rechargeStam = StartCoroutine(RechargeStamina());
        }
        // set speed to crouch speed
        else if (isCrouching)
        {
            Vector3 moveDirection = (transform.right * moveInput.x + transform.forward * moveInput.y).normalized;
            Vector3 targetVelocity = moveDirection * crouchSpeed;
            // set crourch speed
            rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);
        }
        // else  default to reg walking speed
        else
        {
            Vector3 moveDirection = (transform.right * moveInput.x + transform.forward * moveInput.y).normalized;
            Vector3 targetVelocity = moveDirection * currentSpeed;
            // walking speed
            rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);
        }

    }

    // private methods for stam + bat recharge logic
    private IEnumerator RechargeStamina()
    {
        yield return new WaitForSeconds(1f);

        while (currentStamina < maxStamina)
        {
            currentStamina += chargeRate / 10f;
            if (currentStamina > maxStamina)
            {
                currentStamina = maxStamina;
            }
            staminaBar.fillAmount = currentStamina / maxStamina;
            yield return new WaitForSeconds(.1f);
        }
    }

    private IEnumerator RechargeBattery()
    {
        yield return new WaitForSeconds(1f);

        while (currentBattery < maxBattery)
        {
            currentBattery += chargeRate / 10f;
            if (currentBattery > maxBattery)
            {
                currentBattery = maxBattery;
            }
            batteryBar.fillAmount = currentBattery / maxBattery;
            yield return new WaitForSeconds(.1f);
        }
    }

    // input toggles
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }
    public void OnLook(InputValue value)
    {
        lookInput = value.Get<Vector2>();
    }
    public void OnCrouch(InputValue value)
    {
        isCrouching = value.isPressed;
    }
    public void OnSprint(InputValue value)
    {
        isSprinting = value.isPressed;
    }
    public void OnFlashlight(InputValue value)
    {
        flashlightOn = !flashlightOn;
        spotlight.enabled = flashlightOn;
    }

    private void OnTriggerEnter(Collider other)
    {
       //checks if we hit is coin
        if (other.CompareTag("Coin"))
        {
            
            coinsCollected++;

            //update the HUD and Console
            UpdateCoinUI();
            Debug.Log("Coins Collected: " + coinsCollected);

            //destroy the coin object so it disappears immediately
            Destroy(other.gameObject);

            //checks if we have enough coins to win
            if (coinsCollected >= coinsToWin)
            {
                WinGame();
            }
        }
    }

    void WinGame()
    {
        Debug.Log("win!");
        if (winMenuScript != null)
        {
            winMenuScript.ShowWinScreen(); 

            //unlock the mouse so you can actually click the "Play Again" buttons
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    void UpdateCoinUI()
    {
        if (coinText != null)
        {
            coinText.text = "Coins: " + coinsCollected + " / " + coinsToWin;
        }
    }
}