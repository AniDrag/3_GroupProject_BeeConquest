using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using static MoveOnInteract;
[RequireComponent(typeof(SphereCollider))]

public class InteractActivator : MonoBehaviour
{
    public enum InteractionType
    {
        OnKeyPress,
        WhenInRange,
        WhenItemPlacedInZone,
        WhenDestroyed,
    }

    [Header("Settings")]
    public InteractionType interactionType = InteractionType.OnKeyPress;
    [SerializeField] private bool canTriggerMultipleTimes = false;
    [SerializeField] private float interactionRadius = 2f;
    [Tooltip("time it takes before it can be triggered again")]
    [SerializeField, Range(0.1f,60f)] private float retriggerInterval = 2f;
    [SerializeField] private LayerMask triggerableLayers;
    [SerializeField] private string[] triggerableTags;

    [Header("Events")]
    public UnityEvent onEnterTriggered;
    public UnityEvent onExitTriggered;
    private void Awake()
    {
        SphereCollider collider = GetComponent<SphereCollider>();
        collider.radius = interactionRadius;
    }
    private bool _triggered = false;

    public void TRIG_OnEnter(GameObject obj)
    {
        
        if (IsTriggerable(obj) && !_triggered)
        {

            _triggered = true;
            onEnterTriggered?.Invoke();
            if (canTriggerMultipleTimes)
            {
                Invoke(nameof(ResetTrigger), retriggerInterval);
            }
        }
    }

    private void TRIG_OnExit(GameObject obj)
    {
        _triggered = true;
        onExitTriggered?.Invoke();
        if (canTriggerMultipleTimes)
        {
            Invoke(nameof(ResetTrigger), retriggerInterval);
        }
    }

    private bool IsTriggerable(GameObject obj)
    {
        // Check layer mask
        if (((1 << obj.layer) & triggerableLayers.value) == 0)
            return false;

        // Check tags if provided
        if (triggerableTags != null && triggerableTags.Length > 0)
        {
            foreach (var tag in triggerableTags)
            {
                if (obj.CompareTag(tag))
                    return true;
            }
            return false;
        }

        return true; // no tags defined = allow
    }

    void ResetTrigger()
    {
        _triggered = false;
    }
    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        // Draw final destination
        Gizmos.color = new Color(1, 0, 0, 0.2f);
        Gizmos.DrawSphere(transform.position, interactionRadius);
#endif
    }
}

