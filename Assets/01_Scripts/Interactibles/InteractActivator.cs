using UnityEngine;
using UnityEngine.Events;
//[RequireComponent(typeof(SphereCollider))]

public class InteractActivator : MonoBehaviour, Iinteract
{
    public enum InteractionType
    {
        OnKeyPress,
        WhenInRange,
        WhenItemPlacedInZone
    }
    // ("===== Text settings ======")]
    [SerializeField] private string interactionText = "(E) ";
    [SerializeField] private string objectName = "Test Object";
    [SerializeField] private string triggerText = "Triggerd this";

    //("===== Events =====")]
    [SerializeField] public UnityEvent triggerOnInteract;
    [SerializeField] public UnityEvent onEnterTriggered;
    [SerializeField] public UnityEvent onExitTriggered;

    // ("===== Settings =====")]
    [SerializeField] public InteractionType interactionType = InteractionType.OnKeyPress;
    [Tooltip("time it takes before it can be triggered again")]
    [SerializeField, Range(0.1f, 60f)] private float retriggerInterval = 2f;
    [SerializeField] private bool canInteractOnlyOnce;

    private bool interacted;
    private bool entered;


    #region InteractionData
    public virtual void Interact()// can be overwriten
    {
        if (!interacted)
        {
            interacted = true;
            Debug.Log(triggerText);
            if (interactionType == InteractionType.WhenItemPlacedInZone)
            {
                if (entered)
                {
                    entered = false;
                    onExitTriggered?.Invoke();
                    interacted = canInteractOnlyOnce;
                }
                else
                {
                    onEnterTriggered?.Invoke();
                }
            }
            else
            {
                triggerOnInteract?.Invoke();

            }
            if (!canInteractOnlyOnce) Invoke(nameof(ResetTrigger), retriggerInterval);
        }
    }

    public string GetInteractionText() => interactionText + objectName;

    #endregion
    void ResetTrigger()
    {
        interacted = false;
    }
}
/*
    [SerializeField] private bool canTriggerMultipleTimes = false;
    [SerializeField] private float interactionRadius = 2f;
    [Tooltip("time it takes before it can be triggered again")]
    
    [SerializeField] private LayerMask triggerableLayers;
    [SerializeField] private string[] triggerableTags;

    //[Header("Events")]
    //public UnityEvent onEnterTriggered;
    //public UnityEvent onExitTriggered;
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
}*/