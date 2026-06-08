using UnityEngine;
using System.Collections;
using Photon.Pun;
using UnityEngine.UI;
using System;

public class EnemyHealth : MonoBehaviourPun
{
    [Header("Chỉ số cơ bản")]
    public int maxHealth = 500; 
    public int diemThuong = 10; // ĐÃ THÊM: Giết con này được 10 điểm
    private int currentHealth;
    private bool isDead = false; 

    [Header("Hiệu ứng trúng đòn")]
    private SpriteRenderer spriteRenderer;
    public Color hitColor = Color.red; 
    private Color originalColor;       
    public Slider healthSlider; 

    private Animator animator;

    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
        animator = GetComponent<Animator>(); 

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    // ĐÃ SỬA: Nhận thêm ID của người chém
    [PunRPC] 
    public void TakeDamage(int damage, int attackerViewID)
    {
        if (isDead) return;

        currentHealth -= damage;
        if (healthSlider != null) healthSlider.value = currentHealth;

        GameObject popupObj = Instantiate(Resources.Load("DamagePopupCanvas"), transform.position, Quaternion.identity) as GameObject;
        bool isCritical = damage > 30; 
        popupObj.GetComponent<DamagePopup>().Setup(damage, isCritical);

        StartCoroutine(FlashEffect());

        if (currentHealth > 0)
        {
            photonView.RPC("SyncHurtAnimation", RpcTarget.All);
            EnemyAI ai = GetComponent<EnemyAI>();
            if (ai != null) ai.TriggerHurtStun(0.4f);
        }
        else
        {
            isDead = true;
            if (healthSlider != null) healthSlider.gameObject.SetActive(false);

            // ĐÃ THÊM: TÌM CHỦ NHÂN ĐỂ TRẢ ĐIỂM
            PhotonView keTanCong = PhotonView.Find(attackerViewID);
            if (keTanCong != null && keTanCong.IsMine)
            {
                PlayerScore viDiem = keTanCong.GetComponent<PlayerScore>();
                if (viDiem != null) viDiem.CongDiem(diemThuong);
            }

            Die();
        }
    }

    [PunRPC]
    void SyncHurtAnimation()
    {
        if (animator != null) animator.SetTrigger("hurt"); 
    }

    public int GetCurrentHealth() { return currentHealth; }

    IEnumerator FlashEffect()
    {
        spriteRenderer.color = hitColor;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
    }

    void Die()
    {
        photonView.RPC("SyncDieAnimation", RpcTarget.All);
        StartCoroutine(DestroyAfterAnimation());
        PhotonNetwork.Instantiate("gold", transform.position, Quaternion.identity);
    }

    [PunRPC]
    void SyncDieAnimation()
    {
        if (animator != null) animator.SetTrigger("die");
        Collider2D coll = GetComponent<Collider2D>();
        if (coll != null) coll.enabled = false;
    }

    IEnumerator DestroyAfterAnimation()
    {
        yield return new WaitForSeconds(1.5f); 
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }

    internal int GetMaxHealth()
    {
        throw new NotImplementedException();
    }
}