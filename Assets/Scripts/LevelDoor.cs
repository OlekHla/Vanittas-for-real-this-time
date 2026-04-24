using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelDoor : MonoBehaviour
{
    [SerializeField] private string targetSceneName;
    [SerializeField] private string targetSpawnPointId;
    public GameObject inputThingie;
    public SpriteRenderer SpriteRenderer;
    public Sprite openDoor;

    public Player plr;
    
    private Boolean hasBeenUsed = false;
    private Boolean open = false;

    public void CheckResult(TMP_InputField input)
    {
        Debug.Log(input.text);
        if(input.text == "7" || input.text.ToLower() == "siedem")
        {
            HideInput();
            SpriteRenderer.sprite = openDoor;
            open = true;
            Transport(plr);
        }
        else
        {
            Debug.Log("u stupi");
        }
    }

    public void ShowInput()
    {
        inputThingie.SetActive(true);
    }
    public void HideInput()
    {
        inputThingie.SetActive(false);
    }

    void Transport(Player player)
    {
        if (player == null)
        {
            return;
        }

        if (LevelTransitionManager.Instance == null)
        {
            Debug.LogError("There is no LevelTransitionManager in the scene.");
            return;
        }

        hasBeenUsed = true;
        LevelTransitionManager.Instance.TransitionToScene(targetSceneName, targetSpawnPointId);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (open == false) 
        {
            if (!inputThingie.activeSelf)
            {
                ShowInput();
            }
            return;
        }
        if (hasBeenUsed == true)
        {
            return;
        }

        Player player = other.GetComponentInParent<Player>();
        Transport(player);
    }
}