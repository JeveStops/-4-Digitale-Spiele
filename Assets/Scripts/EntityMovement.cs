using System;
using UnityEngine;

public class EntityMovement : MonoBehaviour
{
    [Header("Assignables")]
    public Transform playerCam; // Bei Gegnern kommt hier einfach ein leeres "Kopf"-Objekt rein
    public Transform orientation;
    
    [Header("Dance")]
    public Animator animator;
    public GameObject normalCamera;
    public GameObject danceCamera;
    public DanceCameraOrbit danceCameraOrbit;
    private bool isDancing;
    public bool IsDancing => isDancing;


    private Rigidbody rb;

    [Header("Look Settings")]
    private float xRotation;
    private float sensitivity = 50f;
    private float sensMultiplier = 1f;

    [Header("Movement Settings")]
    public float moveSpeed = 4500;
    public float maxSpeed = 20;
    public bool grounded;
    public LayerMask whatIsGround;
    public float counterMovement = 0.175f;
    private float threshold = 0.01f;
    public float maxSlopeAngle = 35f;

    [Header("Crouch & Slide")]
    private Vector3 crouchScale = new Vector3(1, 0.5f, 1);
    private Vector3 playerScale;
    public float slideForce = 400;
    public float slideCounterMovement = 0.2f;

    [Header("Jumping")]
    private bool readyToJump = true;
    private float jumpCooldown = 0.25f;
    public float jumpForce = 250f;

    // --- NEU: Diese Variablen werden von "Auﬂen" (PlayerInput oder AI) gesteuert ---
    [HideInInspector] public float inputX, inputY;
    [HideInInspector] public bool inputJumping, inputCrouching;
    [HideInInspector] public float inputMouseX, inputMouseY;
    // --------------------------------------------------------------------------------

    private Vector3 normalVector = Vector3.up;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        playerScale = transform.localScale;

            if (normalCamera != null)
        normalCamera.SetActive(true);

