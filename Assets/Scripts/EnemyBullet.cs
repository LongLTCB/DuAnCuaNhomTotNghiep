using UnityEngine;
using Photon.Pun;
using System.Collections;

public class EnemyBullet : MonoBehaviourPun
{
    public float speed = 8f;
    public int damage = 10;

    private Animator animator;
    private Rigidbody2D rb;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        // Cho viên đạn bay về phía trước ngay khi sinh ra
        rb.linearVelocity = transform.right * speed;

        // Chủ phòng đếm ngược 3 giây để xóa đạn nếu không trúng ai
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(DestroyOverTime());
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Chỉ Chủ phòng tính toán va chạm để không bị trừ máu 2 lần
        if (!PhotonNetwork.IsMasterClient) return;

        // Trúng Player
        if (other.CompareTag("Player"))
        {
            // Trừ máu người chơi (Giả định Player có hàm RPC TakeDamage)
            other.GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.All, damage);
            
            // Kích nổ và xóa
            ExplodeAndDestroy();
        }
        // Trúng Mặt đất/Tường (Nếu bạn có Tag "Ground" hoặc "Wall")
        else if (other.CompareTag("Ground") || other.CompareTag("Wall"))
        {
            ExplodeAndDestroy();
        }
    }

    void ExplodeAndDestroy()
    {
        photonView.RPC("SyncExplosion", RpcTarget.All);
        
        // Đạn nổ xong mới xóa (chờ 0.2s để diễn hết hoạt ảnh nổ)
        StartCoroutine(DestroyAfterExplosion());
    }

    [PunRPC]
    void SyncExplosion()
    {
        // Dừng đạn lại không bay nữa
        if (rb != null) rb.linearVelocity = Vector2.zero;
        
        // Phát hoạt ảnh nổ
        if (animator != null) animator.SetTrigger("explode");
        
        // Tắt va chạm để không vướng người chơi
        Collider2D coll = GetComponent<Collider2D>();
        if (coll != null) coll.enabled = false;
    }

    IEnumerator DestroyAfterExplosion()
    {
        yield return new WaitForSeconds(0.2f); // Chỉnh số này cho khớp độ dài ảnh nổ
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }

    IEnumerator DestroyOverTime()
    {
        yield return new WaitForSeconds(3f);
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
}