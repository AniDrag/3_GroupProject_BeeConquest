using UnityEngine;

public class AbilitySettings : ScriptableObject
{
    [SerializeField] public string AbilityName = "Ability";
    [SerializeField] public GameObject abilityVisualPrefab;
    [SerializeField] public Sprite image;
}
