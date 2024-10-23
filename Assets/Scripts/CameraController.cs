using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float _cameraSpeed = 5f;

    public Vector3 Position;

    private void Update()
    {
        transform.position = Vector3.Lerp(transform.position, Position, _cameraSpeed * Time.deltaTime);
    }
}
