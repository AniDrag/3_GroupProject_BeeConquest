using UnityEngine;

[CreateAssetMenu(fileName ="Ability UI prefab", menuName = "Game Tools/ Bees", order = 0)]
public class AbilitySettings : ScriptableObject
{
    [SerializeField] public string AbilityName = "Ability";
    [SerializeField] public GameObject abilityVisualPrefab;
    [SerializeField] public Sprite sprite;
}
