using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.Assertions;
using System.Threading.Tasks;

public class ItemProfile : MonoBehaviour
{
    public TMPro.TMP_Text ename;
    public Image eimage;
    private GameObject scroll;
    private Loader loader;
    private Camera cameraMain;

    private string animationUrl;

    private void init(Dictionary<string, object> data, Loader _loader, GameObject _scroll, Camera _cameraMain)
    {
        loader = _loader;
        scroll = _scroll;
        cameraMain = _cameraMain;

        Asset asset = (Asset)data["asset"];

        ename.text = asset.name;

        string animation_url = asset.animation_url;
        if (String.Equals(animation_url.Substring(0, 4), "ipfs"))
        {
            animation_url = String.Format("https://{0}/ipfs/{1}", Constants.GATEWAY, animation_url.Substring(7));
        }
        string image = asset.image;
        if (String.Equals(image.Substring(0, 4), "ipfs"))
        {
            image = String.Format("https://{0}/ipfs/{1}", Constants.GATEWAY, image.Substring(7));
        }

        animationUrl = animation_url;

        StartCoroutine(LoadImage(image)); 
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

    public void Initialize(Dictionary<string, object> data, Loader _loader, GameObject _scroll, Camera _cameraMain)
    {
        init(data, _loader, _scroll, _cameraMain);
    }
}