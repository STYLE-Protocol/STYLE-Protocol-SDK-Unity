#if UNITY_WEBGL

using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Login : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void Web3Connect();

    [DllImport("__Internal")]
    private static extern string ConnectAccount();

    [DllImport("__Internal")]
    private static extern void SetConnectAccount(string value);

    private int expirationTime;
    private string account;
    ProjectConfigScriptableObject projectConfigSO = null;
    
    void Start()
    {
        // loads the data saved from the editor config
        projectConfigSO = (ProjectConfigScriptableObject)Resources.Load("ProjectConfigData", typeof(ScriptableObject));
        PlayerPrefs.SetString("ProjectID", projectConfigSO.ProjectID);
        PlayerPrefs.SetString("ChainID", projectConfigSO.ChainID);
        PlayerPrefs.SetString("Chain", projectConfigSO.Chain);
        PlayerPrefs.SetString("Network", projectConfigSO.Network);
        PlayerPrefs.SetString("RPC", projectConfigSO.RPC);
    }

    public void OnLogin()
    {
        Web3Connect();
        OnConnected();
    }

    async private void OnConnected()
    {
        account = ConnectAccount();
        while (account == "")
        {
            await new WaitForSeconds(1f);
            account = ConnectAccount();
        };
        // save account for next scene
        PlayerPrefs.SetString("Account", account);
        // reset login message
        SetConnectAccount("");
        // load next scene
        SceneManager.LoadScene("Demo");
    }

    public void OnLoginProfile()
    {
        Web3Connect();
        OnConnectedProfile();
    }

    async private void OnConnectedProfile()
    {
        account = ConnectAccount();
        while (account == "")
        {
            await new WaitForSeconds(1f);
            account = ConnectAccount();
        };
        // save account for next scene
        PlayerPrefs.SetString("Account", account);
        // reset login message
        SetConnectAccount("");
        // load next scene
        SceneManager.LoadScene("ProfileDemo");
    }

    public void OnSkip()
    {
        // burner account for skipped sign in screen
        PlayerPrefs.SetString("Account", "");
        // move to next scene
        SceneManager.LoadScene("Demo");
    }
}

#else

using System.Text;
using Nethereum.Signer;
using Nethereum.Util;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Web3Unity.Scripts.Library.Web3Wallet;

public class Login : MonoBehaviour
{
    public Toggle rememberMe;
    ProjectConfigScriptableObject projectConfigSO = null;
    private void Start()
    {
        // change this if you are implementing your own sign in page
        Web3Wallet.url = "https://chainsafe.github.io/game-web3wallet/";
        // loads the data saved from the editor config
        projectConfigSO = (ProjectConfigScriptableObject)Resources.Load("ProjectConfigData", typeof(ScriptableObject));
        PlayerPrefs.SetString("ProjectID", projectConfigSO.ProjectID);
        PlayerPrefs.SetString("ChainID", projectConfigSO.ChainID);
        PlayerPrefs.SetString("Chain", projectConfigSO.Chain);
        PlayerPrefs.SetString("Network", projectConfigSO.Network);
        PlayerPrefs.SetString("RPC", projectConfigSO.RPC);
        // if remember me is checked, set the account to the saved account
        if (PlayerPrefs.HasKey("RememberMe") && PlayerPrefs.HasKey("Account"))
            if (PlayerPrefs.GetInt("RememberMe") == 1 && PlayerPrefs.GetString("Account") != "")
            {
                // move to next scene
                //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            }
    }
    public async void OnLogin()
    {
        // get current timestamp
        var timestamp = (int)System.DateTime.UtcNow.Subtract(new System.DateTime(1970, 1, 1)).TotalSeconds;
        // set expiration time
        var expirationTime = timestamp + 60;
        // set message
        var message = expirationTime.ToString();
        // sign message
        var signature = await Web3Wallet.Sign(message);
        // verify account
        var account = SignVerifySignature(signature, message);
        var now = (int)System.DateTime.UtcNow.Subtract(new System.DateTime(1970, 1, 1)).TotalSeconds;
        // validate
        if (account.Length == 42 && expirationTime >= now)
        {
            // save account
            PlayerPrefs.SetString("Account", account);
            if (rememberMe.isOn)
                PlayerPrefs.SetInt("RememberMe", 1);
            else
                PlayerPrefs.SetInt("RememberMe", 0);
            print("Account: " + account);
            // load next scene
            SceneManager.LoadScene("Demo");
        }
    }

     public async void OnLoginProfile()
    {
        // get current timestamp
        var timestamp = (int)System.DateTime.UtcNow.Subtract(new System.DateTime(1970, 1, 1)).TotalSeconds;
        // set expiration time
        var expirationTime = timestamp + 60;
        // set message
        var message = expirationTime.ToString();
        // sign message
        var signature = await Web3Wallet.Sign(message);
        // verify account
        var account = SignVerifySignature(signature, message);
        var now = (int)System.DateTime.UtcNow.Subtract(new System.DateTime(1970, 1, 1)).TotalSeconds;
        // validate
        if (account.Length == 42 && expirationTime >= now)
        {
            // save account
            PlayerPrefs.SetString("Account", account);
            if (rememberMe.isOn)
                PlayerPrefs.SetInt("RememberMe", 1);
            else
                PlayerPrefs.SetInt("RememberMe", 0);
            print("Account: " + account);
            // load next scene
            SceneManager.LoadScene("ProfileDemo");
        }
    }

    public string SignVerifySignature(string signatureString, string originalMessage)
    {
        var msg = "\x19" + "Ethereum Signed Message:\n" + originalMessage.Length + originalMessage;
        var msgHash = new Sha3Keccack().CalculateHash(Encoding.UTF8.GetBytes(msg));
        var signature = MessageSigner.ExtractEcdsaSignature(signatureString);
        var key = EthECKey.RecoverFromSignature(signature, msgHash);
        return key.GetPublicAddress();
    }
}

#endif
