using UnityEngine;
using System.Collections;

public class Ability : MonoBehaviour, IInteract
{
    private string AbilityName = "Ability";
    private BasicBee bee;
    float time;
    [SerializeField, Range(5,20)] float duration = 10;

    [SerializeField] float bobAmplitude = 0.15f;        // idle up/down amplitude
    [SerializeField] float bobFrequency = 1.4f;        // idle cycles per second
    [SerializeField] Vector3 fixedEulerRotation = new Vector3(0f, 180f, 0f); // rotation while idle
    [SerializeField] float interactRise = 1.0f;        // how far up during interact animation
    [SerializeField] float interactDuration = 0.6f;   // how long the interact animation takes

    [SerializeField] float spinSpeed = 90f;           // degrees per second
    [SerializeField] Vector3 spinAxis = Vector3.up;  // axis to spin around while idle
    private float _spinOffset;                        // random start offset so spins differ

    // internal state (add these to your class)
    Vector3 _spawnPos;
    bool _isInteracting = false;
    Coroutine _interactRoutine;


    public void Interact(GameObject interactor)
    {
        // trigger ability logic immediately (same as before)
        bee.TriggerAbilityLogic(bee, interactor.GetComponent<PlayerCore>(), transform.position);

        // if we are already animating, ignore duplicate interacts
        if (_isInteracting) return;

        // start the animation coroutine (which will destroy at the end)
        if (_interactRoutine != null) StopCoroutine(_interactRoutine);
        _interactRoutine = StartCoroutine(AnimateRiseThenDestroy());
    }
    public void DeInteract(GameObject interactor)
    {

    } //uselsess
    public string GetInteractionText() => AbilityName;
    public bool CanInteract(GameObject interactor) => interactor.GetComponent<PlayerCore>() != null;
    public InteractionType Type() => InteractionType.WhenInRange;

    public void SetAbilityData(BasicBee parentBee, string setName)
    {
        AbilityName = setName;
        bee = parentBee;
    }
    private void Start()
    {
        time = Time.time + duration;
        _spawnPos = transform.position;
        _isInteracting = false;

        _spinOffset = Random.Range(0f, 360f);

    }
    private void Update()
    {
        // auto destroy when lifetime expired (only when not interacting)
        if (!_isInteracting && Time.time >= time)
        {
            Destroy(gameObject);
            return;
        }

        // while interacting we don't run the idle bob/rotate
        if (_isInteracting) return;

        // Sine bobbing around the spawn position
        float bob = Mathf.Sin(Time.time * (Mathf.PI * 2f) * bobFrequency) * bobAmplitude;
        transform.position = _spawnPos + Vector3.up * bob;

        // Fixed base rotation (how it should face while idle)
        Quaternion baseRot = Quaternion.Euler(fixedEulerRotation);

        // Spin around provided axis with per-instance offset
        float spinAngle = _spinOffset + Time.time * spinSpeed; // degrees
        Quaternion spin = Quaternion.AngleAxis(spinAngle, spinAxis.normalized);

        // Combine base rotation with spin (spin applied after base rotation)
        transform.rotation = baseRot * spin;
    }

    private IEnumerator AnimateRiseThenDestroy()
    {
        _isInteracting = true;

        // cache start values
        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + Vector3.up * interactRise;
        Quaternion startRot = transform.rotation;

        // resolve camera
        Camera cam = Camera.main;

        // compute rotation that looks at the camera (front of object -> camera)
        Quaternion targetRot = startRot;
        if (cam != null)
        {
            Vector3 toCam = cam.transform.position - transform.position;
            if (toCam.sqrMagnitude > 0.0001f)
                targetRot = Quaternion.LookRotation(toCam, Vector3.up);
        }

        float elapsed = 0f;
        while (elapsed < interactDuration)
        {
            float p = elapsed / interactDuration;
            // a smooth ease-out feel
            float eased = 1f - Mathf.Pow(1f - p, 3f);

            // position: move up with easing
            transform.position = Vector3.Lerp(startPos, targetPos, eased);

            // rotation: slerp toward the camera-facing rotation
            transform.rotation = Quaternion.Slerp(startRot, targetRot, eased);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // final snap to target
        transform.position = targetPos;
        transform.rotation = targetRot;

        // destroy after animation
        Destroy(gameObject);
    }

}
