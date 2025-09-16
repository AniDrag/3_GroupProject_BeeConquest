using UnityEngine;

#region Quest Related Components
interface IQuestItem {
    string Name { get; }
    string GetItemName();


}
public enum QuestTypes
{
    KillQuest,
    CollectionQuest,
    InteractQuest
}
#endregion

#region Interaction Related components
/// <summary>
/// Uses void Interact(GameObject interactor) as the triger for each function
/// Use string GetInteractionText() is a pointer for the for the name of iteraction
/// So ise public void GetInteractionText() => nameOfInteractible; as a function in said script;
/// bool CanInteract(GameObject interactor); so we check if we can interact
/// InteractionType Type() => Type for what type ot os
/// </summary>
public interface IInteract
{
    void Interact(GameObject interactor);
    void DeInteract(GameObject interactor);
    string GetInteractionText(); // e.g. "Open Chest", "Talk", "Pick Up"

    bool CanInteract(GameObject interactor);
    InteractionType Type();
}
public enum InteractionType
{
    OnKeyPress,     // manual, player-initiated
    OnKeyHold,        // hold button for progress bar (e.g., mining, charging)
    WhenInRange,    // auto-trigger
    Toggle,         // acts like a switch (light on/off, door open/close)
    Continuous,     // triggers every frame while in range/holding (damage zones, healing pools)
    Remote,         // can be triggered from distance (e.g., aiming at it)
}
#endregion
