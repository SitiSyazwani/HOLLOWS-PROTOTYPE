using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("References")]
    private TMP_Text tmpText;
    private Button button;

    [Header("Settings")]
    [SerializeField] private bool shouldStaySelected = true;
    [SerializeField] private bool disableAllButtonsOnClick = false;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private float clickVolume = 1f;
    [SerializeField] private float actionDelay = 0.1f; // Delay before button action executes
    private static AudioSource globalAudioSource; // Static so it persists

    private string originalText;
    private bool isSelected = false;
    private bool isHovering = false;

    void Awake()
    {
        // Get the text component that's a DIRECT child
        tmpText = GetComponentInChildren<TMP_Text>();
        button = GetComponent<Button>();

        if (tmpText != null)
        {
            originalText = tmpText.text;
            Debug.Log($"Button '{gameObject.name}' initialized with text: '{originalText}'");
        }
        else
        {
            Debug.LogError($"Button '{gameObject.name}' has no TMP_Text child!");
        }

        // Setup a global audio source that won't be affected by button state
        if (globalAudioSource == null)
        {
            GameObject audioObj = new GameObject("ButtonAudioSource");
            globalAudioSource = audioObj.AddComponent<AudioSource>();
            globalAudioSource.playOnAwake = false;
            globalAudioSource.loop = false;
            DontDestroyOnLoad(audioObj); // Persist across scenes
        }
    }

    void Start()
    {
        // Add our click listener at the START of the frame
        // This ensures it runs BEFORE Inspector OnClick events
        if (button != null)
        {
            // Remove any existing listener first
            button.onClick.RemoveListener(OnButtonClick);
            // Add it at the beginning by inserting at index 0
            button.onClick.AddListener(OnButtonClick);
        }
    }

    void OnEnable()
    {
        ResetButton();
    }

    void Update()
    {
        // Force the text to stay correct every frame only if selected
        if (isSelected && tmpText != null)
        {
            string expectedText = $"<color=red><u>{originalText}</u></color>";
            if (tmpText.text != expectedText)
            {
                tmpText.text = expectedText;
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // This fires BEFORE Button.onClick events in the Inspector
        // Perfect timing to play the sound first!
        if (button != null && button.interactable)
        {
            PlayClickSound();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log($"Pointer ENTER on: {gameObject.name}");

        if (isSelected || (button != null && !button.interactable)) return;

        isHovering = true;
        UpdateTextDisplay();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log($"Pointer EXIT on: {gameObject.name}");

        if (isSelected || (button != null && !button.interactable)) return;

        isHovering = false;
        UpdateTextDisplay();
    }

    private void OnButtonClick()
    {
        Debug.Log($"Button CLICKED: {gameObject.name}");

        // Sound already played in OnPointerClick, so just handle the visual stuff
        if (shouldStaySelected)
        {
            isSelected = true;
            isHovering = false;
            UpdateTextDisplay();

            if (disableAllButtonsOnClick)
            {
                DisableAllMenuButtons();
            }
        }
    }

    private System.Collections.IEnumerator DelayedButtonAction()
    {
        yield return new WaitForSeconds(actionDelay);
        ExecuteButtonAction();
    }

    private void ExecuteButtonAction()
    {
        isSelected = true;
        isHovering = false;
        UpdateTextDisplay();

        if (disableAllButtonsOnClick)
        {
            DisableAllMenuButtons();
        }
    }

    private void PlayClickSound()
    {
        if (clickSound == null)
        {
            // No sound assigned, skip silently
            return;
        }

        if (globalAudioSource != null)
        {
            globalAudioSource.PlayOneShot(clickSound, clickVolume);
            Debug.Log($"Playing click sound on: {gameObject.name}");
        }
    }

    private void DisableAllMenuButtons()
    {
        Button[] allButtons = FindObjectsOfType<Button>();
        foreach (Button btn in allButtons)
        {
            btn.interactable = false;
        }
    }

    private void UpdateTextDisplay()
    {
        if (tmpText == null)
        {
            Debug.LogError($"tmpText is null on {gameObject.name}!");
            return;
        }

        if (isSelected)
        {
            tmpText.text = $"<color=red><u>{originalText}</u></color>";
            Debug.Log($"{gameObject.name} -> RED & UNDERLINED");
        }
        else if (isHovering)
        {
            tmpText.text = $"<u>{originalText}</u>";
            Debug.Log($"{gameObject.name} -> UNDERLINED");
        }
        else
        {
            tmpText.text = originalText;
            Debug.Log($"{gameObject.name} -> NORMAL");
        }
    }

    public void ResetButton()
    {
        Debug.Log($"RESET Button: {gameObject.name}");
        isSelected = false;
        isHovering = false;
        UpdateTextDisplay();
    }
}