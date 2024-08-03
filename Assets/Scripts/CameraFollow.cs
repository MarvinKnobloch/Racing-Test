using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private float cameraRotation;

    [SerializeField] private Vector3 offset;


    private void LateUpdate()
    {
        transform.position = Player.Instance.transform.position + offset;
        transform.rotation = Quaternion.Slerp(transform.rotation, Player.Instance.transform.rotation, cameraRotation * Time.deltaTime);
    }
}
