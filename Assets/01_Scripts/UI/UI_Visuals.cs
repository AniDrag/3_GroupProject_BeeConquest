using AniDrag.Utility;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_Visuals : MonoBehaviour
{
    [Header("----- Refrences -----")]
    [SerializeField] private PlayerCore player;

    [Header("----- Input -----")]
    [SerializeField] private PlayerInput inputs;
    private InputAction menuAction;


    [Header("----- Playr Visuals -----")]
    [SerializeField] private bool enableVarsP;
    [SerializeField, ShowIf("enableVarsP")] public TMP_Text playerHealth;
    [SerializeField, ShowIf("enableVarsP")] public Slider playerHealthSlider;
    [Space]
    [SerializeField, ShowIf("enableVarsP")] public TMP_Text pollinCounterText;
    [SerializeField, ShowIf("enableVarsP")] public TMP_Text pollinPerSecText;
    [SerializeField, ShowIf("enableVarsP")] public Image backpakImage;
    [SerializeField, ShowIf("enableVarsP")] private float backpackFillLerpSpeed;
    [Space]
    [SerializeField, ShowIf("enableVarsP")] public TMP_Text honeyCounterText;
    [Space]
    [SerializeField, ShowIf("enableVarsP")] public Image crosHairImage;
    [Space]
    [SerializeField, ShowIf("enableVarsP")] public TMP_Text performanceText;

    [Header("----- Interaction Visuals -----")]
    [SerializeField] private bool enableVarsV;
    [SerializeField, ShowIf("enableVarsV")] public GameObject interactionBox;
    [SerializeField, ShowIf("enableVarsV")] public string interactionText = "Press → E ← to interact";
    [SerializeField, ShowIf("enableVarsV")] public TMP_Text interactedItemText;

    [Header("----- Menu & Options Visuals -----")]
    [SerializeField] private bool enableVarsMO;
    [SerializeField, ShowIf("enableVarsMO")] private GameObject menuPanel;   // Main menu panel
    [SerializeField, ShowIf("enableVarsMO")] private GameObject optionsPanel; // Options sub-menu
    [Space]
    [SerializeField, ShowIf("enableVarsMO")] private Button optionsBtn;
    [SerializeField, ShowIf("enableVarsMO")] private Button logOutBtn;
    [SerializeField, ShowIf("enableVarsMO")] private Button closeBtn;
    private bool menuOpen = false;

    #region ─────────────────────────────  Default Functions ───────────────────────────── 
    private void Awake()
    {
        if (menuPanel == null) Debug.LogError("❌ menuPanel is not assigned!", this);
        if (optionsPanel == null) Debug.LogError("❌ optionsPanel is not assigned!", this);
        if (optionsBtn == null) Debug.LogError("❌ optionsBtn is not assigned!", this);
        if (logOutBtn == null) Debug.LogError("❌ logOutBtn is not assigned!", this);
        if (closeBtn == null) Debug.LogError("❌ closeBtn is not assigned!", this);
        menuAction = inputs.actions["Menu"];

        if (menuAction == null)
            Debug.LogError("❌ Input Action 'Menu' not found in PlayerInput!");
        else
            menuAction.Enable();

        if (player == null)
        {
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
    #endregion

    #region ─────────────────────────────  Pollin related Functions ───────────────────────────── 
    public void UI_UpdatePollin(long currentPollin, long maxPollin)
    {
        pollinCounterText.text = $"Pollen: {currentPollin} / {maxPollin}♣";
        float percent = (float)currentPollin / (float)maxPollin;
        StopAllCoroutines(); // avoid stacking multiple coroutines
        StartCoroutine(PollinLerp(percent));
        //Debug.Log("Updating visuals");
    }

    IEnumerator PollinLerp(float newPercent)
    {
        while (Mathf.Abs(backpakImage.fillAmount - newPercent) > 0.001f)
        {
            backpakImage.fillAmount = Mathf.Lerp(
                backpakImage.fillAmount,
                newPercent,
                Time.deltaTime * backpackFillLerpSpeed
            );
            yield return null; // wait one frame
        }
        backpakImage.fillAmount = newPercent; // snap to final value
    }
    #endregion
    #region ───────────────────────────── Menu & Options related Functions ───────────────────────────── 
    //───────────────────────────── Menu Toggle
    #region ───────────────────────────── Menu Toggle
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
    #endregion

    //───────────────────────────── Button Actions
    #region ───────────────────────────── Button Actions
    private void UI_OpenOptions() => optionsPanel.SetActive(true);

    private void UI_LogOut()
    {
        Debug.Log("Logging out...");
        Game_Manager.instance.LeaveServer(player.playerID); // replace with your actual function
        SceneManager.LoadSceneAsync(0);

    }

    #endregion
    //───────────────────────────── Player Control
    #region ───────────────────────────── Player Control
    private void ControllPlayerCamAndMove()
    {
        bool enableGameplay = !menuPanel.activeSelf;

        //player.GetComponent<PlayerMovemant>().enabled = enableGameplay;
        Camera.main.GetComponent<PlayerCamera>().disableCamRotation = enableGameplay;
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
    #endregion

    #region Interaction system
    // ───────────────────────────── Interaction system
    public void UI_ShowOrCloseInteractBox(bool state) =>
        interactionBox.gameObject.SetActive(state);

    public void UI_SetInteractText(string text) => interactedItemText.text = interactionText + text;
    #endregion
    #endregion
    #region ─────────────────────────────  Debug Button Fucnctions ───────────────────────────── 
    [Button("Test backpackLerp")]
    public void CallLerpFunction()
    {
        UI_UpdatePollin(10, 20);
    }
    [Button("Reset Backpack")]
    public void ResetBackpack()
    {
        UI_UpdatePollin(0, 20);
    }
    #endregion
}
