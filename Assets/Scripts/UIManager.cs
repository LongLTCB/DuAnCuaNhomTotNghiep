using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance; // Singleton để gọi từ mọi nơi

    [Header("Giao diện của tôi")]
    public Slider myHealthBar;

    [Header("Giao diện Mục tiêu")]
    public GameObject targetPanel;
    public Text targetNameText;
    public Slider targetHealthBar;

    void Awake()
    {
        instance = this;
        targetPanel.SetActive(false); // Ẩn mục tiêu lúc mới vào game
    }

    // Hàm cập nhật máu góc trái
    public void UpdateMyHealth(int current, int max)
    {
        myHealthBar.maxValue = max;
        myHealthBar.value = current;
    }

    // Hàm bật thanh máu ở giữa khi click trúng
    public void ShowTarget(string targetName, int currentHealth, int maxHealth)
    {
        targetPanel.SetActive(true);
        targetNameText.text = targetName;
        targetHealthBar.maxValue = maxHealth;
        targetHealthBar.value = currentHealth;
    }

    // Hàm tắt thanh máu ở giữa khi click ra ngoài
    public void HideTarget()
    {
        targetPanel.SetActive(false);
    }

    // Hàm này dùng để chạy tụt máu ở giữa màn hình
    public void UpdateTargetHealth(int current)
    {
        targetHealthBar.value = current;
    }
}