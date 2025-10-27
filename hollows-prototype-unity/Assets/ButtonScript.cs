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
        tmpText = GetComponentInChildren<TMP_Text>();
        button = GetComponent<Button>();
        
        if (tmpText != null)
        {
            originalText = tmpText.text;
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
        // Force the text to stay correct every frame
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
        // Don't show hover effect if button is disabled or already selected
        if (isSelected || (button != null && !button.interactable)) return;
        
        isHovering = true;
        UpdateTextDisplay();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Don't update if button is disabled or already selected
        if (isSelected || (button != null && !button.interactable)) return;
        
        isHovering = false;
        UpdateTextDisplay();
    }

    private void OnButtonClick()
    {
        if (shouldStaySelected)
        {
            isSelected = true;
            isHovering = false;
            UpdateTextDisplay();
            
            // Disable all buttons in the menu
            if (disableAllButtonsOnClick)
            {
                DisableAllMenuButtons();
            }
        }
    }

    private void DisableAllMenuButtons()
    {
        // Find all buttons in the scene and disable them
        Button[] allButtons = FindObjectsOfType<Button>();
        foreach (Button btn in allButtons)
        {
            btn.interactable = false;
        }
    }

    private void UpdateTextDisplay()
    {
        if (tmpText == null) return;

        if (isSelected)
        {
            tmpText.text = $"<color=red><u>{originalText}</u></color>";
        }
        else if (isHovering)
        {
            tmpText.text = $"<u>{originalText}</u>";
        }
        else
        {
            tmpText.text = originalText;
        }
    }

    public void ResetButton()
    {
        isSelected = false;
        isHovering = false;
        UpdateTextDisplay();
    }
}