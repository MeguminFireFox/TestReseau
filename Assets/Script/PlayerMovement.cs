using Unity.Netcode;
using UnityEngine;
#if NEW_INPUT_SYSTEM_INSTALLED
using UnityEngine.InputSystem;
#endif

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private float _speed = 5f;

    void Update()
    {
        if (!IsOwner || !IsSpawned) return;

        float moveX = 0f;
        float moveZ = 0f;

#if ENABLE_INPUT_SYSTEM && NEW_INPUT_SYSTEM_INSTALLED
        if (Keyboard.current.aKey.isPressed) moveX = -1f;
        if (Keyboard.current.dKey.isPressed) moveX = 1f;
        if (Keyboard.current.wKey.isPressed) moveZ = 1f;
        if (Keyboard.current.sKey.isPressed) moveZ = -1f;
#endif

        Vector3 movement = new Vector3(moveX, 0f, moveZ).normalized;
        transform.Translate(movement * _speed * Time.deltaTime, Space.World);
    }
}
