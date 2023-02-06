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

    private GameObject model;
    private GameObject modelContainer;
    private GameObject scroll;
    private Loader loader;
    private Camera cameraMain;
    private TrackballCamera cameraModel;

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

    private void init(Dictionary<string, object> data, Loader _loader, GameObject _scroll, GameObject _modelContainer, GameObject _model, Camera _cameraMain, TrackballCamera _cameraModel)
    {
        loader = _loader;
        scroll = _scroll;
        modelContainer = _modelContainer;
        model = _model;
        cameraMain = _cameraMain;
        cameraModel = _cameraModel;

        buyBtn.onClick.AddListener(() => OnClick(data));

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

        
        EventTrigger trigger = eimage.GetComponent<EventTrigger>( );
		EventTrigger.Entry entry = new EventTrigger.Entry( );
		entry.eventID = EventTriggerType.PointerClick;
		entry.callback.AddListener( (_) => OnPointerEnter() );
		trigger.triggers.Add( entry );
        
        //EventTrigger.Entry entry2 = new EventTrigger.Entry( );
		//entry2.eventID = EventTriggerType.PointerExit;
		//entry2.callback.AddListener( (_) => OnPointerExit() );
		//trigger.triggers.Add( entry2 );

        //EventTrigger obj = gameObject.GetComponent<EventTrigger>();
        //obj.onPointerEnter.AddListener(() => OnPointerEnter(data));
        //obj.onPointerExit.AddListener(() => OnPointerExit(data));
        
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

    public void Initialize(Dictionary<string, object> data, Loader _loader, GameObject _scroll, GameObject _modelContainer, GameObject _model, Camera _cameraMain, TrackballCamera _cameraModel)
    {
        init(data, _loader, _scroll, _modelContainer, _model, _cameraMain, _cameraModel);
    }

    
    public async void OnPointerEnter()
    {   
        loader.Show();
        
        var gltf = new GLTFast.GltfImport();
        var settings = new GLTFast.ImportSettings {
            GenerateMipMaps = true,
            AnisotropicFilterLevel = 3,
            NodeNameMethod = GLTFast.NameImportMethod.OriginalUnique
        };
        var success = await gltf.Load(animationUrl, settings);

        if (success) {
            //var gameObjectT = new GameObject("glTF");
            scroll.transform.localScale = new UnityEngine.Vector3(0, 0, 0);
            modelContainer.transform.localScale = new UnityEngine.Vector3(1, 1, 1);
            cameraMain.enabled = false;
            cameraModel.GetComponent<Camera>().enabled = true;
            await gltf.InstantiateMainSceneAsync(model.transform);
            GLTFast_onLoadComplete(model);
        }
        else {
            Debug.LogError("Loading glTF failed!");
        }
        
        loader.Unshow();
    }

    void GLTFast_onLoadComplete(GameObject asset) {
        var bounds = CalculateLocalBounds(asset.transform);
        cameraModel.SetTarget(bounds);
    }

    public Bounds CalculateLocalBounds(Transform transform)
    {
        Quaternion currentRotation = transform.rotation;
        transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        var rends = transform.GetComponentsInChildren<Renderer>();
        
        if (rends.Length < 1) return new Bounds(Vector3.zero, Vector3.one);
        
        Bounds bounds = new Bounds(rends[0].bounds.center, Vector3.zero);
        foreach (Renderer renderer in rends)
        {
            bounds.Encapsulate(renderer.bounds);
        }
        Vector3 localCenter = bounds.center - transform.position;
        bounds.center = localCenter;
        transform.rotation = currentRotation;
        return bounds;
    }

    /*
    public void OnPointerExit()
    {
        eimage.transform.localScale = new UnityEngine.Vector3(1, 1, 1);
        emodel.transform.localScale = new UnityEngine.Vector3(0, 0, 0);
    }
    */
}