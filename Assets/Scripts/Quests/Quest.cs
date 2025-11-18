using DialogueEditor;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// How to create a simple collection quest:
/// 
/// Collectible:
///     - Create an object with a trigger collider
///     - Add Collectible script to it and give PromptCanvas (E to interact)
/// Quest Target (Object that moves at end):
///     - Create an object that is going to move
///     - Add an Animator
///     - In the Animation window, create a new animation such as "moving.anim"
///     - In Animation, add transform property: rotation, scale and/or position.
///     - Add keyframes with different transform properties
///     - Find the .anim and turn off loop.
///     - In Animator, right click, create new empty state, right click it, set it as default layer state
///     - Keep the previous animation that was created, unconnected
///     - Add Quest Target to the moving object, and set the completion animation name as the animation in the Animator (ex. moving)
/// NPC:
///     - Create a pre-collected conversation and a post-collected conversation.
///     - Set the current conversation as the pre-collected conversation.
///     - Do this after creating Quest: Click on last speech node of post-collected and add an event: Quest -> Conversation Finished.
/// Quest:
///     - Add the quest to the QuestManager under GameManager
///     - Add collectible, list of quest targets, the NPC and the post-collected conversation.
/// 
/// Should work :)
/// </summary>
public class Quest : MonoBehaviour
{
    [Header("Objects")]
    [SerializeField] private Collectible collectible;
    [SerializeField] private List<QuestTarget> questTargets;

    [Header("NPC")]
    [SerializeField] private DialogueNPC npc;
    [SerializeField] private NPCConversation completedConversation;

    private void OnEnable()
    {
        collectible.OnCollected += QuestCompleted;
    }

    private void OnDisable()
    {
        collectible.OnCollected -= QuestCompleted;
    }

    private void QuestCompleted()
    {
        npc.SwitchConversation(completedConversation);
    }

    public void ConversationFinished()
    {
        foreach (QuestTarget target in questTargets)
        {
            target.QuestCompleted();
        }
    }
}
