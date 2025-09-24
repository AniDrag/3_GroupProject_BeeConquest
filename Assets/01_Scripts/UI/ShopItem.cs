using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItem: MonoBehaviour 
{
    [SerializeField] TMP_Text itemName;
    [SerializeField] private TextMeshProUGUI itemPrice;
    [SerializeField] private Image itemImage;
    [SerializeField] private int minFontSize = 100;
    [SerializeField] private int maxFontSize = 150;
    public Button button;

    public void AsignData(string headerText,string amount, Sprite image)
    {
        itemName.text = headerText;
        itemPrice.text = amount; // .ToString("C0");
        itemImage.sprite = image;
        // Force auto-sizing
        itemPrice.enableAutoSizing = true;
        itemPrice.fontSizeMin = minFontSize;   // Minimum readable font
        itemPrice.fontSizeMax = maxFontSize;   // Max font size
    }
    public void updatePrice(string amount)
    {
        if (amount != "0")
            itemPrice.text = amount;
        else
            itemPrice.text = "FREE";
    }

}
