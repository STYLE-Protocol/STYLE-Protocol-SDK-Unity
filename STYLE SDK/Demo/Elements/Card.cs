using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Accessibility;
using UnityEngine.Networking;
using UnityEngine.EventSystems;

public class Card : MonoBehaviour
{
    public TMPro.TMP_Text ename;
    public TMPro.TMP_Text edesc;
    public Button buyBtn;

    async private void OnClick(Dictionary<string, object> data)
    {
        string wallet = PlayerPrefs.GetString("Account");
        string resp = await SDK.approveERC20(wallet, data);
        print(resp);
        if (string.Equals(resp, "true"))
        {
            print("approved");
            resp = await SDK.buyAndMintItem(wallet, data);
            print(resp);
            if (string.Equals(resp, "true"))
            {
                print("bought");
            } else
            {
                print("not bought");
            }
        } else
        {
            print("not approved");
        }
    }

    private void init(string name, string desc, Dictionary<string, object> data)
    {
        buyBtn.onClick.AddListener(() => OnClick(data));
        ename.text = name;
        edesc.text = desc;
    }

    public Card(string name, string desc, Dictionary<string, object> data)
    {
        init(name, desc, data);
    }

    public void Initialize(string name, string desc, Dictionary<string, object> data)
    {
        init(name, desc, data);
        this.transform.localScale = new Vector3(1, 1, 1);
    }
}