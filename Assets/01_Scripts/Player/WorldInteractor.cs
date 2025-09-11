using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SphereCollider))]
public class WorldInteractor : MonoBehaviour
{
    [SerializeField] private float interactionRadius = 2f;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private KeyCode cycleKey = KeyCode.Tab;
    [SerializeField] PlayerIngameMenu menu;

    private readonly List<Iinteract> _nearbyInteractables = new List<Iinteract>();
    private int _currentIndex = 0;
    public PlayerInput input;

    private void Awake()
    {
       // input = transform.parent.GetComponent<PlayerMovemant>().input;
        SphereCollider col = GetComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = interactionRadius;
    }

    private void Update()
    {
        if (_nearbyInteractables.Count == 0) return;

        // Cycle through interactables
        if (Input.GetKeyDown(cycleKey))
        {
            // if index is bigger /= to max interactibles it is set to 0
            _currentIndex++;
            if (_currentIndex >= _nearbyInteractables.Count)
            {
                _currentIndex = 0;
            }
            _nearbyInteractables[_currentIndex].GetInteractionText();
            menu.UI_SetInteractText(_nearbyInteractables[_currentIndex].GetInteractionText());
            Debug.Log("Swap Text, Index num:" + _currentIndex);
        }
        

        // Interact with current
        if (Input.GetKeyDown(interactKey))
        {
            _nearbyInteractables[_currentIndex].Interact();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Iinteract>(out var interactable))
        {
            InteractActivator interactibleDetails = interactable as InteractActivator;
            if (interactibleDetails != null && interactibleDetails.interactionType == InteractActivator.InteractionType.WhenInRange)
                interactable.Interact();
            else if (interactibleDetails.interactionType == InteractActivator.InteractionType.OnKeyPress)
            {
                _nearbyInteractables.Add(interactable);
                Debug.Log(interactable.GetInteractionText());
                _currentIndex++;
                menu.UI_SetInteractText(interactable.GetInteractionText());
                menu.UI_ShowOrCloseInteractBox(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<Iinteract>(out var interactable))
        {
            _nearbyInteractables.Remove(interactable);

            if (_nearbyInteractables.Count == 0)
            {
                // No interactables left, hide UI and reset index
                _currentIndex = 0;
                menu.UI_ShowOrCloseInteractBox(false);
                return;
            }

            // Clamp index so it doesn’t go out of range
            if (_currentIndex >= _nearbyInteractables.Count)
            {
                _currentIndex = _nearbyInteractables.Count - 1;
            }

            menu.UI_SetInteractText(_nearbyInteractables[_currentIndex].GetInteractionText());
        }
    }
}
