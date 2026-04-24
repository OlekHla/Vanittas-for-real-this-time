using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelTransitionManager : MonoBehaviour
{
    public static LevelTransitionManager Instance;

    [SerializeField] private CanvasGroup fadeCanvasGroup;

    [SerializeField] private float fadeOutDuration = .6f;
    [SerializeField] private float blackScreenDuration = .4f;
    [SerializeField] private float fadeInDuration = .6f;
    [SerializeField] private float playerLockAfterTransitionDuration = .4f;

    private Boolean isTransitioning = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = false;
        }
    }

    public void TransitionToScene(string sceneName, string spawnPointId)
    {
        if (isTransitioning == true)
        {
            return;
        }

        StartCoroutine(Transition(sceneName, spawnPointId));
    }

    private IEnumerator Transition(string sceneName, string spawnPointId)
    {
        isTransitioning = true;

        Player player = UnityEngine.Object.FindFirstObjectByType<Player>();

        if (player != null)
        {
            player.SetControlsEnabled(false);
        }

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.blocksRaycasts = true;
        }

        yield return Fade(0f, 1f, fadeOutDuration);

        yield return new WaitForSeconds(blackScreenDuration);

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        while (operation.isDone == false)
        {
            yield return null;
        }

        player = UnityEngine.Object.FindFirstObjectByType<Player>();

        if (player != null)
        {
            player.SetControlsEnabled(false);

            SpawnPoint spawnPoint = FindSpawnPoint(spawnPointId);

            if (spawnPoint != null)
            {
                player.transform.position = spawnPoint.transform.position;
            }
            else
            {
                Debug.LogWarning("Could not find SpawnPoint with id: " + spawnPointId);
            }
        }

        yield return Fade(1f, 0f, fadeInDuration);

        yield return new WaitForSeconds(playerLockAfterTransitionDuration);

        if (player != null)
        {
            player.SetControlsEnabled(true);
        }

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.blocksRaycasts = false;
        }

        isTransitioning = false;
    }

    private IEnumerator Fade(float startAlpha, float targetAlpha, float duration)
    {
        if (fadeCanvasGroup == null)
        {
            yield break;
        }

        float timer = 0f;

        while (timer < duration)
        {
            yield return null;

            timer += Time.deltaTime;

            float t = timer / duration;
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
        }

        fadeCanvasGroup.alpha = targetAlpha;
    }

    private SpawnPoint FindSpawnPoint(string spawnPointId)
    {
        SpawnPoint[] spawnPoints = UnityEngine.Object.FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);

        foreach (SpawnPoint spawnPoint in spawnPoints)
        {
            if (spawnPoint.SpawnPointId == spawnPointId)
            {
                return spawnPoint;
            }
        }

        return null;
    }
}