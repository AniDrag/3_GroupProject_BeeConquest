using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SphereCollider))]
public class WorldInteractor : MonoBehaviour
{
    [Header("----- Refrences -----")]
    [SerializeField] private PlayerCore player;

    [Header("----- Inputs -----")]
    [SerializeField] private PlayerInput inputs;
    private InputAction cycleAction;
    private InputAction interactAction;

    [Header("----- Interaction settings -----")]
    [SerializeField] private float interactionRadius = 2f;  
    
    private readonly List<IInteract> _nearbyInteractables = new List<IInteract>();
    private int _currentIndex = 0;

    private void Awake()
    {

        cycleAction = inputs.actions["Cycle"];
        interactAction = inputs.actions["Interact"];
        // input = transform.parent.GetComponent<PlayerMovemant>().input;
        SphereCollider col = GetComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = interactionRadius;
       
    }
    private void OnEnable()
    {
        cycleAction.Enable();
        interactAction.Enable();
        cycleAction.performed += CycleInteractible;
        interactAction.performed += InteractWithObject;
    }

    private void OnDisable()
    {
        cycleAction.performed -= CycleInteractible;
        interactAction.performed -= InteractWithObject;
        cycleAction.Disable();
        interactAction.Disable();
    }

    private void CycleInteractible(InputAction.CallbackContext ctx)
    {
        if (_nearbyInteractables.Count == 0) return;
        _currentIndex++;
        if (_currentIndex >= _nearbyInteractables.Count) _currentIndex = 0;
        UpdateUI();
    }
    private void InteractWithObject(InputAction.CallbackContext ctx)
    {
        if (_nearbyInteractables.Count == 0) return;
        _nearbyInteractables[_currentIndex].Interact(player.gameObject);
    }
    private void UpdateUI()
    {
        if (_nearbyInteractables.Count == 0) return;

        _currentIndex = Mathf.Clamp(_currentIndex, 0, _nearbyInteractables.Count - 1);

        var text = _nearbyInteractables[_currentIndex].GetInteractionText();
        player.GetComponent<PlayerCore>().visualsUI.interactedItemText.text = text;
        //menu.UI_SetInteractText(text);
        //Debug.Log($"UI updated: {text}");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<IInteract>(out var interactable))
        {
           
            //Debug.Log("Added an interactor");
            InteractionType type = interactable.Type();
            if (type == InteractionType.WhenInRange)
                interactable.Interact(player.gameObject);
            else if (type == InteractionType.OnKeyPress)
            {
                if (!_nearbyInteractables.Contains(interactable))_nearbyInteractables.Add(interactable);

                // Only reset to the first interactable if this is the first one added
                if (_nearbyInteractables.Count == 1)
                    _currentIndex = 0;
                else _currentIndex++;

                Debug.Log(interactable.GetInteractionText());
                UpdateUI();
                player.visualsUI.UI_ShowOrCloseInteractBox(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<IInteract>(out var interactable))
        {
            interactable.DeInteract(player.gameObject);
            _nearbyInteractables.Remove(interactable);
            if (_nearbyInteractables.Count == 0)
            {
                // No interactables left, hide UI and reset index
                _currentIndex = 0;
                player.visualsUI.UI_ShowOrCloseInteractBox(false);
                return;
            }

            // Clamp index so it doesn’t go out of range
            if (_currentIndex >= _nearbyInteractables.Count)
            {
                _currentIndex = _nearbyInteractables.Count - 1;
            }

            player.visualsUI.UI_SetInteractText(_nearbyInteractables[_currentIndex].GetInteractionText());
        }
    }
}
