using UnityEngine;
using Photon.Pun;
using System.Collections;

public class EnemyAI : MonoBehaviourPun
{
    [Header("Phạm vi Di chuyển (A đến B)")]
    public float patrolDistance = 3f; // Khoảng cách đi qua lại tính từ điểm mọc ra
    public float moveSpeed = 1.5f;
    private float leftLimit, rightLimit;
    private bool movingRight = true;
    private bool isWaiting = false;   // Đang đứng nghỉ 1s

    [Header("Phạm vi Phát hiện & Tấn công")]
    public float detectionRange = 4f; 
    public float attackRate = 1.5f; 
    public Transform firePoint;     
    private float nextAttackTime;

    // Thái độ của quái
    private bool isPlayerInZone = false;
    private bool isAggressive = false; // Bị nổi điên (10% tỉ lệ)

    private bool isFacingRight = true; // Ban đầu mặt phải, nếu cần sẽ lật lại
    private Animator animator;
    private EnemyHealth healthScript;
    private bool isStunned = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        healthScript = GetComponent<EnemyHealth>();

        // Tính toán tọa độ Điểm A (Trái) và Điểm B (Phải) dựa vào lúc mới sinh ra
        leftLimit = transform.position.x - patrolDistance;
        rightLimit = transform.position.x + patrolDistance;
    }

    void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (healthScript != null && healthScript.GetCurrentHealth() <= 0) return;
        if (isStunned) return;

        Transform targetPlayer = FindNearestPlayer();
        float distToPlayer = targetPlayer != null ? Vector2.Distance(transform.position, targetPlayer.position) : Mathf.Infinity;

        // --- KHI NGƯỜI CHƠI BƯỚC VÀO VÙNG PHÁT HIỆN ---
        if (distToPlayer <= detectionRange)
        {
            // Khoảnh khắc VỪA BƯỚC VÀO (Lọc tỉ lệ 10%)
            if (!isPlayerInZone)
            {
                isPlayerInZone = true;
                int roll = Random.Range(0, 100); // Đổ xúc xắc từ 0 đến 99
                isAggressive = (roll < 10);      // Dưới 10 -> Tỉ lệ 10% nổi điên
            }

            // Dừng bước và quay mặt lườm người chơi
            if (animator != null) animator.SetBool("isWalking", false);
            FaceTarget(targetPlayer.position);

            // Nếu nó thuộc nhóm 10% nổi điên -> Nhả đạn!
            if (isAggressive)
            {
                Attack(targetPlayer.position);
            }
        }
        // --- KHI KHÔNG CÓ AI TRONG VÙNG HOẶC ĐÃ BỎ ĐI ---
        else
        {
            // Xóa bỏ trạng thái thù hằn nếu người chơi đã đi khuất
            if (isPlayerInZone)
            {
                isPlayerInZone = false;
                isAggressive = false;
            }

            Patrol(); // Tiếp tục đi tuần tra A -> B
        }
    }

    void Patrol()
    {
        if (isWaiting) return; // Đang nghỉ ngơi thì không đi

        float targetX = movingRight ? rightLimit : leftLimit;
        Vector2 targetPos = new Vector2(targetX, transform.position.y);

        // --- ĐÂY LÀ CHÌA KHÓA CHỮA BỆNH MOONWALK ---
        // Bắt buộc: Đang đi sang phải mà mặt đang quay trái -> Lật mặt!
        if (movingRight && !isFacingRight) Flip();
        // Đang đi sang trái mà mặt đang quay phải -> Lật mặt!
        else if (!movingRight && isFacingRight) Flip();
        // ------------------------------------------

        // Di chuyển về điểm đích
        transform.position = EnemyMovementUtility.MoveTowardsWithoutPassingThroughWalls(transform.position, targetPos, moveSpeed, Time.deltaTime);
        if (animator != null) animator.SetBool("isWalking", true);

        // Nếu đã đi đến nơi (Điểm A hoặc B)
        if (Mathf.Abs(transform.position.x - targetX) < 0.1f)
        {
            StartCoroutine(WaitAtPointRoutine());
        }
    }

    IEnumerator WaitAtPointRoutine()
    {
        isWaiting = true;
        if (animator != null) animator.SetBool("isWalking", false);
        
        yield return new WaitForSeconds(1f); // ĐỨNG YÊN 1 GIÂY
        
        movingRight = !movingRight; // Chỉ đổi hướng đi
        // LƯU Ý: Tôi đã xóa lệnh Flip() ở đây đi, vì hàm Patrol ở trên sẽ tự động lo việc quay mặt rồi!
        
        isWaiting = false;
    }

    Transform FindNearestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        Transform nearest = null;
        float minDistance = Mathf.Infinity;

        foreach (GameObject p in players)
        {
            float dist = Vector2.Distance(transform.position, p.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                nearest = p.transform;
            }
        }
        return nearest;
    }

    void Attack(Vector3 targetPos)
    {
        if (Time.time >= nextAttackTime)
        {
            photonView.RPC("SyncRangedAttack", RpcTarget.All);

            Vector2 direction = targetPos - firePoint.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0, 0, angle);

            PhotonNetwork.InstantiateRoomObject("EnemyBullet", firePoint.position, rotation);
            nextAttackTime = Time.time + 1f / attackRate;
        }
    }

    [PunRPC]
    void SyncRangedAttack()
    {
        if (animator != null) animator.SetTrigger("attack1");
    }

    public void TriggerHurtStun(float stunDuration)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        isAggressive = true;
        StartCoroutine(StunRoutine(stunDuration));
    }

    IEnumerator StunRoutine(float duration)
    {
        isStunned = true;
        if (animator != null) animator.SetBool("isWalking", false);
        yield return new WaitForSeconds(duration);
        isStunned = false;
    }

    void FaceTarget(Vector3 targetPos)
    {
        if (targetPos.x > transform.position.x && !isFacingRight) Flip();
        else if (targetPos.x < transform.position.x && isFacingRight) Flip();
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
    }

    // Vẽ ranh giới để bạn dễ hình dung
    void OnDrawGizmosSelected()
    {
        // Vòng màu vàng: Tầm phát hiện/Lườm
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Đường thẳng xanh lá: Quãng đường A -> B khi sinh ra
        Gizmos.color = Color.green;
        Vector3 start = transform.position + new Vector3(-patrolDistance, 0, 0);
        Vector3 end = transform.position + new Vector3(patrolDistance, 0, 0);
        Gizmos.DrawLine(start, end);
    }
}