using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class SaveableEntity : MonoBehaviour
{
    [SerializeField] private string id;

    public string Id => id;

    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(id))
            id = Guid.NewGuid().ToString("N");
    }
}

