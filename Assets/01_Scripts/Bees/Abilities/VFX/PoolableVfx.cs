using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class PoolableVfx : MonoBehaviour
{
    [HideInInspector] public string PoolKey;
    [HideInInspector] public AbilityVfxPooler RootPooler;

    Coroutine running;

    /// <summary>
    /// Called by the pool when this instance is taken from the pool.
    /// Cancels any old coroutine left over.
    /// </summary>
    public void OnTakenFromPool()
    {
        if (running != null)
        {
            StopCoroutine(running);
            running = null;
        }
    }

    /// <summary>
    /// Start a simple timer that will return this object after 'seconds'.
    /// Works the same way as your FloatingLabel: coroutine runs on this active GameObject.
    /// </summary>
    public void StartAutoReturn(float seconds)
    {
        if (running != null)
        {
            StopCoroutine(running);
            running = null;
        }
        running = StartCoroutine(AutoReturnCoroutine(seconds));
    }

    IEnumerator AutoReturnCoroutine(float seconds)
    {
        if (seconds > 0f)
            yield return new WaitForSeconds(seconds);
        // clear handle before returning so ReturnNow/OnDisable know it's finished
        running = null;
        ReturnNow();
    }

    /// <summary>
    /// Immediately return this instance to the pool (or destroy if no pooler).
    /// Cancels the running timer coroutine.
    /// </summary>
    public void ReturnNow()
    {
        if (running != null)
        {
            StopCoroutine(running);
            running = null;
        }

        if (RootPooler != null)
            RootPooler.Return(gameObject);
        else
            gameObject.SetActive(false);
    }

    void OnDisable()
    {
        // stop coroutine if object is disabled (keeps behavior consistent with FloatingLabel)
        if (running != null)
        {
            StopCoroutine(running);
            running = null;
        }
    }
}