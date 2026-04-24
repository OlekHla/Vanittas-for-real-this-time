using UnityEngine;
using UnityEngine.UI;

public class ScrapManager : MonoBehaviour
{
    public static ScrapManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private Text scrapText;

    [Header("Z³om")]
    [SerializeField] private int scrapAmount = 0;
    [SerializeField] private string textPrefix = "Z³om: ";

    [Header("Wybór umiejêtnoœci za z³om")]
    [SerializeField] private AbilityChoiceManager abilityChoiceManager;
    [SerializeField] private int scrapNeededForAbilityChoice = 5;
    [SerializeField] private bool triggerAbilityChoiceOnlyOnce = true;
    [SerializeField] private bool consumeScrapWhenChoiceStarts = false;

    private bool abilityChoiceAlreadyTriggered = false;

    public int ScrapAmount
    {
        get { return scrapAmount; }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (abilityChoiceManager == null)
        {
            abilityChoiceManager = Object.FindFirstObjectByType<AbilityChoiceManager>();
        }

        RefreshUI();
        CheckAbilityChoiceThreshold();
    }

    public void AddScrap(int amount)
    {
        scrapAmount += amount;

        if (scrapAmount < 0)
        {
            scrapAmount = 0;
        }

        RefreshUI();
        CheckAbilityChoiceThreshold();
    }

    public void SetScrap(int amount)
    {
        scrapAmount = Mathf.Max(0, amount);
        RefreshUI();
        CheckAbilityChoiceThreshold();
    }

    private void CheckAbilityChoiceThreshold()
    {
        if (scrapAmount < scrapNeededForAbilityChoice)
        {
            return;
        }

        if (triggerAbilityChoiceOnlyOnce == true && abilityChoiceAlreadyTriggered == true)
        {
            return;
        }

        if (abilityChoiceManager == null)
        {
            Debug.LogWarning("Nie ma AbilityChoiceManager na scenie.");
            return;
        }

        if (abilityChoiceManager.IsChoicePanelOpen == true)
        {
            return;
        }

        abilityChoiceAlreadyTriggered = true;

        if (consumeScrapWhenChoiceStarts == true)
        {
            scrapAmount -= scrapNeededForAbilityChoice;

            if (scrapAmount < 0)
            {
                scrapAmount = 0;
            }

            RefreshUI();
        }

        abilityChoiceManager.OpenChoicePanel();
    }

    private void RefreshUI()
    {
        if (scrapText != null)
        {
            scrapText.text = textPrefix + scrapAmount;
        }
    }
}