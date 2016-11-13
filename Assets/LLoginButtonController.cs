using UnityEngine;
using System.Collections;

public class LLoginButtonController : MonoBehaviour {

    public GameObject UserNameInputFieldObject;
    public GameObject PasswordInputField;

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void OnClick()
    {
        string user = UserNameInputFieldObject.GetComponent<UnityEngine.UI.Text>().text;
        string pass = PasswordInputField.GetComponent<UnityEngine.UI.Text>().text;
        GameObject.Find("EverClient").GetComponent<EverClient>().Login(user,pass);
    }
}
