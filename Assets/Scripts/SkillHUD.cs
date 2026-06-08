using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillHUD : MonoBehaviour
{
    public static SkillHUD instance;

    [Header("UI Dash (Space)")]
    public Image dashOverlay;
    public TextMeshProUGUI dashText;

    [Header("UI Skill (L)")]
    public Image skillOverlay;
    public TextMeshProUGUI skillText;

    [Header("UI Clone (H)")]
    public Image cloneOverlay;
    public TextMeshProUGUI cloneText;

    void Awake() { instance = this; }

    void Start()
    {
        // Lúc mới vào game thì hiện đủ chiêu (không có bóng mờ)
        dashOverlay.fillAmount = 0; dashText.text = "";
        skillOverlay.fillAmount = 0; skillText.text = "";
        cloneOverlay.fillAmount = 0; cloneText.text = "";
    }

    public void UpdateCooldown(string skillName, float currentTimer, float maxCooldown)
    {
        float fill = currentTimer / maxCooldown;
        string timeText = currentTimer > 0.1f ? Mathf.CeilToInt(currentTimer).ToString() : "";

        if (skillName == "Dash") { dashOverlay.fillAmount = fill; dashText.text = timeText; }
        if (skillName == "Skill") { skillOverlay.fillAmount = fill; skillText.text = timeText; }
        if (skillName == "Clone") { cloneOverlay.fillAmount = fill; cloneText.text = timeText; }
    }
}