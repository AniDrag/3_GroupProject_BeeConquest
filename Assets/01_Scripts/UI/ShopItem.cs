using TMPro;
using UnityEngine;

public class ShopItem: MonoBehaviour 
{
    [SerializeField] TMP_Text itemName;
    [SerializeField] private TextMeshProUGUI itemPrice;
    [SerializeField] private int minFontSize = 100;
    [SerializeField] private int maxFontSize = 150;


    public void AsignData(string headerText,long amount)
    {
        itemName.text = headerText;
        itemPrice.text = amount.ToString("C0");
        // Force auto-sizing
        itemPrice.enableAutoSizing = true;
        itemPrice.fontSizeMin = minFontSize;   // Minimum readable font
        itemPrice.fontSizeMax = maxFontSize;   // Max font size
    }
}
