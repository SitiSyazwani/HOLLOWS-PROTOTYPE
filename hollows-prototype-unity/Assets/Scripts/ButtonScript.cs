using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("References")]
    private TMP_Text tmpText;
    private Button button;

    [Header("Settings")]
    [SerializeField] private bool shouldStaySelected = true;
    [SerializeField] private bool disableAllButtonsOnClick = false;

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

        if (button != null)
        {
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