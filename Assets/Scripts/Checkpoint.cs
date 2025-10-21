using System;
using UnityEngine;

[ExecuteAlways]
public class Checkpoint : MonoBehaviour
{
    [SerializeField, HideInInspector] private string checkpointId;

    public string CheckpointId => checkpointId;

    private void OnValidate()
    {
        if (!gameObject.scene.IsValid())
        {
            if (checkpointId == null)
                checkpointId = String.Empty;

            return;
        }

        var checkpoints = FindObjectsByType<Checkpoint>(FindObjectsSortMode.None);
        bool duplicate = Array.Exists(checkpoints, c => c != this && c.CheckpointId == checkpointId);

        if (string.IsNullOrEmpty(checkpointId) || duplicate)
        {
            checkpointId = Guid.NewGuid().ToString();
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
}
