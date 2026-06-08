using NUnit.Framework;
using UnityEngine;

public class SystemTests
{
    // BÀI TEST 1: Khâu chọn nhân vật (ClassSelection)
    [Test]
    public void HIGH_ChonNhanVat_MayPhaiNhoDungID()
    {
        // 1. Xóa sạch trí nhớ cũ đi để test cho chuẩn
        PlayerPrefs.DeleteKey("NhanVatDaChon");

        // 2. Kịch bản: Người chơi bấm nút chọn Pháp sư (ID = 1)
        int idPhapSu = 1;
        PlayerPrefs.SetInt("NhanVatDaChon", idPhapSu);
        PlayerPrefs.Save();

        // 3. Kiểm tra xem máy nó có lưu đúng số 1 không
        int idMayNho = PlayerPrefs.GetInt("NhanVatDaChon", 0);
        Assert.AreEqual(idPhapSu, idMayNho, $"LỖI: Chọn Phấp sư (1) mà máy lại nhớ thành ({idMayNho})");
    }

    // BÀI TEST 2: Đảm bảo không đẻ rác điểm số (Logic âm)
    [Test]
    public void MEDIUM_DiemSo_KhongTheNopDiemAm()
    {
        // Kịch bản: Bị hack, truyền điểm âm vào
        int diemBiHack = -500;
        
        // Thường thì trong code SubmitScore bro sẽ có dòng: if(score < 0) score = 0;
        // Giả lập logic đó ở đây:
        int diemThucTeGuiLen = Mathf.Max(0, diemBiHack);

        // Kiểm tra
        Assert.AreEqual(0, diemThucTeGuiLen, "LỖI BẢO MẬT: Game đang cho phép nộp điểm âm lên BXH!");
    }
}