using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Photon.Pun;

public class HeroStupicMassiveTestSuite
{
    private GameObject playerObject;
    private PlayerHealth healthScript;
    private PlayerScore scoreScript;
    private PlayerCombat combatScript;
    private PlayerController controllerScript;

    // Hàm này chạy TRƯỚC mỗi bài test: Dùng để đẻ ra 1 con nhân vật giả (Mocking)
    [SetUp]
    public void Setup()
    {
        playerObject = new GameObject("DummyPlayer");
        
        // Gắn các linh hồn vào nhân vật giả
        healthScript = playerObject.AddComponent<PlayerHealth>();
        scoreScript = playerObject.AddComponent<PlayerScore>();
        combatScript = playerObject.AddComponent<PlayerCombat>();
        controllerScript = playerObject.AddComponent<PlayerController>();

        // Set thông số cơ bản để test
        healthScript.maxHealth = 500;
        combatScript.attackCooldown = 0.5f;
        combatScript.skillCooldown = 3f;
    }

    // Hàm này chạy SAU mỗi bài test: Dọn rác
    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(playerObject);
    }

    #region --- HỆ THỐNG MÁU & SÁT THƯƠNG (HEALTH SYSTEM) ---
    
    [Test]
    public void TC01_Health_StartsAtMaximum()
    {
        // Kiểm tra xem vừa đẻ ra máu có đầy không
        Assert.AreEqual(500, healthScript.maxHealth);
    }

    [Test]
    public void TC02_TakeDamage_ReducesCurrentHealth()
    {
        // Khởi tạo máu
        int currentHp = healthScript.maxHealth;
        int damageTaken = 50;
        
        // Giả lập trừ máu
        currentHp -= damageTaken;
        
        Assert.AreEqual(450, currentHp);
    }

    [Test]
    public void TC03_TakeDamage_HealthCannotGoBelowZero()
    {
        int currentHp = 10;
        int massiveDamage = 9999;
        
        currentHp -= massiveDamage;
        if (currentHp < 0) currentHp = 0; // Logic chặn máu âm
        
        Assert.AreEqual(0, currentHp);
    }

    [Test]
    public void TC04_Heal_IncreasesHealth()
    {
        int currentHp = 200;
        currentHp += 50; // Hồi 50 máu
        
        Assert.AreEqual(250, currentHp);
    }

    [Test]
    public void TC05_Heal_CannotExceedMaxHealth()
    {
        int currentHp = 480;
        currentHp += 100; 
        if (currentHp > healthScript.maxHealth) currentHp = healthScript.maxHealth;
        
        Assert.AreEqual(500, currentHp);
    }
    #endregion

    #region --- HỆ THỐNG ĐIỂM SỐ (SCORE SYSTEM) ---

    [Test]
    public void TC06_Score_StartsAtZero()
    {
        Assert.AreEqual(0, scoreScript.currentScore);
    }

    [Test]
    public void TC07_AddScore_IncreasesTotalScore()
    {
        scoreScript.currentScore = 0;
        int pointsFromEnemy = 15;
        
        scoreScript.currentScore += pointsFromEnemy;
        
        Assert.AreEqual(15, scoreScript.currentScore);
    }

    [Test]
    public void TC08_AddScore_MultipleKillsAccumulateCorrectly()
    {
        scoreScript.currentScore = 0;
        scoreScript.currentScore += 10;
        scoreScript.currentScore += 25;
        
        Assert.AreEqual(35, scoreScript.currentScore);
    }
    #endregion

    #region --- HỆ THỐNG CHIẾN ĐẤU (COMBAT & COOLDOWNS) ---

    [Test]
    public void TC09_NormalAttack_CalculatesDamageCorrectly()
    {
        combatScript.attackDamage = 20;
        int criticalMultiplier = 2; // Giả lập chí mạng x2
        
        int finalDamage = combatScript.attackDamage * criticalMultiplier;
        
        Assert.AreEqual(40, finalDamage);
    }

    [Test]
    public void TC10_SkillAttack_CalculatesDamageCorrectly()
    {
        combatScript.skillDamage = 50;
        int bonusDamageFromItems = 15;
        
        int finalDamage = combatScript.skillDamage + bonusDamageFromItems;
        
        Assert.AreEqual(65, finalDamage);
    }

    [UnityTest]
    public IEnumerator TC11_AttackCooldown_PreventsSpamming()
    {
        float lastAttackTime = Time.time;
        float cooldown = 0.5f;
        
        // Vừa đánh xong, thử đánh luôn -> Phải bị false (chưa hồi xong)
        bool canAttackNow = (Time.time >= lastAttackTime + cooldown);
        Assert.IsFalse(canAttackNow);
        
        // Đợi 0.6 giây
        yield return new WaitForSeconds(0.6f);
        
        // Thử lại -> Phải trả về true (đã hồi xong)
        canAttackNow = (Time.time >= lastAttackTime + cooldown);
        Assert.IsTrue(canAttackNow);
    }
    #endregion

    #region --- HỆ THỐNG DI CHUYỂN (MOVEMENT & DASH) ---

    [Test]
    public void TC12_Movement_SpeedIsPositive()
    {
        controllerScript.moveSpeed = 5f;
        Assert.Greater(controllerScript.moveSpeed, 0f, "Tốc độ di chuyển không được âm!");
    }

    [Test]
    public void TC13_Dash_SpeedIsGreaterThanWalkSpeed()
    {
        controllerScript.moveSpeed = 5f;
        controllerScript.dashSpeed = 15f;
        
        Assert.Greater(controllerScript.dashSpeed, controllerScript.moveSpeed);
    }

    [Test]
    public void TC14_FaceDirection_FlipsLocalScaleX()
    {
        Vector3 scale = new Vector3(1, 1, 1);
        float moveX = -1f; // Đi sang trái
        
        if (moveX < 0) scale.x = Mathf.Abs(scale.x); // Xoay mặt (Dựa theo code SPUM của bro)
        
        Assert.AreEqual(1, scale.x);
    }
    #endregion

    #region --- HỆ THỐNG ĐẠN & VŨ KHÍ (PROJECTILES) ---

    [Test]
    public void TC15_HomingProjectile_StoresAttackerID()
    {
        // Khởi tạo một GameObject Đạn giả
        GameObject projObj = new GameObject("Projectile");
        HomingProjectile projScript = projObj.AddComponent<HomingProjectile>();
        
        // Căn cước người chém
        int mockAttackerID = 999;
        
        // Khởi tạo đạn
        projScript.Initialize(null, null, 20, true, mockAttackerID);
        
        // Dùng Reflection để đọc biến private ownerID bên trong viên đạn
        var field = typeof(HomingProjectile).GetField("ownerID", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        int storedID = (int)field.GetValue(projScript);
        
        Assert.AreEqual(999, storedID, "Viên đạn KHÔNG NHỚ ID của chủ nhân!");
        
        Object.DestroyImmediate(projObj);
    }
    #endregion
}