using AniDrag.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItem : MonoBehaviour
{
    [SerializeField, Required] Image beeSprite;
    [SerializeField, Required] public Button button;
    [SerializeField, Required] TextMeshProUGUI itemAmountText;
    public int childIdex = 0;

    private void Reset()
    {
        beeSprite = transform.GetComponent<Image>();
        button = transform.GetChild(0).GetComponent<Button>();
        button.onClick.RemoveAllListeners();
    }

    public void AsignData(Sprite sprite)
    {
        beeSprite.sprite = sprite;
    }
    public void UpdateText(string newText)
    {
        itemAmountText.text = newText;
    }
}
