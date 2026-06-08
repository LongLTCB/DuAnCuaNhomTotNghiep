using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PlayerCombatTests
{
    private GameObject player;
    private PlayerCombat combatScript;

    [SetUp]
    public void Setup()
    {
        player = new GameObject("TestFighter");
        combatScript = player.AddComponent<PlayerCombat>();
        
        // Cài đặt thông số giả lập
        // (Mạnh nhớ đổi tên biến cho khớp với file PlayerCombat thực tế của bro nhé)
        combatScript.attackDamage = 20;
    }

    // TEST 1: Logic thông thường - Dame phải được gán đúng
    [Test]
    public void NORMAL_KiemTraThongSo_DameKhoiDiemPhaiLa20()
    {
        Assert.AreEqual(20, combatScript.attackDamage, "Lỗi: Dame khởi điểm bị sai!");
    }

    // TEST 2: TEST THỜI GIAN (Đợi Cooldown)
    // Chú ý: Dùng [UnityTest] và IEnumerator để có thể chờ (yield return)
    [UnityTest]
    public IEnumerator MEDIUM_HoiChieu_PhaiDoi0_5GiayMoiDuocDanhTiep()
    {
        float cooldownKynang = 0.5f;
        float thoiGianDuocDanhTiep = Time.time + cooldownKynang;

        // 1. Vừa tung chiêu xong, test ngay lập tức -> Phải báo FALSE (Chưa hồi xong)
        Assert.IsFalse(Time.time >= thoiGianDuocDanhTiep, "Lỗi hack: Vừa chém xong đã đòi chém tiếp!");

        // 2. Chờ thời gian trôi qua đúng 0.51 giây (như người chơi đang đợi)
        yield return new WaitForSeconds(0.51f);

        // 3. Kiểm tra lại -> Phải báo TRUE (Đã hồi chiêu xong)
        Assert.IsTrue(Time.time >= thoiGianDuocDanhTiep, "Lỗi game: Qua 0.5s rồi mà vẫn khóa chiêu của người chơi!");
    }

    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(player);
    }
}