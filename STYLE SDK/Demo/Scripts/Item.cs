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
using System.Collections;

public class Item : MonoBehaviour
{
    public TMPro.TMP_Text ename;
    public Image eimage;
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

        Asset asset = (Asset)data["asset"];
        string animation_url = asset.animation_url;
        if (String.Equals(animation_url.Substring(0, 4), "ipfs"))
        {
            animation_url = String.Format("https://{0}/ipfs/{1}", Constants.GATEWAY, animation_url.Substring(7));
        }
        print(animation_url);
        string image = asset.image;
        if (String.Equals(image.Substring(0, 4), "ipfs"))
        {
            image = String.Format("https://{0}/ipfs/{1}", Constants.GATEWAY, image.Substring(7));
        }
        print(image);

        StartCoroutine(LoadImage(image));
        /*
        var gltf = new GLTFast.GltfImport();

        gltf.Load(animation_url).ContinueWith((task) => {
            if (task.Result) {
                var obj = gameObject.GetComponent("Model") as GLTFast.GltfAsset;
                gltf.InstantiateMainSceneAsync(obj.gameObject.transform);
            }
            else {
                Debug.LogError("Loading glTF failed!");
            }
        });
        */
    }

    IEnumerator LoadImage(string image)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(image);
        yield return request.SendWebRequest();
        if(request.isNetworkError || request.isHttpError) 
            Debug.Log(request.error);
        else{
            Texture2D tex = ((DownloadHandlerTexture) request.downloadHandler).texture;
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            eimage.sprite = sprite;
        }
    }

    
    public Item(string name, string desc, Dictionary<string, object> data)
    {
        init(name, desc, data);
    }

    public void Initialize(string name, string desc, Dictionary<string, object> data)
    {
        init(name, desc, data);
    }
}