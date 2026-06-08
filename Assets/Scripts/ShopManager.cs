using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    public static ShopManager instance;
    
    [Header("Giao diện Shop")]
    public GameObject shopPanel;

    void Awake() { instance = this; }

    public void OpenShop()
    {
        shopPanel.SetActive(true);
    }

    public void CloseShop()
    {
        shopPanel.SetActive(false);
    }

    public void BuyHealth()
    {
        int cost = 50;
        if (CurrencyManager.instance.currentGold >= cost)
        {
            CurrencyManager.instance.AddGold(-cost); // Trừ tiền
            Debug.Log("<color=green>Đã mua Máu thành công!</color>");
            // (Bài sau chúng ta sẽ móc nối code để bơm đầy thanh máu thực tế)
        }
        else
        {
            Debug.Log("<color=red>Không đủ tiền mua Máu!</color>");
        }
    }

    public void BuyDamage()
    {
        int cost = 100;
        if (CurrencyManager.instance.currentGold >= cost)
        {
            CurrencyManager.instance.AddGold(-cost);
            Debug.Log("<color=yellow>Đã nâng cấp Sát thương!</color>");
            // (Bài sau sẽ nối code tăng dame chém)
        }
        else
        {
            Debug.Log("<color=red>Không đủ tiền nâng cấp!</color>");
        }
    }
}