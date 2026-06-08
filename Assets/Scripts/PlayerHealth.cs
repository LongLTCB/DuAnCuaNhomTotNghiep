using UnityEngine;
using UnityEngine.UI; 
using Photon.Pun;
using System.Collections;

public class PlayerHealth : MonoBehaviourPun
{
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Máu trên đầu (Cho mọi người xem)")]
    public Image healthFill; 
    
    // Biến này KHÔNG CẦN KÉO THẢ NỮA, code sẽ tự tìm
    private Slider healthSlider; 

    private Animator animator;
    private PlayerController controller;
    private PlayerCombat combat;

    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponentInChildren<Animator>();
        controller = GetComponent<PlayerController>();
        combat = GetComponent<PlayerCombat>();

        // -------------------------------------------------------------
        // RADAR TÌM KIẾM UI (CHỈ DÀNH CHO NHÂN VẬT CHÍNH CHỦ)
        // -------------------------------------------------------------
        if (photonView.IsMine)
        {
            // Tự động quét toàn bộ màn hình để tìm cái cục tên là "MainHealthSlider"
            GameObject uiMaus = GameObject.Find("MainHealthSlider");
            
            if (uiMaus != null)
            {
                healthSlider = uiMaus.GetComponent<Slider>();
                healthSlider.maxValue = maxHealth;
                healthSlider.value = currentHealth;
            }
            else
            {
                Debug.LogWarning("Không tìm thấy Slider nào tên là 'MainHealthSlider' trên màn hình!");
            }
        }

        UpdateHealthUI(); 
    }

    public void NhanSatThuong(int damage)
    {
        if (photonView != null)
        {
            photonView.RPC("TakeDamage", RpcTarget.All, damage);
        }
    }

    [PunRPC]
    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return; 

        currentHealth -= damage;
        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            if (animator != null) animator.SetTrigger("hurtTrigger");
        }
    }

    void UpdateHealthUI()
    {
        // 1. Cập nhật thanh máu nhỏ trên đầu
        if (healthFill != null)
        {
            healthFill.fillAmount = (float)currentHealth / maxHealth;
        }

        // 2. SỬA CHỖ NÀY: Kiểm tra xem có PhotonView không rồi mới check IsMine
        if (photonView != null && photonView.IsMine && healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }
    }

    void Die()
    {
        Debug.Log("Nhân vật đã nằm xuống!");
        
        if (animator != null)
        {
            animator.SetTrigger("dieTrigger");       
            animator.SetBool("isDead", true);    
        }

        if (controller != null) controller.enabled = false;
        if (combat != null) combat.enabled = false;

        gameObject.tag = "Untagged"; 
        Collider2D coll = GetComponent<Collider2D>();
        if (coll != null) coll.enabled = false; 

        if (photonView.IsMine)
        {
            GameManager.instance.ShowDeathMenu();
        }
    }

    public int GetCurrentHealth() { return currentHealth; }
    public int GetMaxHealth() { return maxHealth; }
}