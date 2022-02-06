using UnityEngine;
using TMPro;

public class AuthUIManager : MonoBehaviour
{
    public static AuthUIManager instance; //check what this code does - why static?

    [Header("References")]
    [SerializeField]
    private GameObject checkingForAccountUI;
    [SerializeField]
    private GameObject loginUi;
    [SerializeField]
    private GameObject registerUI;
    [SerializeField]
    private GameObject verifyEmailUI;
    [SerializeField]
    private TMP_Text verifyEmailText;


    private void Awake() //signleton pattern - learn how it works
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    private void ClearUI()
    {
        loginUi.SetActive(false);
        registerUI.SetActive(false);
        FirebaseManager.instance.ClearOutput();
    }

    public void LoginScreen()
    {
        ClearUI();
        loginUi.SetActive(true);
    }
    public void RegisterScreen()
    {
        ClearUI();
        registerUI.SetActive(true);
    }

    public void LoginScreenButtonTest()
    {
        Debug.Log("Login");
    }
}