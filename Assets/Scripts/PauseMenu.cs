using UnityEngine;

public class PauseMenu : MonoBehaviour
{
  public void closePauseMenu()
    {
        LevelTransitionManager.Instance.TransitionToScene("MainMenu", "fromMenu");
    }
}
