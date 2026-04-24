using System;
using UnityEngine;

public class LevelDoor : MonoBehaviour
{
    [SerializeField] private string targetSceneName;
    [SerializeField] private string targetSpawnPointId;

    private Boolean hasBeenUsed = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasBeenUsed == true)
        {
            return;
        }

        Player player = other.GetComponentInParent<Player>();

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
}