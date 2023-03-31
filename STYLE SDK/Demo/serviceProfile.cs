using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

//Interacting with blockchain
public class serviceProfile : MonoBehaviour
{
    public ItemProfile item;
    public GameObject items;
    public Loader loader;
    public GameObject scroll;
    public Camera cameraMain;

    async private void  Start()
    {
        loader.Show();

        print(JsonConvert.SerializeObject(await STYLE_SDK.getContractsData()));

        Dictionary<string, string> userProof = await STYLE_SDK.getUserProof();

        List<Dictionary<string, object>> ownedDerivatives = await STYLE_SDK.getOwnedDerivatives(userProof);

        Debug.Log(JsonConvert.SerializeObject(ownedDerivatives));
        for (int i = 0; i < ownedDerivatives.Count; i++)
        {
            Dictionary<string, object> data = ownedDerivatives[i];

            ItemProfile _item = Instantiate(item, items.transform);
            _item.Initialize(data, loader, scroll, cameraMain);
        }

        loader.Unshow();
    }
}