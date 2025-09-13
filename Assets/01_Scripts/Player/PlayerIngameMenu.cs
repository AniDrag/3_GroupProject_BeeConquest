using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
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
    [SerializeField] private PlayerCore player;

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

        if (player == null){    
            Debug.LogError("❌ Input Action 'Menu' not found in PlayerInput!");
            player = transform.parent.GetComponent<PlayerCore>();
        }

        menuPanel.SetActive(false);
        optionsPanel.SetActive(false);
        // force PlayerInput to detect scheme
        if (string.IsNullOrEmpty(inputs.currentControlScheme))
            inputs.SwitchCurrentControlScheme("Keyboard&Mouse", Keyboard.current, Mouse.current);
        //Debug.Log("Scheme type: " + inputs.currentControlScheme + " || Is cursure visible: " + menuPanel.activeSelf);
        ApplyControlScheme(inputs.currentControlScheme);
    }

    private void OnEnable()
    {
        menuAction.performed += UI_ToggleMenu;
        inputs.onControlsChanged += OnControlsChanged;

        optionsBtn.onClick.AddListener(UI_OpenOptions);
        logOutBtn.onClick.AddListener(UI_LogOut);
        closeBtn.onClick.AddListener(UI_CloseMenu);
    }

    private void OnDisable()
    {
        menuAction.performed -= UI_ToggleMenu;
        inputs.onControlsChanged -= OnControlsChanged;

        optionsBtn.onClick.RemoveListener(UI_OpenOptions);
        logOutBtn.onClick.RemoveListener(UI_LogOut);
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
        menuOpen = true;
        menuPanel.SetActive(true);
        optionsPanel.SetActive(false);

        ControllPlayerCamAndMove();
        ApplyControlScheme(inputs.currentControlScheme);
    }

    private void UI_CloseMenu()
    {
        Debug.Log("Disable menu");
        menuOpen = false;
        menuPanel.SetActive(false);
        optionsPanel.SetActive(false);
        ControllPlayerCamAndMove();
        ApplyControlScheme(inputs.currentControlScheme);
    }


    // ───────────────────────────── Button Actions
    private void UI_OpenOptions() => optionsPanel.SetActive(true);

    private void UI_LogOut()
    {
        Debug.Log("Logging out...");
        Game_Manager.instance.LeaveServer(player.playerID); // replace with your actual function
        Scene_Manager.instance.SCENE_LoadScene(0);// go to main menu
        
    }
    // ───────────────────────────── Player Control
    private void ControllPlayerCamAndMove()
    {
        bool enableGameplay = !menuPanel.activeSelf;

        //player.GetComponent<PlayerMovemant>().enabled = enableGameplay;
        Camera.main.GetComponent<PlayerCamera>().enabled = enableGameplay;
    }
    // ───────────────────────────── Input Switching
    private void OnControlsChanged(PlayerInput playerInput)
    {
        ApplyControlScheme(playerInput.currentControlScheme);
    }
    private void ApplyControlScheme(string scheme)
    {
        bool menuIsOpen = menuPanel.activeSelf;
        
        //Debug.Log("Scheme type: " + scheme + " || Is cursure visible: " + menuIsOpen);
        if (scheme == "Keyboard&Mouse")
        {
            Cursor.visible = menuIsOpen;
            Cursor.lockState = menuIsOpen ? CursorLockMode.Confined : CursorLockMode.Locked;
            EventSystem.current.SetSelectedGameObject(null); // clear gamepad selection
        }
        else if (scheme == "Gamepad")
        {
            Cursor.visible = false;
            if (menuIsOpen)
            {
                // auto-select first button for controller navigation
                EventSystem.current.SetSelectedGameObject(optionsBtn.gameObject);
            }
        }
    }
    /// <summary>
    /// When opening a menu we check controll skeme to enable or not enable cursor visability.
    /// </summary>
   // private void UpdateCursorVisibility()
   // {
   //     if (inputs.currentControlScheme == "Keyboard&Mouse")
   //     {
   //         Cursor.lockState = CursorLockMode.Confined;
   //         Cursor.visible = true;
   //     }
   //     else
   //     {
   //         Cursor.visible = false;
   //     }
   // }
    #region Interaction system
    // ───────────────────────────── Interaction system
    public void UI_ShowOrCloseInteractBox(bool state) =>
        interactBox.gameObject.SetActive(state);

    public void UI_SetInteractText(string text) => interactText.text = text;
    #endregion
}
