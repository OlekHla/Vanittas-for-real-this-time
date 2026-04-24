using System;
using System.Collections.Generic;
using UnityEngine;

public class AbilityChoiceManager : MonoBehaviour
{
    public static Boolean HasChoiceBeenMade = false;
    public static SamuraiAbility PlayerAbility = SamuraiAbility.None;
    public static SamuraiAbility BossAbility = SamuraiAbility.None;

    [SerializeField] private Player player;
    [SerializeField] private Boss boss;

    [SerializeField] private GameObject choicePanel;

    [SerializeField] private SamuraiAbility firstAbility = SamuraiAbility.Dash;
    [SerializeField] private SamuraiAbility secondAbility = SamuraiAbility.DoubleJump;

    [SerializeField] private Boolean randomizeChoicesOnStart = false;

    [SerializeField]
    private List<SamuraiAbility> availableAbilities = new List<SamuraiAbility>()
    {
        SamuraiAbility.Dash,
        SamuraiAbility.DoubleJump
    };

    void Start()
    {
        FindCharacters();

        if (randomizeChoicesOnStart == true)
        {
            RollRandomAbilities();
        }

        if (choicePanel != null)
        {
            choicePanel.SetActive(true);
        }

        if (player != null)
        {
            player.SetControlsEnabled(false);
        }
    }

    private void FindCharacters()
    {
        if (player == null)
        {
            player = UnityEngine.Object.FindFirstObjectByType<Player>();
        }

        if (boss == null)
        {
            boss = UnityEngine.Object.FindFirstObjectByType<Boss>();
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
        FindCharacters();

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

        if (boss != null)
        {
            boss.ClearAbilities();
            boss.AddAbility(BossAbility);
        }

        if (choicePanel != null)
        {
            choicePanel.SetActive(false);
        }

        Debug.Log("Player ability: " + PlayerAbility);
        Debug.Log("Boss ability: " + BossAbility);
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