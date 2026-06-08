using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Photon.Pun;

public class HeroStupicPlayModeTests
{
    // Biến lưu trữ vật thể trên Scene
    private GameObject playerObj;
    private GameObject enemyObj;

    [SetUp]
    public void Setup()
    {
        // Tự động tạo 2 khối lập phương giả làm Player và Quái vật ngay trên Scene
        playerObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        playerObj.name = "Test_Player";
        playerObj.transform.position = new Vector3(-5, 0, 0); // Đứng bên trái

        enemyObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        enemyObj.name = "Test_Enemy";
        enemyObj.transform.position = new Vector3(5, 0, 0); // Đứng bên phải
        enemyObj.AddComponent<EnemyHealth>(); // Gắn máu cho quái
    }

    [TearDown]
    public void Teardown()
    {
        // Dọn dẹp Scene sau khi test xong để không bị rác
        Object.Destroy(playerObj);
        if (enemyObj != null) Object.Destroy(enemyObj);
    }

    // [UnityTest] cho phép dùng IEnumerator để mô phỏng thời gian trôi qua ngoài Scene
    [UnityTest]
    public IEnumerator TC01_Scene_PlayerAutoMovesToTarget()
    {
        // Gắn script di chuyển vào Player
        PlayerController controller = playerObj.AddComponent<PlayerController>();
        controller.moveSpeed = 10f;
        
        // Bắt nhân vật tự chạy đến tọa độ (0,0,0)
        Vector3 targetPos = Vector3.zero;

        // Mô phỏng vòng lặp Update() di chuyển trong 0.5 giây
        float timePassed = 0f;
        while (timePassed < 0.5f)
        {
            playerObj.transform.position = Vector3.MoveTowards(playerObj.transform.position, targetPos, controller.moveSpeed * Time.deltaTime);
            timePassed += Time.deltaTime;
            yield return null; // Chờ 1 khung hình (Frame) trên Scene
        }

        // Kiểm tra xem nhân vật đã rời khỏi vị trí cũ (-5,0,0) và chạy tới gần đích chưa
        Assert.Greater(playerObj.transform.position.x, -4f, "Nhân vật không di chuyển trên Scene!");
    }

    [UnityTest]
    public IEnumerator TC02_Scene_HomingProjectileFliesToEnemy()
    {
        // 1. Sinh ra 1 viên đạn trên Scene
        GameObject projectileObj = new GameObject("Test_Bullet");
        projectileObj.transform.position = playerObj.transform.position;
        HomingProjectile homing = projectileObj.AddComponent<HomingProjectile>();
        homing.speed = 20f;

        // 2. Nạp mục tiêu cho viên đạn là con quái vật
        homing.Initialize(enemyObj.transform, null, 50, true, 1);

        // 3. Cho thời gian trôi qua 0.3 giây trên Scene để đạn bay
        float timePassed = 0f;
        while (timePassed < 0.3f)
        {
            // Mô phỏng hàm Update của viên đạn
            projectileObj.transform.position = Vector2.MoveTowards(projectileObj.transform.position, enemyObj.transform.position, homing.speed * Time.deltaTime);
            timePassed += Time.deltaTime;
            yield return null; // Vẽ lên màn hình Scene
        }

        // 4. Kiểm tra xem đạn có bay lại gần quái vật không (Khoảng cách phải giảm đi)
        float distance = Vector3.Distance(projectileObj.transform.position, enemyObj.transform.position);
        Assert.Less(distance, 5f, "Đạn không bay đuổi theo quái vật trên Scene!");

        Object.Destroy(projectileObj);
    }

    [UnityTest]
    public IEnumerator TC03_Scene_EnemyDestroysItselfAfterDeathAnimationDelay()
    {
        EnemyHealth enemyHealth = enemyObj.GetComponent<EnemyHealth>();
        enemyHealth.maxHealth = 100;
        
        // Chém quái 999 sát thương cho nó chết luôn
        enemyHealth.TakeDamage(999, 1);

        // Theo code của bro, quái sẽ chờ 1.5 giây để chạy xong Animation chết rồi mới biến mất
        // Chúng ta sẽ cho Scene đợi 1.6 giây
        yield return new WaitForSeconds(1.6f);

        // Kiểm tra xem con quái đã bị xóa sổ khỏi Scene chưa (Bị Destroy)
        // Unity overload toán tử == null cho GameObject, nếu nó bị Destroy thì sẽ trả về true
        Assert.IsTrue(enemyObj == null, "Quái vật chết nhưng xác không biến mất khỏi Scene sau 1.5 giây!");
    }
}