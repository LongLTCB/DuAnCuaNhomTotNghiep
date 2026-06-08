using UnityEngine;
using UnityEngine.UI;

public class DamagePopup : MonoBehaviour
{
    public float moveSpeed = 2f;    // Tốc độ bay lên
    public float destroyTime = 1f;  // Sống được 1 giây thì biến mất

    private Text damageText;

    // Hàm này sẽ được gọi ngay khi cục Text vừa sinh ra để bơm số vào
    public void Setup(int damageAmount, bool isCrit)
    {
        damageText = GetComponentInChildren<Text>();
        damageText.text = damageAmount.ToString();

        if (isCrit)
        {
            // Nếu là chí mạng: Chữ to hơn và màu Đỏ
            damageText.color = Color.red;
            damageText.fontSize += 20; 
        }
        else
        {
            // Đánh thường: Màu vàng
            damageText.color = Color.yellow;
        }

        // Đẩy vị trí spawn lên trên đầu mục tiêu một chút để không che khuất nhân vật
        transform.position += new Vector3(Random.Range(-0.5f, 0.5f), 1f, 0); 
        
        // Hẹn giờ tự hủy
        Destroy(gameObject, destroyTime);
    }

    void Update()
    {
        // Liên tục bay lên trên
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;
    }
}