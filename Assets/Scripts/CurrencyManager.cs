using UnityEngine;
using UnityEngine.UI;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager instance;
    public int currentGold = 0;
    public Text goldDisplay; // Kéo UI Text hiện vàng vào đây
    void Start()
{
    AddGold(100);
}

    void Awake() { instance = this; }

    public void AddGold(int amount)
    {
        currentGold += amount;
        if (goldDisplay != null) goldDisplay.text = "Vàng: " + currentGold.ToString();
        Debug.Log("Bạn nhận được " + amount + " vàng!");
    }
}