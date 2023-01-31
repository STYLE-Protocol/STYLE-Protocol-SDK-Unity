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
    public GLTFast.GltfAsset emodel;
    public Button buyBtn;
    public Loader loader;

    private int imgSize = 280;

    private string animationUrl;

    async private void OnClick(Dictionary<string, object> data)
    {
        loader.Show();
        string wallet = PlayerPrefs.GetString("Account");
        string resp = await SDK.approveERC20(wallet, data);
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
        loader.Unshow();
    }

    private void init(Dictionary<string, object> data)
    {
        buyBtn.onClick.AddListener(() => OnClick(data));

        Asset asset = (Asset)data["asset"];

        ename.text = asset.name;

        
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

        animationUrl = animation_url;

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
        if(request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError) 
            Debug.Log(request.error);
        else{
            Texture2D tex = ((DownloadHandlerTexture) request.downloadHandler).texture;
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            eimage.sprite = sprite;
        }
    }

    
    public Item(Dictionary<string, object> data)
    {
        init(data);
    }

    public void Initialize(Dictionary<string, object> data)
    {
        init(data);
    }

    public void OnMouseOver()
    {
        eimage.transform.localScale = new UnityEngine.Vector3(0, 0, 0);
        emodel.transform.localScale = new UnityEngine.Vector3(1, 1, 1);

        emodel.Url = animationUrl;
    }

    public void OnMouseExit()
    {
        eimage.transform.localScale = new UnityEngine.Vector3(1, 1, 1);
        emodel.transform.localScale = new UnityEngine.Vector3(0, 0, 0);

        emodel.Url = "";
    }
}