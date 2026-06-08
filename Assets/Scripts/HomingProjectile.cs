using UnityEngine;
using Photon.Pun;

public class HomingProjectile : MonoBehaviour
{
    [Header("Hiệu ứng khi trúng mục tiêu")]
    public GameObject hitEffectPrefab;

    [Header("Cấu hình bay")]
    public float speed = 15f; // Tốc độ bay
    public string hitEffectName = "HitExplosion"; // Hiệu ứng nổ lúc va chạm
    
    // --- Ô SỬA LỖI BAY NGƯỢC ---
    [Header("Góc Xoay Bổ Sung")]
    [Tooltip("Điều chỉnh góc này nếu hiệu ứng bay ngược. Thử nhập: 0, 90, -90, hoặc 180")]
    public float rotationOffset = 0f; 
    // ----------------------------

    private Transform target;
    private PhotonView targetPV;
    private int damage;
    private bool isMine;
    
    // 1. ĐÃ THÊM: Biến lưu trữ ID của người bắn ra viên đạn này
    private int ownerID; 

    // 2. ĐÃ SỬA: Hàm Initialize nhận thêm Căn cước công dân (attackerID) từ người chém
    public void Initialize(Transform targetTransform, PhotonView pv, int dmg, bool mine, int attackerID)
    {
        target = targetTransform;
        targetPV = pv;
        damage = dmg;
        isMine = mine;
        
        // Ghi nhớ ID này để lát nữa đòi nợ
        ownerID = attackerID; 
    }

    void Update()
    {
        // Nếu quái chết trước khi kiếm bay tới thì hủy kiếm luôn
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        // 1. Di chuyển tên lửa đuổi theo quái
        transform.position = Vector2.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        // 2. --- XỬ LÝ XOAY ĐẦU XOAY MŨI ---
        Vector2 direction = target.position - transform.position; // Hướng bay
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg; // Góc bay gốc

        // Lệnh thần thánh: Góc bay + Góc la bàn bổ sung để sửa lỗi Asset vẽ sai hướng
        transform.rotation = Quaternion.Euler(0, 0, angle + rotationOffset);

        // 3. Kiểm tra nếu bay tới nơi (Khoảng cách < 0.2)
        if (Vector2.Distance(transform.position, target.position) < 0.2f)
        {
            Explode();
        }
    }

    void Explode()
    {
        // Sinh hiệu ứng BÙM trên người quái
        GameObject effect = Instantiate(Resources.Load<GameObject>(hitEffectName), transform.position, Quaternion.identity);
        effect.transform.SetParent(target); // Nổ dính trên người quái

        // Trừ máu quái (Chỉ máy của người ra đòn mới được phép trừ, tránh trừ đúp)
        if (isMine && targetPV != null)
        {
            // 3. ĐÃ SỬA: Báo cáo với con quái là "Tao (ownerID) vừa chém mày đấy!"
            targetPV.RPC("TakeDamage", RpcTarget.All, damage, ownerID);
        }

        // Tự hủy cái kiếm đang bay
        Destroy(gameObject);
    }
}