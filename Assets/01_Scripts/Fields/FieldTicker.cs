using System.Collections.Generic;
using UnityEngine;

// Small singleton to centralize field ticking.
// FieldGenerator registers itself in OnEnable and deregisters in OnDisable.
public class FieldTicker : MonoBehaviour
{
    //private static FieldTicker _instance;
    //public static FieldTicker Instance
    //{
    //    get
    //    {
    //        if (_instance == null)
    //        {
    //            var go = new GameObject("FieldTicker");
    //            DontDestroyOnLoad(go);
    //            _instance = go.AddComponent<FieldTicker>();
    //        }
    //        return _instance;
    //    }
    //}

    //private readonly List<FieldGenerator> registered = new List<FieldGenerator>();

    //public void Register(FieldGenerator fg)
    //{
    //    if (fg == null) return;
    //    if (!registered.Contains(fg)) registered.Add(fg);
    //}

    //public void Unregister(FieldGenerator fg)
    //{
    //    if (fg == null) return;
    //    registered.Remove(fg);
    //}

    //// Use FixedUpdate for deterministic physics-like ticks (matches your existing code)
    //private void FixedUpdate()
    //{
    //    float dt = Time.fixedDeltaTime;
    //    for (int i = 0; i < registered.Count; i++)
    //    {
    //        var fg = registered[i];
    //        if (fg != null) fg.Tick(dt);
    //    }
    //}
}

