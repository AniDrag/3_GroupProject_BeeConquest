using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerIngameMenu : MonoBehaviour
{
    [Header("===== Interact =====")]
    [SerializeField] Transform interactBox;
    [SerializeField] TMP_Text interactText;

    [Header("Input")]
    [SerializeField] private PlayerInput inputs;
    private InputAction menuAction;

    [Header("Menu References")]
    [SerializeField] private GameObject menuPanel;   // Main menu panel
    [SerializeField] private GameObject optionsPanel; // Options sub-menu
    [SerializeField] PlayerCore player;

    [Header("Menu Buttons")]
    [SerializeField] private Button optionsBtn;
    [SerializeField] private Button logOutBtn;
    [SerializeField] private Button closeBtn;

    private bool menuOpen = false;

    private void Awake()
    {
        menuAction = inputs.actions["Menu"];
        if (menuAction == null)
            Debug.LogError("❌ Input Action 'Menu' not found in PlayerInput!");
        else
            menuAction.Enable();

        if (player == null)
            player = transform.parent.GetComponent<PlayerCore>();
    }

    private void OnEnable()
    {
        if (menuAction != null)
            menuAction.performed += UI_ToggleMenu;

        optionsBtn.onClick.AddListener(UI_OpenOptions);
        logOutBtn.onClick.AddListener(UI_DoLogOut);
        closeBtn.onClick.AddListener(UI_CloseMenu);

        // start hidden
        menuPanel.SetActive(false);
        optionsPanel.SetActive(false);
    }

    private void OnDisable()
    {
        if (menuAction != null)
            menuAction.performed -= UI_ToggleMenu;

        optionsBtn.onClick.RemoveListener(UI_OpenOptions);
        logOutBtn.onClick.RemoveListener(UI_DoLogOut);
        closeBtn.onClick.RemoveListener(UI_CloseMenu);
    }

    // ───────────────────────────── Menu Toggle
    private void UI_ToggleMenu(InputAction.CallbackContext ctx)
    {
        Debug.Log("Menu action pressed!");
        if (menuOpen) UI_CloseMenu();
        else UI_OpenMenu();
    }

    private void UI_OpenMenu()
    {
        Debug.Log("Enable menu");
        menuPanel.SetActive(true);
        optionsPanel.SetActive(false);
        menuOpen = true;
        ControllPlayerCamAndMove(menuOpen);

        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }

    private void UI_CloseMenu()
    {
        Debug.Log("Disable menu");
        menuPanel.SetActive(false);
        optionsPanel.SetActive(false);
        menuOpen = false;
        ControllPlayerCamAndMove(menuOpen);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }


    // ───────────────────────────── Button Actions
    private void UI_OpenOptions()
    {
        optionsPanel.SetActive(true);
    }

    private void UI_DoLogOut()
    {
        Debug.Log("Logging out...");
        Game_Manager.instance.LeaveServer(player.playerID); // replace with your actual function
        Scene_Manager.instance.SCENE_LoadScene(0);// go to main menu
        
    }
    void ControllPlayerCamAndMove(bool menuIsOpen)
    {
        bool enableGameplay = !menuIsOpen;

        player.GetComponent<PlayerMovemant>().enabled = enableGameplay;
        Camera.main.GetComponent<PlayerCamera>().enabled = enableGameplay;
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