        if (danceCamera != null)
            danceCamera.SetActive(false);
    }

    private void FixedUpdate()
    {
        Movement();
    }

    private void Update()
    {
        if (!isDancing)
        {
            Look();
        }
    }

    // NEU: Von auﬂen aufrufbar, wenn die Taste (oder die KI) Ducken dr¸ckt
    public void StartCrouch()
    {
        transform.localScale = crouchScale;
        transform.position = new Vector3(transform.position.x, transform.position.y - 0.5f, transform.position.z);
        if (rb.linearVelocity.magnitude > 0.5f)
        {
            if (grounded)
            {
                rb.AddForce(orientation.transform.forward * slideForce);
            }
        }
    }

    // NEU: Von auﬂen aufrufbar, wenn Ducken losgelassen wird
    public void StopCrouch()
    {
        transform.localScale = playerScale;
        transform.position = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);
    }

    private void Movement()
    {
        rb.AddForce(Vector3.down * Time.deltaTime * 10);

        Vector2 mag = FindVelRelativeToLook();
        float xMag = mag.x, yMag = mag.y;

        // Wir nutzen jetzt inputX und inputY anstelle von x und y!
        CounterMovement(inputX, inputY, mag);

        if (readyToJump && inputJumping) Jump();

        float maxSpeed = this.maxSpeed;

        if (inputCrouching && grounded && readyToJump)
        {
            rb.AddForce(Vector3.down * Time.deltaTime * 3000);
            return;
        }

        if (inputX > 0 && xMag > maxSpeed) inputX = 0;
        if (inputX < 0 && xMag < -maxSpeed) inputX = 0;
        if (inputY > 0 && yMag > maxSpeed) inputY = 0;
        if (inputY < 0 && yMag < -maxSpeed) inputY = 0;

        float multiplier = 1f, multiplierV = 1f;

        if (!grounded)
        {
            multiplier = 0.5f;
            multiplierV = 0.5f;
        }

        if (grounded && inputCrouching) multiplierV = 0f;

        rb.AddForce(orientation.transform.forward * inputY * moveSpeed * Time.deltaTime * multiplier * multiplierV);
        rb.AddForce(orientation.transform.right * inputX * moveSpeed * Time.deltaTime * multiplier);
    }

    private void Jump()
    {
        if (grounded && readyToJump)
        {
            readyToJump = false;
            rb.AddForce(Vector2.up * jumpForce * 1.5f);
            rb.AddForce(normalVector * jumpForce * 0.5f);

            Vector3 vel = rb.linearVelocity;
            if (rb.linearVelocity.y < 0.5f)
                rb.linearVelocity = new Vector3(vel.x, 0, vel.z);
            else if (rb.linearVelocity.y > 0)
                rb.linearVelocity = new Vector3(vel.x, vel.y / 2, vel.z);

            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void ResetJump()
    {
        readyToJump = true;
    }

    private void Look()
    {
        // Wir nutzen hier inputMouseX und Y!
        float mX = inputMouseX * sensitivity * Time.fixedDeltaTime * sensMultiplier;
        float mY = inputMouseY * sensitivity * Time.fixedDeltaTime * sensMultiplier;

        Vector3 rot = playerCam.transform.localRotation.eulerAngles;
        float desiredX = rot.y + mX;

        xRotation -= mY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        playerCam.transform.localRotation = Quaternion.Euler(xRotation, desiredX, 0);
        orientation.transform.localRotation = Quaternion.Euler(0, desiredX, 0);
    }

    private void CounterMovement(float x, float y, Vector2 mag)
    {
        if (!grounded || inputJumping) return;

        if (inputCrouching)
        {
            rb.AddForce(moveSpeed * Time.deltaTime * -rb.linearVelocity.normalized * slideCounterMovement);
            return;
        }

        if (Math.Abs(mag.x) > threshold && Math.Abs(x) < 0.05f || (mag.x < -threshold && x > 0) || (mag.x > threshold && x < 0))
        {
            rb.AddForce(moveSpeed * orientation.transform.right * Time.deltaTime * -mag.x * counterMovement);
        }
        if (Math.Abs(mag.y) > threshold && Math.Abs(y) < 0.05f || (mag.y < -threshold && y > 0) || (mag.y > threshold && y < 0))
        {
            rb.AddForce(moveSpeed * orientation.transform.forward * Time.deltaTime * -mag.y * counterMovement);
        }

        if (Mathf.Sqrt((Mathf.Pow(rb.linearVelocity.x, 2) + Mathf.Pow(rb.linearVelocity.z, 2))) > maxSpeed)
        {
            float fallspeed = rb.linearVelocity.y;
            Vector3 n = rb.linearVelocity.normalized * maxSpeed;
            rb.linearVelocity = new Vector3(n.x, fallspeed, n.z);
        }
    }

    public Vector2 FindVelRelativeToLook()
    {
        float lookAngle = orientation.transform.eulerAngles.y;
        float moveAngle = Mathf.Atan2(rb.linearVelocity.x, rb.linearVelocity.z) * Mathf.Rad2Deg;

        float u = Mathf.DeltaAngle(lookAngle, moveAngle);
        float v = 90 - u;

        float magnitue = rb.linearVelocity.magnitude;
        float yMag = magnitue * Mathf.Cos(u * Mathf.Deg2Rad);
        float xMag = magnitue * Mathf.Cos(v * Mathf.Deg2Rad);

        return new Vector2(xMag, yMag);
    }

    private bool IsFloor(Vector3 v)
    {
        float angle = Vector3.Angle(Vector3.up, v);
        return angle < maxSlopeAngle;
    }

    private bool cancellingGrounded;

    private void OnCollisionStay(Collision other)
    {
        int layer = other.gameObject.layer;
        if (whatIsGround != (whatIsGround | (1 << layer))) return;

        for (int i = 0; i < other.contactCount; i++)
        {
            Vector3 normal = other.contacts[i].normal;
            if (IsFloor(normal))
            {
                grounded = true;
                cancellingGrounded = false;
                normalVector = normal;
                CancelInvoke(nameof(StopGrounded));
            }
        }

        float delay = 3f;
        if (!cancellingGrounded)
        {
            cancellingGrounded = true;
            Invoke(nameof(StopGrounded), Time.deltaTime * delay);
        }
    }

    private void StopGrounded()
    {
        grounded = false;
    }

    public void StartDance()
    {
        if (isDancing) return;

        isDancing = true;

        animator.SetBool("Dance", true);

        if (normalCamera != null) normalCamera.SetActive(false);
        if (danceCamera != null) danceCamera.SetActive(true);

        if (danceCameraOrbit != null)
        {
            danceCameraOrbit.StartOrbitFromCurrentView(normalCamera.transform);
        }
    }

    public void StopDance()
    {
        if (!isDancing) return;

        isDancing = false;

        animator.SetBool("Dance", false);

        if (danceCamera != null) danceCamera.SetActive(false);
        if (normalCamera != null) normalCamera.SetActive(true);
    }

}
