using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PanelController : MonoBehaviour
{
    public List<Panel> panels = new List<Panel>();

    public void ShowPanel(Panel panelToShow)
    {
        foreach (var panel in panels)
        {
            if (panelToShow == panel)
            {
                panel.gameObject.SetActive(true);
            }
            else
            {
                panel.gameObject.SetActive(false);
            }

            //panel.gameObject.SetActive(panelToShow == panel); <-- to samo co ca³y if else
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene(1);
    }

    public void ExitGame()
    {
        Application.Quit();
    }


}
