using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SphereCollider))]
public class WorldInteractor : MonoBehaviour
{
    [SerializeField] private PlayerInput input;
    [SerializeField] private float interactionRadius = 2f;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private KeyCode cycleKey = KeyCode.Tab;
    [SerializeField] PlayerIngameMenu menu;
    [SerializeField] private GameObject player;

    private readonly List<Iinteract> _nearbyInteractables = new List<Iinteract>();
    private int _currentIndex = 0;

    private void Awake()
    {
       // input = transform.parent.GetComponent<PlayerMovemant>().input;
        SphereCollider col = GetComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = interactionRadius;
        player = transform.parent.gameObject;
    }

    private void Update()
    {
        Debug.Log(_currentIndex);
        if (_nearbyInteractables.Count == 0) return;

        // Cycle through interactables
        if (Input.GetKeyDown(cycleKey))
        {
            _currentIndex++;
            if (_currentIndex >= _nearbyInteractables.Count) _currentIndex = 0;
            UpdateUI();
        }
        
        // Interact with current
        if (Input.GetKeyDown(interactKey))
        {
            _nearbyInteractables[_currentIndex].Interact(player);
        }
    }
    private void UpdateUI()
    {
        if (_nearbyInteractables.Count == 0) return;

        _currentIndex = Mathf.Clamp(_currentIndex, 0, _nearbyInteractables.Count - 1);

        var text = _nearbyInteractables[_currentIndex].GetInteractionText();
        menu.UI_SetInteractText(text);
        Debug.Log($"UI updated: {text}");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Iinteract>(out var interactable))
        {
           
            Debug.Log("Added an interactor");
            InteractionType type = interactable.Type();
            if (type == InteractionType.WhenInRange)
                interactable.Interact(player);
            else if (type == InteractionType.OnKeyPress)
            {
                if (!_nearbyInteractables.Contains(interactable))_nearbyInteractables.Add(interactable);

                // Only reset to the first interactable if this is the first one added
                if (_nearbyInteractables.Count == 1)
                    _currentIndex = 0;
                else _currentIndex++;

                Debug.Log(interactable.GetInteractionText());
                UpdateUI();
                menu.UI_ShowOrCloseInteractBox(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<Iinteract>(out var interactable))
        {
            interactable.DeInteract(player);
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
