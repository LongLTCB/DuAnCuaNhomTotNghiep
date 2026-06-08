using UnityEngine;

public class DepthSorter : MonoBehaviour
{
    void Start()
    {
        // Bắt Camera phải vẽ các vật thể theo thứ tự: Chân (Y) thấp hơn thì nằm đè lên trên
        Camera.main.transparencySortMode = TransparencySortMode.CustomAxis;
        Camera.main.transparencySortAxis = new Vector3(0, 1, 0);
    }
}