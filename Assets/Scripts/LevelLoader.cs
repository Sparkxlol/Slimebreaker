using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    [SerializeField] private int sceneBuildIndex;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<PlayerMovement>() != null)
            GameManager.instance.LoadLevel(sceneBuildIndex);
    }
}
