using UnityEngine;
using System.Collections;
using Firebase;
using Firebase.Auth;
using TMPro;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager instance;

    [Header("Firebase")]
    public FirebaseAuth auth;
    public FirebaseUser user;
    [Space(5f)]

    [Header("Login References")]
    [SerializeField]
    private TMP_InputField loginEmail;
    [SerializeField]
    private TMP_InputField loginPassword;
    [SerializeField]
    private TMP_Text loginOutputText;
    [Space(5f)]

    [Header("Register References")]
    [SerializeField]
    private TMP_InputField registerUsername;
    [SerializeField]
    private TMP_InputField registerEmail;
    [SerializeField]
    private TMP_InputField registerPassword;
    [SerializeField]
    private TMP_InputField registerConfirmPassword;
    [SerializeField]
    private TMP_Text registerOutputText;


    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(instance.gameObject);
            instance = this;
        }

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(checkDependencyTask => {
            var dependencyStatus = checkDependencyTask.Result;

            if (dependencyStatus == DependencyStatus.Available)
            {
                InitializeFirebase();
            }
            else
            {
                Debug.LogError($"Could not resolve all Firebase dependencies:  {dependencyStatus}");
            }

        });
    }

    private void InitializeFirebase()
    {
        auth = FirebaseAuth.DefaultInstance;

        auth.StateChanged += AuthStateChanged;
        AuthStateChanged(this, null);
    }

    private void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        if (auth.CurrentUser != user)
        {
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null; //this bool checks if we are signed in. Why do we need it? What's the point of login screen if we are already logged?

            if (!signedIn && user != null)
            {
                Debug.Log("Signed Out"); // Here you can put some code to be executed when user is signed out
            }

            user = auth.CurrentUser;

            if (signedIn)
            {
                Debug.Log($"Signed In: {user.DisplayName}"); // maybe here we should simply open the scene? 
            }
        }
    }

    public void ClearOutput()
    {
        loginOutputText.text = "";
        registerOutputText.text = "";
    }

    public void SignOutButton() //should I create a specific script here or this would be to specific? Maybe a cript to take care of all of the UI in second scene?
    {
        auth.SignOut();
        GameManager.instance.ChangeScene(0);
        loginOutputText.text = "Signed Out!";
    }

    public void LoginButton()
    {
        StartCoroutine(LoginLogic(loginEmail.text, loginPassword.text));
    }

    public void RegisterButton()
    {
        StartCoroutine(RegisterLogic(registerUsername.text, registerEmail.text, registerPassword.text, registerConfirmPassword.text));
    }
    private IEnumerator LoginLogic(string _email, string _password)
    {
        Credential credential = EmailAuthProvider.GetCredential(_email, _password);

        var loginTask = auth.SignInWithCredentialAsync(credential);

        yield return new WaitUntil(predicate: () => loginTask.IsCompleted);

        if (loginTask.Exception != null)
        {
            FirebaseException firebaseException = (FirebaseException)loginTask.Exception.GetBaseException();
            AuthError error = (AuthError)firebaseException.ErrorCode;
            string output = "Unknown error, please try again";

            switch (error)
            {
                case AuthError.MissingEmail:
                    output = "Please enter your Email";
                    break;
                case AuthError.MissingPassword:
                    output = "Please enter your Password";
                    break;
                case AuthError.InvalidEmail:
                    output = "Invalid Email";
                    break;
                case AuthError.WrongPassword:
                    output = "Incorrect Password";
                    break;
                case AuthError.UserNotFound:
                    output = "Account does not exist";
                    break;
            }
            loginOutputText.text = output;
        }
        else
        {
            if (user.IsEmailVerified) //if users email address is verified open lobby scene
            {
                yield return new WaitForSeconds(1f);
                GameManager.instance.ChangeScene(1);
            }
            else
            {
                //TODO: Display notification for users that did not verify their email
                user.SendEmailVerificationAsync();
                loginOutputText.text = "Please verify your email first";
            }
        }

    }
    private IEnumerator RegisterLogic(string _username, string _email, string _password, string _confirmPassword)
    {
        if (_username == "")
        {
            registerOutputText.text = "Please enter the username";
        }
        else if (_password != _confirmPassword)
        {
            registerOutputText.text = "Passwords do not match!";
        }
        else
        {
            var registerTask = auth.CreateUserWithEmailAndPasswordAsync(_email, _password);

            yield return new WaitUntil(predicate: () => registerTask.IsCompleted);

            if (registerTask.Exception != null)
            {
                FirebaseException firebaseException = (FirebaseException)registerTask.Exception.GetBaseException();
                AuthError error = (AuthError)firebaseException.ErrorCode;
                string output = "Unknown error, please try again";

                switch (error)
                {
                    case AuthError.InvalidEmail:
                        output = "Invalid Email";
                        break;
                    case AuthError.EmailAlreadyInUse:
                        output = "Email already in use";
                        break;
                    case AuthError.WeakPassword:
                        output = "Weak Password";
                        break;
                    case AuthError.MissingEmail:
                        output = "Please enter your email";
                        break;
                    case AuthError.MissingPassword:
                        output = "Please enter your password";
                        break;
                }
                registerOutputText.text = output;
            }
            else
            {
                UserProfile profile = new UserProfile
                {
                    DisplayName = _username,
                };

                var defaultUserTask = user.UpdateUserProfileAsync(profile);

                yield return new WaitUntil(predicate: () => defaultUserTask.IsCompleted);

                if (defaultUserTask.Exception != null)
                {
                    user.DeleteAsync(); //Delete user if updating to a default profile fails
                    FirebaseException firebaseException = (FirebaseException)defaultUserTask.Exception.GetBaseException();
                    AuthError error = (AuthError)firebaseException.ErrorCode;
                    string output = "Unknown error, please try again";

                    switch (error)
                    {
                        case AuthError.Cancelled:
                            output = "Update User Cancelled";
                            break;
                        case AuthError.SessionExpired:
                            output = "Session expired";
                            break;
                    }
                    registerOutputText.text = output;
                }
                else
                {
                    Debug.Log($"Firebase user created successfully: {user.DisplayName}({user.UserId})");

                    user.SendEmailVerificationAsync();
                }
            }
        }
    }

}