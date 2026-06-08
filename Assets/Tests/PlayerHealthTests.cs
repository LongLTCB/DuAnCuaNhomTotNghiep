using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PlayerHealthTests
{
    private GameObject testPlayer;
    private PlayerHealth healthScript;

    // BƯỚC CHUẨN BỊ: Chạy trước khi bắt đầu test
    [SetUp]
    public void Setup()
    {
        // 1. Sinh ra một cái xác không hồn để test
        testPlayer = new GameObject("TestAatrox");
        
        // 2. Gắn bộ lòng (Script máu) vào
        healthScript = testPlayer.AddComponent<PlayerHealth>();
        healthScript.maxHealth = 100;
        healthScript.currentHealth = 100;
    }
    
    // BÀI TEST 1: Ăn sát thương chí mạng phải ngỏm
    [Test]
    public void CRITICAL_Nhan150Dam_MauPhaiVeKhongVaMatTag()
    {
        // HÀNH ĐỘNG: Chém 150 máu
        healthScript.TakeDamage(150);

        // KIỂM TRA (Assert): Trọng tài vào cuộc
        Assert.IsTrue(healthScript.currentHealth <= 0, "LỖI: Trừ 150 máu mà nó chưa chịu chết!");
    }

    // BƯỚC DỌN DẸP: Chạy sau khi test xong để không rác bộ nhớ
    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(testPlayer);
    }
}