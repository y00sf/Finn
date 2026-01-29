using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class PlayerUIManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject journalPanel;
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Input Actions")]
    [SerializeField] private InputAction journalAction;
    [SerializeField] private InputAction inventoryAction;
    [SerializeField] private InputAction settingsAction;
    [SerializeField] private InputAction escapeAction;
    [SerializeField] private InputAction RestartGame;
    [SerializeField] private InputAction PixelGame;
    [SerializeField] private InputAction refillBaitsAction;
    [SerializeField] private string SceneName;
    [SerializeField] private UniversalRendererData URD;
    

    [Header("Scripts to Toggle")]
    [SerializeField] private FishingCaster fishingCaster;
    [SerializeField] private PlayerMovement playerInput;
    [SerializeField] private PlayerInteraction playerInteraction;

    private bool isAnyPanelOpen = false;
    private bool isPixelated = false;

    private void OnEnable()
    {
        journalAction?.Enable();
        inventoryAction?.Enable();
        settingsAction?.Enable();
        escapeAction?.Enable();
        RestartGame?.Enable();
        PixelGame?.Enable();
        refillBaitsAction?.Enable();

        journalAction.performed += OnJournalPressed;
        inventoryAction.performed += OnInventoryPressed;
        settingsAction.performed += OnSettingsPressed;
        escapeAction.performed += OnEscapePressed;
        RestartGame.performed += OnRestartPressed;
        PixelGame.performed += OnPixelGame;
        refillBaitsAction.performed += OnRefillBaitsPressed;
    }

    private void OnDisable()
    {
        journalAction.performed -= OnJournalPressed;
        inventoryAction.performed -= OnInventoryPressed;
        settingsAction.performed -= OnSettingsPressed;
        escapeAction.performed -= OnEscapePressed;
        RestartGame.performed -= OnRestartPressed;
        PixelGame.performed -= OnPixelGame;
        refillBaitsAction.performed -= OnRefillBaitsPressed;

        journalAction?.Disable();
        inventoryAction?.Disable();
        settingsAction?.Disable();
        escapeAction?.Disable();
        RestartGame?.Disable();
        PixelGame?.Disable();
        refillBaitsAction?.Disable();
    }

    private void OnRestartPressed(InputAction.CallbackContext ctx)
    {
        SceneManager.LoadScene(SceneName);
    }

    private void Start()
    {
        CloseAllPanels();
    }

    private void OnRefillBaitsPressed(InputAction.CallbackContext context)
    {
        if (FishingManager.Instance != null)
        {
            FishingManager.Instance.RefillAllBaits();
        }
    }

    private void OnJournalPressed(InputAction.CallbackContext context)
    {
        TogglePanel(journalPanel);
    }

    private void OnInventoryPressed(InputAction.CallbackContext context)
    {
        TogglePanel(inventoryPanel);
    }

    private void OnSettingsPressed(InputAction.CallbackContext context)
    {
        TogglePanel(settingsPanel);
    }

    private void OnEscapePressed(InputAction.CallbackContext context)
    {
        if (isAnyPanelOpen)
        {
            CloseAllPanels();
        }
    }

    private void OnPixelGame(InputAction.CallbackContext context)
    {
        TogglePixel();
    }

    private void TogglePixel()
    {
        if (URD == null) return;

        isPixelated = !isPixelated;
        URD.rendererFeatures[1].SetActive(isPixelated);
    }

    private void TogglePanel(GameObject panel)
    {
        if (panel == null) return;

        bool shouldOpen = !panel.activeSelf;

        CloseAllPanels();

        if (shouldOpen)
        {
            OpenPanel(panel);
        }
    }

    private void OpenPanel(GameObject panel)
    {
        if (panel == null) return;

        CloseAllPanels();
        panel.SetActive(true);
        isAnyPanelOpen = true;
        SetPlayerScriptsEnabled(false);
    }

    public void CloseAllPanels()
    {
        if (journalPanel != null) journalPanel.SetActive(false);
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);

        isAnyPanelOpen = false;
        SetPlayerScriptsEnabled(true);
    }

    private void SetPlayerScriptsEnabled(bool enabled)
    {
        if (fishingCaster != null) fishingCaster.enabled = enabled;
        if (playerInput != null) playerInput.enabled = enabled;
        if(playerInteraction != null) playerInteraction.enabled = enabled;
    }

    public bool IsAnyPanelOpen() => isAnyPanelOpen;
}