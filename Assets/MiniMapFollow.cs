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

[Header("Auto Find")]
public string playerTag = "Player";

private void Start()
{
FindLocalPlayer();
KeepCameraTopDown();
}

private void LateUpdate()
{
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
}