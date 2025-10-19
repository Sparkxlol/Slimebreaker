using System;
using UnityEngine;

[ExecuteAlways]
public class Checkpoint : MonoBehaviour
{
    [SerializeField, HideInInspector] private string checkpointId;

    public string CheckpointId => checkpointId;

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(checkpointId))
        {
            checkpointId = Guid.NewGuid().ToString();
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
}
