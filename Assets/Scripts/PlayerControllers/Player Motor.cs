using System.Collections;
using UnityEngine;

public class PlayerMotor : MonoBehaviour
{
    private CharacterController controller;
    private Vector3 playerVelocity;
    public float speed = 5f;
    public float gravity = -9.81f * 2;
    public float jumpHeight = 3f;
    bool sprinting = false;
    private bool isGrounded;
    public float walkSpeed = 8f;
    public float sprintSpeed = 12f;
    public float scopeSpeed = 3f;
    private bool isScoped = false;

    public bool invertedControls = false;


    [Header("Flying Settings")]
    public bool flyingEnabled = false;
    public float flySpeed = 6f;
    public float maxFlyHeight = 10f;
    public float flyGravity = -2f;
    public float flyRiseSpeed = 5f;
    public float flyDescendSpeed = 4f;
    [Header("Flying Animation Settings")]
    public float levitationSpeed = 3f;
    public float hoverHeight = 5f;
    public float hoverAmplitude = 0.5f;
    public float hoverFrequency = 1f;
    public float descentSpeed = 2f;

    private float groundLevel;
    private bool wasGroundedBeforeFlying;
    private bool isLevitating = false;
    private bool isDescending = false;
    private float hoverTimer = 0f;
    private float targetHoverHeight;
    private Coroutine flyingTransitionCoroutine;

    public ParticleSystem sprintEffect;
    public bool SpringEffectShowing;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        speed = walkSpeed;
        groundLevel = transform.position.y;

