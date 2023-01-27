using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

//Interacting with blockchain
public class service : MonoBehaviour
{
    async private void  Start()
    {
        print("here");
        List<Dictionary<string, object>> requestedNFTs = await SDK.getRequestedNFTsAPI(new List<string>(), new List<string>(), new List<string>());
        print(requestedNFTs.Count.ToString());
        print(JsonConvert.SerializeObject(requestedNFTs));
        Dictionary<string, object> NFT = requestedNFTs[0];
        print(JsonConvert.SerializeObject(NFT));
        //List<Dictionary<string, object>> listedNFTs = await SDK.getListedNFTs(new List<string>());
        //print(JsonConvert.SerializeObject(listedNFTs));
        string wallet = "0x67701e71F9412Af1BcB2D77897F40139B6Ccc073";
        string resp = await SDK.approveERC20(wallet, NFT);
        print(resp);
        if (string.Equals(resp, "true"))
        {
            print("approved");
            resp = await SDK.buyAndMintItem(wallet, NFT);
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
        //GetComponent<Text>().text = JsonConvert.SerializeObject(data);

    }
}