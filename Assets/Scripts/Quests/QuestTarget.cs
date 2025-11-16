using System.Collections.Generic;
using UnityEngine;

public class QuestTarget : MonoBehaviour
{
    [SerializeField] private string completionAnimationName;
    private Animator anim;

    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    public void QuestCompleted()
    {
        anim.Play(completionAnimationName);
    }
}