        if (sprintEffect != null)
        {
            sprintEffect.Stop();
            SpringEffectShowing = false;
        }
    }

    void Update()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && !flyingEnabled)
        {
            groundLevel = transform.position.y;
        }

        SpringEffectShowing = sprinting && !isScoped && isGrounded && !flyingEnabled;

        if (flyingEnabled && !isLevitating && !isDescending)
        {
            hoverTimer += Time.deltaTime;
        }
    }

    public void ProcessMove(Vector2 input)
    {
        Vector3 moveDirection = Vector3.zero;

        if (invertedControls)
        {
            moveDirection.x = -input.x;
            moveDirection.z = -input.y;
        }
        else
        {
            moveDirection.x = input.x;
            moveDirection.z = input.y;
        }
        controller.Move(transform.TransformDirection(moveDirection) * speed * Time.deltaTime);

        if (flyingEnabled)
        {
            HandleFlyingGravity();
            HandleFlyingMovement();
        }
        else
        {
            HandleNormalGravity();
        }
        controller.Move(playerVelocity * Time.deltaTime);
    }
    void HandleNormalGravity()
    {
        playerVelocity.y += gravity * Time.deltaTime;
        if (isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = -1f;
        }
    }
    void HandleFlyingMovement()
    {
        if (isLevitating)
        {
            float currentHeight = transform.position.y - groundLevel;
            if (currentHeight < targetHoverHeight)
            {
                playerVelocity.y = levitationSpeed;
            }
            else
            {
                playerVelocity.y = 0f;
                isLevitating = false;
                hoverTimer = 0f;

            }
        }
        else if (!isDescending)
        {
            float hoverOffset = Mathf.Sin(hoverTimer * hoverFrequency * 2f * Mathf.PI) * hoverAmplitude;
            float targetY = groundLevel + targetHoverHeight + hoverOffset;
            float currentY = transform.position.y;

            if (Mathf.Abs(targetY - currentY) > 0.1f)
            {
                playerVelocity.y = (targetY - currentY) * 2f;
            }
            else
            {
                playerVelocity.y = 0f;
            }
        }
    }
    void HandleFlyingGravity()
    {
        playerVelocity.y += flyGravity * Time.deltaTime;

        float currentHeight = transform.position.y - groundLevel;
        if (currentHeight >= maxFlyHeight && playerVelocity.y > 0)
        {
            playerVelocity.y = 0f;
        }
        if (transform.position.y <= groundLevel && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }
    }
    /*public void ProcessFlyInput(bool flyUp, bool flyDown)
    {
        if (!flyingEnabled) return;
        if (flyUp)
        {
            float currentHeight = transform.position.y - groundLevel;
            if (currentHeight < maxFlyHeight)
            {
                playerVelocity.y = -flyDescendSpeed;
            }
        }
    }
    No need to hit the space bar to fly happens automatically once the ability starts*/

    public void Jump()
    {
        if (flyingEnabled)
        {
            /*float currentHeight = transform.position.y - groundLevel;
            if (currentHeight < maxFlyHeight)
            {
                playerVelocity.y = flyRiseSpeed;
            } No Manual flying */
            return;
        }
        else if (isGrounded)
        {
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -3.0f * gravity);
        }
    }

    public void Sprint(bool isSprinting)
    {
        sprinting = isSprinting;

        if (!isScoped)
        {
            if (isSprinting)
            {
                speed = flyingEnabled ? flySpeed * 1.5f : sprintSpeed;
                if (isGrounded && !flyingEnabled && sprintEffect != null && !sprintEffect.isPlaying)
                {
                    sprintEffect.Play();
                }
            }
            else
            {
                speed = flyingEnabled ? flySpeed : walkSpeed;
                if (sprintEffect != null && sprintEffect.isPlaying)
                {
                    sprintEffect.Stop();
                }
            }
        }
        else
        {
            if (sprintEffect != null && sprintEffect.isPlaying)
            {
                sprintEffect.Stop();
            }
        }
    }

    public void SetScoped(bool scoped)
    {
        isScoped = scoped;

        if (scoped)
        {
            speed = scopeSpeed;
            if (sprintEffect != null && sprintEffect.isPlaying)
            {
                sprintEffect.Stop();
            }
        }
        else
        {
            if (flyingEnabled)
            {
                speed = sprinting ? flySpeed * 1.5f : flySpeed;
            }
            speed = sprinting ? sprintSpeed : walkSpeed;
            if (sprinting && isGrounded && sprintEffect != null && !sprintEffect.isPlaying)
            {
                sprintEffect.Play();
            }
        }
    }
    public void SetInvertedControls(bool inverted)
    {
        invertedControls = inverted;
    }
    public void ToggleInvertedControls()
    {
        invertedControls = !invertedControls;
    }
    public bool GetInvertedControls()
    {
        return invertedControls;
    }
    public void SetFlying(bool flying)
    {
        if (flyingEnabled != flying)
        {
            if (flyingTransitionCoroutine != null)
            {
                StopCoroutine(flyingTransitionCoroutine);
                flyingTransitionCoroutine = null;
            }
            flyingEnabled = flying;
            if (flying)
            {
                StartFlying();
            }
            else
            {
                StopFlying();
            }
        }
    }
    void StartFlying()
    {
        wasGroundedBeforeFlying = isGrounded;
        targetHoverHeight = hoverHeight;
        isLevitating = true;
        isDescending = false;
        hoverTimer = 0f;

        speed = sprinting ? flySpeed * 1.5f : flySpeed;

        if (sprintEffect != null && sprintEffect.isPlaying)
        {
            sprintEffect.Stop();
        }
    }
    void StopFlying()
    {
        isLevitating = false;
        isDescending = true;
        flyingTransitionCoroutine = StartCoroutine(DescentSequence());
    }
    IEnumerator DescentSequence()
    {
        while (transform.position.y > groundLevel + 0.5f)
        {
            playerVelocity.y = -descentSpeed;
            yield return null;
        }
        while (transform.position.y > groundLevel + 0.1f)
        {
            playerVelocity.y = -descentSpeed * 0.5f;
            yield return null;
        }

        playerVelocity.y = 0f;
        isDescending = false;

        speed = sprinting ? sprintSpeed : walkSpeed;

        if (sprinting && sprintEffect != null && !sprintEffect.isPlaying)
        {
            sprintEffect.Play();
        }
        flyingTransitionCoroutine = null;
    }
    public void ToggleFlying()
    {
        SetFlying(!flyingEnabled);
    }
    public bool GetFlying()
    {
        return flyingEnabled;
    }
    public float GetCurrentFlyHeight()
    {
        return Mathf.Max(0f, transform.position.y - groundLevel);
    }
    public float GetMaxFlyHeight()
    {
        return maxFlyHeight;
    }
    public bool IsLevitating()
    {
        return isLevitating;
    }
    public bool IsDescending()
    {
        return isDescending;
    }
    public bool IsHovering()
    {
        return flyingEnabled && !isLevitating && !isDescending;
    }

}