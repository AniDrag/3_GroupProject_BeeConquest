// client
using UnityEngine;
using System.Collections.Generic;

public class FloatingLabelPool : MonoBehaviour
{
    public static FloatingLabelPool Instance { get; private set; }

    [Header("Prefab & settings")]
    public FloatingLabel prefab;         // assign your prefab (has FloatingLabel + TMP)
    public Transform labelsParent;       // optional parent in scene

    Stack<FloatingLabel> pool = new Stack<FloatingLabel>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    FloatingLabel CreateNew()
    {
        var go = Instantiate(prefab.gameObject, labelsParent ? labelsParent : transform);
        var fl = go.GetComponent<FloatingLabel>();
        go.SetActive(false);
        return fl;
    }

    public FloatingLabel Get()
    {
        FloatingLabel item = pool.Count > 0 ? pool.Pop() : CreateNew();
        item.gameObject.SetActive(true);
        return item;
    }

    public void Return(FloatingLabel item)
    {
        item.gameObject.SetActive(false);
        item.transform.SetParent(labelsParent ? labelsParent : transform, false);
        pool.Push(item);
    }

    // Show wrapper: amount with formatted string and color mapping
    public void ShowAmount(long amount, Vector3 worldPos, Color c)
    {
        var label = Get();
        string formatted = Formatter.FormatSignedWithSpaces(amount);
        label.Show(formatted, c, worldPos);
    }

    public Color ColorForCell(CellColor c)
    {
        switch (c)
        {
            case CellColor.Red: return Color.red;
            case CellColor.Green: return Color.green;
            case CellColor.Blue: return Color.cyan;
            // extend mapping as needed
            default: return Color.white;
        }
    }
}



