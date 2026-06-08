using UnityEngine;
using Photon.Pun;
using UnityEngine.EventSystems; 
using System.Collections; 

public class PlayerController : MonoBehaviourPun
{
    [Header("Di Chuyển")]
    public float moveSpeed = 5f;
    private Animator animator;
    private PlayerCombat combatScript;

    [Header("Kỹ năng Lướt (Space)")]
    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    private bool isDashing = false;
    private float nextDashTime = 0f;

    [Header("Point & Click")]
    public bool isAutoMoving = false;
    private Vector2 targetPosition; 
    private Transform targetEnemy;  

    // --- ĐÃ THÊM: BIẾN ÂM THANH BƯỚC CHÂN ---
    [Header("Âm thanh Bước Chân")]
    public AudioClip[] footstepSounds; 
    public float stepInterval = 0.35f; 
    private float nextStepTime = 0f;
    private AudioSource audioSource;
    // ----------------------------------------

    void Start()
    {
        animator = GetComponentInChildren<Animator>(); 
        combatScript = GetComponent<PlayerCombat>();
        
        // ĐÃ THÊM: Tự động lấy cái loa (AudioSource) trên nhân vật
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        if (!photonView.IsMine) return;
        
        if (ChatManager.isChatOpen) 
        {
            if (animator != null) animator.SetBool("isWalking", false);
            isAutoMoving = false;
            targetEnemy = null;
            return; 
        }

        if (isDashing || (combatScript != null && combatScript.isBlocking)) return;

        if (Input.GetKeyDown(KeyCode.Space) && Time.time >= nextDashTime)
        {
            StartCoroutine(DashRoutine());
            return; 
        }
        
        if (SkillHUD.instance != null)
        {
            float dashTimer = Mathf.Max(0, nextDashTime - Time.time);
            SkillHUD.instance.UpdateCooldown("Dash", dashTimer, dashCooldown);
        }

        HandleMouseInput();
        HandleMovement();
        
        // ĐÃ THÊM: Chạy logic âm thanh bước chân ở cuối mỗi khung hình
        HandleFootsteps(); 
    }

    void HandleMouseInput()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Collider2D hit = Physics2D.OverlapPoint(mousePos);

            if (hit != null && hit.CompareTag("Enemy")) 
            {
                targetEnemy = hit.transform;
                isAutoMoving = false; 
            }
            else
            {
                targetPosition = mousePos;
                targetEnemy = null;
                isAutoMoving = true;
            }
        }
        
        // CHUỘT PHẢI (TUNG SKILL)
        if (Input.GetMouseButtonDown(1))
        {
            if (targetEnemy != null)
            {
                combatScript.ManualSkillAttack(targetEnemy);
            }
            else 
            {
                Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Collider2D hit = Physics2D.OverlapPoint(mousePos);
                if (hit != null && hit.CompareTag("Enemy"))
                {
                    targetEnemy = hit.transform;
                    combatScript.ManualSkillAttack(targetEnemy);
                }
            }
        }
    }

    void HandleMovement()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        bool isKeyboardMoving = (moveX != 0 || moveY != 0);

        if (isKeyboardMoving)
        {
            isAutoMoving = false; 
            targetEnemy = null;   

            Vector2 movement = new Vector2(moveX, moveY).normalized;
            transform.position += (Vector3)movement * moveSpeed * Time.deltaTime;
            
            if (animator != null) animator.SetBool("isWalking", true); 
            FaceDirection(moveX);
        }
        else if (targetEnemy != null)
        {
            if (!targetEnemy.gameObject.activeInHierarchy)
            {
                targetEnemy = null;
                if (animator != null) animator.SetBool("isWalking", false);
                return;
            }

            float distToEnemy = Vector2.Distance(transform.position, targetEnemy.position);

            if (distToEnemy > combatScript.attackRange)
            {
                transform.position = Vector2.MoveTowards(transform.position, targetEnemy.position, moveSpeed * Time.deltaTime);
                if (animator != null) animator.SetBool("isWalking", true);
                FaceDirection(targetEnemy.position.x - transform.position.x);
            }
            else
            {
                if (animator != null) animator.SetBool("isWalking", false); 
                FaceDirection(targetEnemy.position.x - transform.position.x);
                combatScript.AutoAttackTarget(targetEnemy); 
            }
        }
        else if (isAutoMoving)
        {
            float distToTarget = Vector2.Distance(transform.position, targetPosition);
            if (distToTarget > 0.1f)
            {
                transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
                if (animator != null) animator.SetBool("isWalking", true);
                FaceDirection(targetPosition.x - transform.position.x);
            }
            else
            {
                isAutoMoving = false;
                if (animator != null) animator.SetBool("isWalking", false);
            }
        }
        else
        {
            if (animator != null) animator.SetBool("isWalking", false);
        }
    }

    // --- ĐÃ THÊM: HÀM XỬ LÝ ÂM THANH BƯỚC CHÂN CHUYÊN DỤNG ---
    void HandleFootsteps()
    {
        // Lấy trạng thái từ Animator xem có đang đi bộ không
        bool isWalking = false;
        if (animator != null) isWalking = animator.GetBool("isWalking");

        if (isWalking && Time.time >= nextStepTime)
        {
            if (footstepSounds.Length > 0 && audioSource != null)
            {
                int randomIndex = Random.Range(0, footstepSounds.Length);
                audioSource.pitch = Random.Range(0.85f, 1.15f); // Bóp méo pitch
                audioSource.PlayOneShot(footstepSounds[randomIndex], 0.4f); // Volume 40%
            }
            nextStepTime = Time.time + stepInterval; 
        }
    }
    // ---------------------------------------------------------

    void FaceDirection(float dirX)
    {
        if (Mathf.Abs(dirX) < 0.01f) return;
        Vector3 scale = transform.localScale;
        if (dirX > 0) scale.x = -Mathf.Abs(scale.x); 
        else if (dirX < 0) scale.x = Mathf.Abs(scale.x); 
        transform.localScale = scale;
    }    
    
    IEnumerator DashRoutine()
    {
        isDashing = true;
        isAutoMoving = false; 
        targetEnemy = null; 

        // Tạm thời bỏ hoạt ảnh lướt để tránh báo lỗi
        
        float facingDirection = Mathf.Sign(transform.localScale.x);
        // Do quay mặt bị đảo ngược với SPUM, nên dashDir cũng phải đảo theo
        Vector3 dashDir = new Vector3(-facingDirection, 0, 0); 
        float startTime = Time.time;

        while (Time.time < startTime + dashDuration)
        {
            transform.position += dashDir * dashSpeed * Time.deltaTime;
            yield return null;
        }

        isDashing = false;
        nextDashTime = Time.time + dashCooldown;
    }
}