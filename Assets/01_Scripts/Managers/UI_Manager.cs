using TMPro;
using UnityEngine;

public class UI_Manager : MonoBehaviour
{
    public static UI_Manager instance;


    [Header("===== Interact =====")]
    [SerializeField] Transform interactBox;
    [SerializeField] TMP_Text interactText;
    private void Awake()
    {
        if(instance != null) Destroy(gameObject); 
        
        instance = this;
        
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    #region Interaction system
    public void UI_ShowOrCloseInteractBpx(bool state)
    {
        interactBox.gameObject.SetActive(state);
    }
    public void UI_SetInteractText(string text)
    {
        interactText.text = text;
    }
    #endregion
}
