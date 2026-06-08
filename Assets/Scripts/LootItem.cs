using UnityEngine;
using Photon.Pun;

public class LootItem : MonoBehaviour
{
    public int value = 10; // Giá trị đồng tiền

    void OnTriggerEnter2D(Collider2D other)
    {
        // Nếu nhân vật chính chạm vào
        if (other.CompareTag("Player"))
        {
            PhotonView pv = other.GetComponent<PhotonView>();
            if (pv != null && pv.IsMine)
            {
                CurrencyManager.instance.AddGold(value);
                // Sau khi nhặt thì xóa đồng tiền trên mạng
                if (PhotonNetwork.IsMasterClient) PhotonNetwork.Destroy(gameObject);
                else gameObject.SetActive(false); // Máy khách thì ẩn đi chờ Master xóa
            }
        }
    }
}