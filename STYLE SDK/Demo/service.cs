using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

//Interacting with blockchain
public class service : MonoBehaviour
{
    public Item item;
    public GameObject items;
    public Loader loader;
    public GameObject scroll;
    public Camera cameraMain;

    async private void  Start()
    {
        loader.Show();

        print(JsonConvert.SerializeObject(await STYLE_SDK.getContractsData()));

        List<string> metaverseFilter = new List<string>();
        List<string> typeFilter = new List<string>();
        List<string> subtypeFilter = new List<string>();
        List<Dictionary<string, object>> requestedNFTs = await STYLE_SDK.getRequestedNFTs(metaverseFilter, typeFilter, subtypeFilter);

        for (int i = 0; i < requestedNFTs.Count; i++)
        {
            Dictionary<string, object> data = requestedNFTs[i];

            Item _item = Instantiate(item, items.transform);
            _item.Initialize(data, loader, scroll, cameraMain);
        }

        loader.Unshow();
    }
}