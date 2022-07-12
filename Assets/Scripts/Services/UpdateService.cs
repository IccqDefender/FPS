using System;
using UnityEngine;

public class UpdateService : MonoBehaviour
{
    public static event Action OnUpdate;
    public static event Action OnFixedUpdate;
    public static event Action OnLateUpdate;

    private void Update() => OnUpdate?.Invoke();
    private void FixedUpdate() => OnFixedUpdate?.Invoke();
    private void LateUpdate() => OnLateUpdate?.Invoke();
}
