using System;
using System.Collections.Generic;
using UnityEngine;

public class AbilityChoiceManager : MonoBehaviour
{
    public static Boolean HasChoiceBeenMade = false;
    public static SamuraiAbility PlayerAbility = SamuraiAbility.None;
    public static SamuraiAbility BossAbility = SamuraiAbility.None;

    [Header("Postacie")]
    [SerializeField] private Player player;

    [Header("Panel wyboru")]
    [SerializeField] private GameObject choicePanel;
    [SerializeField] private Boolean showChoicePanelOnStart = false;

    [Header("Umiejêtnoœci do wyboru")]
    [SerializeField] private SamuraiAbility firstAbility = SamuraiAbility.Dash;
    [SerializeField] private SamuraiAbility secondAbility = SamuraiAbility.DoubleJump;

    [Header("Losowanie")]
    [SerializeField] private Boolean randomizeChoicesOnStart = false;

    [SerializeField]
    private List<SamuraiAbility> availableAbilities = new List<SamuraiAbility>()
    {
        SamuraiAbility.Dash,
        SamuraiAbility.DoubleJump
    };

    public Boolean IsChoicePanelOpen
    {
        get
        {
            if (choicePanel == null)
            {
                return false;
            }

            return choicePanel.activeSelf;
        }
    }

    void Start()
    {
        FindPlayer();

        if (randomizeChoicesOnStart == true)
        {
            RollRandomAbilities();
        }

        if (choicePanel != null)
        {
            choicePanel.SetActive(false);
        }

        if (showChoicePanelOnStart == true)
        {
            OpenChoicePanel();
        }
    }

    private void FindPlayer()
    {
        if (player == null)
        {
            player = UnityEngine.Object.FindFirstObjectByType<Player>();
        }
    }

    private void RollRandomAbilities()
    {
        if (availableAbilities.Count < 2)
        {
            Debug.LogWarning("Not enough abilities to randomize choices.");
            return;
        }

        int firstIndex = UnityEngine.Random.Range(0, availableAbilities.Count);
        int secondIndex = firstIndex;

        while (secondIndex == firstIndex)
        {
            secondIndex = UnityEngine.Random.Range(0, availableAbilities.Count);
        }

        firstAbility = availableAbilities[firstIndex];
        secondAbility = availableAbilities[secondIndex];
    }

    public void OpenChoicePanel()
    {
        FindPlayer();

        if (choicePanel != null)
        {
            choicePanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Choice Panel nie jest podpiêty w AbilityChoiceManager.");
        }

        if (player != null)
        {
            player.SetControlsEnabled(false);
        }
    }

    public void ChooseFirstAbility()
    {
        ChooseAbility(firstAbility);
    }

    public void ChooseSecondAbility()
    {
        ChooseAbility(secondAbility);
    }

    public void ChooseDash()
    {
        ChooseAbility(SamuraiAbility.Dash);
    }

    public void ChooseDoubleJump()
    {
        ChooseAbility(SamuraiAbility.DoubleJump);
    }

    public void ChooseAbility(SamuraiAbility chosenAbility)
    {
        FindPlayer();

        SamuraiAbility otherAbility = GetOtherAbility(chosenAbility);

        PlayerAbility = chosenAbility;
        BossAbility = otherAbility;
        HasChoiceBeenMade = true;

        if (player != null)
        {
            player.ClearAbilities();
            player.AddAbility(PlayerAbility);
            player.SetControlsEnabled(true);
        }

        if (choicePanel != null)
        {
            choicePanel.SetActive(false);
        }

        Debug.Log("Wybrana umiejêtnoœæ gracza: " + PlayerAbility);
        Debug.Log("Umiejêtnoœæ bossa zapisana na scenê boss fight: " + BossAbility);
    }

    private SamuraiAbility GetOtherAbility(SamuraiAbility chosenAbility)
    {
        if (chosenAbility == firstAbility)
        {
            return secondAbility;
        }

        if (chosenAbility == secondAbility)
        {
            return firstAbility;
        }

        return SamuraiAbility.None;
    }

    public static void ApplySavedAbilitiesToPlayer(Player player)
    {
        if (HasChoiceBeenMade == false)
        {
            return;
        }

        if (player == null)
        {
            return;
        }

        player.ClearAbilities();
        player.AddAbility(PlayerAbility);
    }

    public static void ApplySavedAbilitiesToBoss(Boss boss)
    {
        if (HasChoiceBeenMade == false)
        {
            return;
        }

        if (boss == null)
        {
            return;
        }

        boss.ClearAbilities();
        boss.AddAbility(BossAbility);
    }
}