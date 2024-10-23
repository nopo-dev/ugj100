using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private GameObject _player;
    [SerializeField] private float _cameraSpeed = 5f;

    private void Update()
    {
        transform.position = Vector3.Lerp(transform.position, _player.transform.position, _cameraSpeed * Time.deltaTime);
    }
}
