using UnityEngine;
using Photon.Pun;

public class ShopInteract : MonoBehaviour
{
    [Header("Chữ Nhắc Nhở Bấm Nút")]
    public GameObject interactText; // Kéo cái chữ "Bấm F..." vào đây

    private bool isPlayerNear = false;

    void Update()
    {
        // Nếu người chơi đứng gần và bấm phím F
        if (isPlayerNear && Input.GetKeyDown(KeyCode.F))
        {
            ShopManager.instance.OpenShop();
            interactText.SetActive(false); // Ẩn chữ đi cho đỡ vướng khi đang mở Shop
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Chỉ hiện chữ khi chính NHÂN VẬT CỦA MÌNH chạm vào
        if (other.CompareTag("Player"))
        {
            PhotonView pv = other.GetComponent<PhotonView>();
            if (pv != null && pv.IsMine)
            {
                isPlayerNear = true;
                if (interactText != null) interactText.SetActive(true);
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PhotonView pv = other.GetComponent<PhotonView>();
            if (pv != null && pv.IsMine)
            {
                isPlayerNear = false;
                if (interactText != null) interactText.SetActive(false);
                ShopManager.instance.CloseShop(); // Tự động đóng Shop nếu bỏ đi xa
            }
        }
    }
}