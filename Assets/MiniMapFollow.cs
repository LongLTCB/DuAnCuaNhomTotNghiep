using UnityEngine;
using Photon.Pun;

public class MiniMapFollow : MonoBehaviour
{
[Header("Follow")]
public Transform targetPlayer;
public float smoothSpeed = 12f;
public Vector2 offset = Vector2.zero;

[Header("Camera Z")]
public float cameraZ = -10f;
// nang cap
[Header("Zoom")]
public float padding = 2f;
private Camera cam;
private bool zoomCalculated = false;

[Header("Auto Find")]
public string playerTag = "Player";

private void Start()
{
    cam = GetComponent<Camera>();

    FindLocalPlayer();
    KeepCameraTopDown();
}

private void LateUpdate()

{
    if (!zoomCalculated)
{
    CalculateZoom();
}
if (targetPlayer == null)
{
FindLocalPlayer();
return;
}

Vector3 desiredPosition = new Vector3(
targetPlayer.position.x + offset.x,
targetPlayer.position.y + offset.y,
cameraZ
);

transform.position = Vector3.Lerp(
transform.position,
desiredPosition,
smoothSpeed * Time.deltaTime
);
}

private void FindLocalPlayer()
{
if (PhotonNetwork.LocalPlayer != null &&
PhotonNetwork.LocalPlayer.TagObject is GameObject taggedObj)
{
PhotonView taggedPv = taggedObj.GetComponent<PhotonView>();
if (taggedPv != null && taggedPv.IsMine)
{
targetPlayer = taggedObj.transform;
return;
}
}

PhotonView[] allViews = FindObjectsOfType<PhotonView>();
foreach (PhotonView pv in allViews)
{
if (pv != null && pv.IsMine && pv.CompareTag(playerTag))
{
targetPlayer = pv.transform;
return;
}
}
}

private void KeepCameraTopDown()
{
transform.rotation = Quaternion.identity;
}
private void CalculateZoom()
{
    if (GroundPositionManager.groundPositions == null ||
        GroundPositionManager.groundPositions.Count == 0)
        return;

    Vector3 min = GroundPositionManager.groundPositions[0];
    Vector3 max = GroundPositionManager.groundPositions[0];

    foreach (Vector3 p in GroundPositionManager.groundPositions)
    {
        min = Vector3.Min(min, p);
        max = Vector3.Max(max, p);
    }

    float width = (max.x - min.x) + padding * 2f;
    float height = (max.y - min.y) + padding * 2f;

    float sizeByHeight = height * 0.5f;
    float sizeByWidth = (width * 0.5f) / cam.aspect;

    // Điều chỉnh hệ số này để zoom gần hoặc xa hơn
    cam.orthographicSize = Mathf.Max(sizeByHeight, sizeByWidth) * 1.1f;

    zoomCalculated = true;
}
}