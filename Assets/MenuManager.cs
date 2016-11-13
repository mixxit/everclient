using UnityEngine;
using System.Collections;

public class MenuManager : MonoBehaviour {

    public GameObject LoginPanel;
    public GameObject InitialisePanel;
    public GameObject ServerSelectPanel;

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void FallbackMessage(string message)
    {
        GoToInitialiseScreen();
        InitialisePanel.GetComponent<InitialisePanelManager>().GetComponentInChildren<UnityEngine.UI.Text>().text = message;
    }

    public void HideAllScreens()
    {
        ServerSelectPanel.SetActive(false);
        InitialisePanel.SetActive(false);
        LoginPanel.SetActive(false);
    }

    public void GoToLoginScreen()
    {
        HideAllScreens();
        LoginPanel.SetActive(true);
    }

    public void GoToInitialiseScreen()
    {
        HideAllScreens();
        InitialisePanel.SetActive(true);
    }
}
