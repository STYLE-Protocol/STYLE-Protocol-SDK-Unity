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

    async private void  Start()
    {
        print("here");
        List<Dictionary<string, object>> requestedNFTs = await SDK.getRequestedNFTsAPI(new List<string>(), new List<string>(), new List<string>());
        print(requestedNFTs.Count.ToString());
        print(JsonConvert.SerializeObject(requestedNFTs));
        Dictionary<string, object> NFT = requestedNFTs[0];
        print(JsonConvert.SerializeObject(NFT));

        
        for (int i = 0; i < requestedNFTs.Count; i++)
        {
            Dictionary<string, object> data = requestedNFTs[i];

            Item _item = Instantiate(item, transform);
            _item.Initialize(String.Format("{0}", i), JsonConvert.SerializeObject(data), data);
            //_item.scale = new Vector3(1, 1, 1);
        }
    }
}