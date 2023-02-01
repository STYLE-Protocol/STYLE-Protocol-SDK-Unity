using System.Threading.Tasks;
using System.Numerics;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Accessibility;
using UnityEngine.Networking;

//Interacting with blockchain
public class service : MonoBehaviour
{
    public Item item;
    public Loader loader;

    async private void  Start()
    {
        loader.Show();

        List<Dictionary<string, object>> requestedNFTs = await SDK.getRequestedNFTsAPI(new List<string>(), new List<string>(), new List<string>());

        for (int i = 0; i < requestedNFTs.Count; i++)
        {
            Dictionary<string, object> data = requestedNFTs[i];

            Item _item = Instantiate(item, transform);
            _item.Initialize(data, loader);
        }

        loader.Unshow();
    }
}