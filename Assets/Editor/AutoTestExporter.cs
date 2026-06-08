using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;
using System.IO;
using System.Text;

// Gắn cái mác này để Unity tự động khởi động máy in khi bật project
[InitializeOnLoad]
public class AutoTestExporter : ICallbacks
{
    static AutoTestExporter()
    {
        // Đăng ký máy in với hệ thống Test Runner
        var api = ScriptableObject.CreateInstance<TestRunnerApi>();
        api.RegisterCallbacks(new AutoTestExporter());
    }

    private string filePath = "BaoCaoTest_HeroStupic.csv"; // Tên file xuất ra
    private StringBuilder csvContent;

    public void RunStarted(ITestAdaptor testsToRun)
    {
        csvContent = new StringBuilder();
        // Tạo dòng tiêu đề cho Excel
        csvContent.AppendLine("Tên Bài Test,Kết Quả,Phân Loại,Ghi Chú Lỗi");
        Debug.Log("<color=cyan>Đang chạy Test Tự Động...</color>");
    }

    public void TestFinished(ITestResultAdaptor result)
    {
        // Chỉ ghi nhận các hàm test (bỏ qua các thư mục tổng)
        if (!result.Test.IsSuite)
        {
            // Cắt tên bài test (VD: CRITICAL_Nhan150Dam -> CRITICAL)
            string priority = "NORMAL";
            if (result.Test.Name.Contains("_")) 
            {
                priority = result.Test.Name.Split('_')[0];
            }

            string status = result.TestStatus.ToString();
            string msg = result.Message != null ? result.Message.Replace(",", " ") : "Pass Mượt Mà";

            // Thêm 1 dòng vào Excel
            csvContent.AppendLine($"{result.Test.Name},{status},{priority},{msg}");
        }
    }

    public void RunFinished(ITestResultAdaptor result)
    {
        // Lưu ra file khi chạy xong toàn bộ
        File.WriteAllText(filePath, csvContent.ToString(), Encoding.UTF8);
        Debug.Log("<color=green>Đã xuất báo cáo Excel (CSV) thành công! Mở thư mục gốc của game để xem.</color>");
    }

    public void TestStarted(ITestAdaptor test) { }
}