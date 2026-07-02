using UnityEngine;
using Photon.Pun;
using System.Collections; // Nhớ dòng này để dùng Coroutine lùi lại

public class CloneAI : MonoBehaviourPun
{
    public float speed = 5f;
    
    [Header("Giữ cự ly (Độ giật)")]
    public float idealDistance = 2f;  // Khoảng cách chém đẹp nhất
    public float recoilSpeed = 10f;     // Tốc độ nảy lùi

    public int cloneDamage;

    private Transform owner;
    private Vector3 followOffset;
    private Transform target;
    
    private bool isAttacking = false;
    private bool isRecoiling = false; // Biến khóa AI khi đang bị đẩy lùi

    private Animator animator;
    private float nextAttackTime;

    void Start()
    {
        animator = GetComponent<Animator>();
        if (photonView.IsMine)
        {
            Invoke("Die", 5f);
        }
    }

    public void Setup(Transform _owner, Vector3 _offset, Transform initialTarget)
    {
        owner = _owner;
        followOffset = _offset;
        if (initialTarget != null) SetTarget(initialTarget);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        isAttacking = true;
    }

    void Update()
    {
        if (!photonView.IsMine) return;
        
        // NẾU ĐANG BỊ ĐẨY LÙI THÌ KHÔNG SUY NGHĨ, KHÔNG CHẠY TỚI!
        if (isRecoiling) return; 

        if (!isAttacking)
        {
            // ---- TRẠNG THÁI 1: ĐI THEO CHỦ NHÂN ----
            if (owner != null)
            {
                Vector3 targetPos = owner.position + followOffset;
                float distanceToOwner = Vector2.Distance(transform.position, targetPos);
                
                if (distanceToOwner > 0.2f)
                {
                    transform.position = EnemyMovementUtility.MoveTowardsWithoutPassingThroughWalls(transform.position, targetPos, speed, Time.deltaTime);
                    animator.SetBool("isWalking", true);
                    FaceDirection(targetPos.x - transform.position.x); 
                }
                else
                {
                    animator.SetBool("isWalking", false);
                    FaceDirection(owner.localScale.x); 
                }
            }
        }
        else
        {
            // ---- TRẠNG THÁI 2: TẤN CÔNG QUÁI VẬT ----
            // ---- TRẠNG THÁI 2: TẤN CÔNG QUÁI VẬT ----
            if (target != null)
            {
                // 1. Tính toán 2 điểm đứng lý tưởng (Trái và Phải)
                Vector3 leftSpot = target.position + new Vector3(-idealDistance, 0, 0);
                Vector3 rightSpot = target.position + new Vector3(idealDistance, 0, 0);

                // 2. Tìm điểm gần nhất
                float distToLeft = Vector2.Distance(transform.position, leftSpot);
                float distToRight = Vector2.Distance(transform.position, rightSpot);
                Vector3 bestSpot = (distToLeft < distToRight) ? leftSpot : rightSpot;
                
                float distToSpot = Vector2.Distance(transform.position, bestSpot);

                // 3. Chạy tới điểm đứng đó
                if (distToSpot > 0.1f) 
                {
                    transform.position = EnemyMovementUtility.MoveTowardsWithoutPassingThroughWalls(transform.position, bestSpot, speed, Time.deltaTime);
                    animator.SetBool("isWalking", true);
                    FaceDirection(bestSpot.x - transform.position.x);
                }
                else 
                {
                    // 4. Đã vào vị trí -> Đứng lại múa kiếm!
                    animator.SetBool("isWalking", false);
                    FaceDirection(target.position.x - transform.position.x); // Nhìn thẳng mặt quái

                    if (Time.time >= nextAttackTime)
                    {
                        animator.SetTrigger("attack1");
                        PhotonView targetView = target.GetComponent<PhotonView>();
                        if (targetView != null) targetView.RPC("TakeDamage", RpcTarget.All, cloneDamage);
                        
                        // Tính toán lực nảy lùi nếu lỡ bị kẹp quá sát
                        float actualDistToEnemy = Vector2.Distance(transform.position, target.position);
                        if (actualDistToEnemy < idealDistance - 0.1f)
                        {
                            float pushBackAmount = idealDistance - actualDistToEnemy;
                            float facingDir = Mathf.Sign(transform.localScale.x);
                            Vector3 recoilDir = new Vector3(-facingDir, 0, 0);
                            StartCoroutine(RecoilRoutine(recoilDir, pushBackAmount));
                        }

                        nextAttackTime = Time.time + 0.5f;
                    }
                }
            }
            else
            {
                isAttacking = false;
        }   }
    }

    void FaceDirection(float directionX)
    {
        if (Mathf.Abs(directionX) < 0.01f) return; 
        Vector3 scale = transform.localScale;
        if (directionX > 0) scale.x = Mathf.Abs(scale.x);       
        else if (directionX < 0) scale.x = -Mathf.Abs(scale.x); 
        transform.localScale = scale;
    }

    // Hiệu ứng giật lùi (Khóa AI trong lúc trượt)
    IEnumerator RecoilRoutine(Vector3 direction, float distance)
    {
        isRecoiling = true; // Khóa AI
        float moved = 0f;
        while (moved < distance)
        {
            float step = recoilSpeed * Time.deltaTime;
            transform.position = EnemyMovementUtility.MoveTowardsWithoutPassingThroughWalls(transform.position, transform.position + direction, recoilSpeed, Time.deltaTime);
            moved += step;
            yield return null;
        }
        isRecoiling = false; // Trượt xong, mở khóa AI lại bình thường
    }

    void Die()
    {
        PhotonNetwork.Destroy(gameObject);
    }
}
