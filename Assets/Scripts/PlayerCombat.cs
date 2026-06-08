using UnityEngine;
using Photon.Pun;
using System.Collections;

public class PlayerCombat : MonoBehaviourPun
{
    [Header("Âm thanh (Kéo thả vào đây)")]
    public AudioClip attackSound; // Tiếng chém thường
    public AudioClip skillSound;  // Tiếng tung Skill

    private AudioSource audioSource; // Cái loa gắn trên người

    [Header("Thông tin Class")]
    public int classType = 0; 

    [Header("Đánh Thường (Chuột Trái)")]
    public float attackRange = 7f;      
    public int attackDamage = 20;       
    public float attackCooldown = 0.5f; 
    public float attackDelay = 0.2f; 
    public string normalAttackPrefab = "FlyingSlash";

    [Header("Kỹ Năng (Chuột Phải)")]
    public int skillDamage = 50;
    public float skillCooldown = 3f;
    public string skillAttackPrefab = "BigExplosion"; 
    private float nextSkillTime = 0;

    private float nextAttackTime = 0;
    private Animator animator;

    public bool isBlocking { get; private set; } = false;

    [Header("Âm thanh Đánh Thường")]
    public AudioClip[] normalAttackSounds; // MẢNG: Cho phép kéo nhiều tiếng chém/phép khác nhau vào đây
    [Range(0.8f, 1.2f)]
    public float minPitch = 0.9f;
    [Range(0.8f, 1.2f)]
    public float maxPitch = 1.1f;

    // [Header("Âm thanh Kỹ Năng")]
    // public AudioClip skillSound;

    // private AudioSource audioSource;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        if (animator != null) animator.SetInteger("classType", classType);

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) 
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        // Nhớ kéo Spatial Blend của AudioSource trong Inspector lên 1 (3D) nhé!
    }

    void Update()
    {
        if (photonView != null && photonView.IsMine)
        if (Input.GetKey(KeyCode.K)) { if (!isBlocking) StartBlock(); }
        else if (isBlocking) { EndBlock(); }
    }

    // GỌI KHI ĐANG RƯỢT ĐUỔI (AUTO)
    public void AutoAttackTarget(Transform enemyTransform)
    {
        if (Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackCooldown;
            if (animator != null) animator.SetTrigger("attackTrigger"); 

            // ĐÃ THÊM: Phát tiếng đánh thường với Pitch ngẫu nhiên
            PlayRandomAttackSound();

            PhotonView targetPV = enemyTransform.GetComponent<PhotonView>();
            if (targetPV != null)
            {
                StartCoroutine(FireProjectileWithDelay(targetPV.ViewID, normalAttackPrefab, attackDamage));
            }
        }
    }

    // GỌI KHI BẤM CHUỘT PHẢI (MANUAL)
    public void ManualSkillAttack(Transform enemyTransform)
    {
        if (Time.time >= nextSkillTime)
        {
            nextSkillTime = Time.time + skillCooldown;
            
            if (animator != null) animator.SetTrigger("skillTrigger"); 

            // ĐÃ THÊM: Phát tiếng Skill
            PlaySound(skillSound);

            PhotonView targetPV = enemyTransform.GetComponent<PhotonView>();
            if (targetPV != null)
            {
                StartCoroutine(FireProjectileWithDelay(targetPV.ViewID, skillAttackPrefab, skillDamage));
            }
        }
        else
        {
            Debug.Log("Skill đang hồi chiêu!");
        }
    }
    void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            // Dùng PlayOneShot để âm thanh có thể đè lên nhau (chém nhanh 2 nhát phát 2 tiếng)
            audioSource.PlayOneShot(clip); 
        }
    }

    IEnumerator FireProjectileWithDelay(int targetViewID, string prefabName, int damage)
    {
        yield return new WaitForSeconds(attackDelay);
        photonView.RPC("RPC_FireProjectile", RpcTarget.All, targetViewID, prefabName, damage);
    }

   [PunRPC]
    void RPC_FireProjectile(int targetViewID, string prefabName, int damageToDeal)
    {
        PhotonView targetPV = PhotonView.Find(targetViewID);
        if (targetPV != null)
        {
            Vector3 spawnPos = transform.position + new Vector3(0, 0.5f, 0);
            GameObject effectPrefab = Resources.Load<GameObject>(prefabName);
            if (effectPrefab == null) return; 

            GameObject proj = Instantiate(effectPrefab, spawnPos, Quaternion.identity);
            HomingProjectile homingCode = proj.GetComponent<HomingProjectile>();
            if (homingCode != null)
            {
                // SỬA DÒNG NÀY: Thêm photonView.ViewID vào cuối cùng
                homingCode.Initialize(targetPV.transform, targetPV, damageToDeal, photonView.IsMine, photonView.ViewID);
            }
        }
    }
    void StartBlock() { isBlocking = true; }
    void EndBlock() { isBlocking = false; }
    void PlayRandomAttackSound()
    {
        // Kiểm tra xem đã kéo âm thanh vào mảng chưa
        if (normalAttackSounds.Length > 0 && audioSource != null)
        {
            // 1. Chọn bừa 1 tiếng trong danh sách (nếu bro có nhiều tiếng)
            int randomIndex = Random.Range(0, normalAttackSounds.Length);
            AudioClip clipToPlay = normalAttackSounds[randomIndex];

            // 2. Chỉnh độ cao (Pitch) ngẫu nhiên để không bị nhàm chán
            audioSource.pitch = Random.Range(minPitch, maxPitch);

            // 3. Phát tiếng
            audioSource.PlayOneShot(clipToPlay);
        }
    }
}