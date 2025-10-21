using UnityEngine;
using UnityEngine.EventSystems;

public class EventSystemManager : MonoBehaviour
{
    private void Awake()
    {
        if (FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Length > 1)
        {
            Destroy(this.gameObject);
        }
    }
}
