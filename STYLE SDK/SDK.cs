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

public class Payment
{
    public string stringValue;
    public BigInteger value;
}

public class PaymentToken
{
    public string address;
    public string name;
    public string symbol;
}

public class Asset
{
    public string name;
    public string description;
    public string image;
    public string animation_url;
    public string extraData;
}

public class SDK
{    
    async public static Task<List<Dictionary<string, object>>> getRequestedNFTsAPI(
        List<string> metaverseFilter,
        List<string> typeFilter,
        List<string> subtypeFilter,
        int cursor = 0,
        int amount = 100,
        int chainId = 5
    )
    {
        try {
            
            string url = String.Format(@"https://{0}/api/nfts/get-requested-nfts?endpoint={1}", Constants.API_HOST, Secrets.ENDPOINTS[chainId]);
            JArray requestedNFTs;
            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            {
                await webRequest.SendWebRequest();
                
                if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.Log(webRequest.error);
                    return null;
                }
                
                string res = webRequest.downloadHandler.text;
                requestedNFTs = JArray.Parse(res);
            }

            List<Dictionary<string, object>> allDataParsed = new List<Dictionary<string, object>>();
            foreach (JToken cur in requestedNFTs)
            {
                Dictionary<string, object> data = new Dictionary<string, object>();
                foreach (KeyValuePair<string, JToken> keyValuePair in (JObject)cur)
                {
                    data[keyValuePair.Key] = keyValuePair.Value;
                }

                Payment payment = new Payment();
                payment.value = BigInteger.Parse((string)cur["payment"]["value"]);
                payment.stringValue = (string)cur["payment"]["stringValue"];
                data["payment"] = payment;

                PaymentToken paymentToken = new PaymentToken();
                paymentToken.name = (string)cur["paymentToken"]["name"];
                paymentToken.symbol = (string)cur["paymentToken"]["symbol"];
                paymentToken.address = (string)cur["paymentToken"]["address"];
                data["paymentToken"] = paymentToken;

                Asset asset = new Asset();
                asset.name = (string)cur["asset"]["name"];
                asset.description = (string)cur["asset"]["description"];
                asset.image = (string)cur["asset"]["image"];
                asset.animation_url = (string)cur["asset"]["animation_url"];
                if (cur["asset"]["extraData"] != null) {
                    asset.extraData = cur["asset"]["extraData"].ToString();
                }
                data["asset"] = asset;

                allDataParsed.Add(data);
            }

            return allDataParsed;
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
            return null;
        }
    }

    async public static Task<string> approveERC20(
        string walletAddress,
        Dictionary<string, object> NFT,
        int chainId = 5
    )
    {
        try
        {
            string ERC20_ABI = Resources.Load<TextAsset>("ERC20_ABI").text;

            PaymentToken paymentToken = (PaymentToken)NFT["paymentToken"];

            Payment payment = (Payment)NFT["payment"];
            BigInteger paymentValue = payment.value;

            string[] args = { walletAddress, Constants.PROTOCOL_CONTRACTS[chainId] };
            string allowanceStr = await EVM.Call(
                Constants.chainId2Chain[chainId],
                Constants.chainId2Network[chainId],
                paymentToken.address,
                ERC20_ABI,
                "allowance",
                JsonConvert.SerializeObject(args)
            );

            BigInteger allowance = BigInteger.Parse(allowanceStr);

            if (paymentValue > allowance)
            {
                string[] argsNew = { Constants.PROTOCOL_CONTRACTS[chainId], paymentValue.ToString() };
                string methodID = await Web3GL.SendContract(
                    "approve",
                    ERC20_ABI,
                    paymentToken.address,
                    JsonConvert.SerializeObject(argsNew),
                    "0",
                    "",
                    ""
                );

                while (true)
                {
                    string status = await EVM.TxStatus(
                    Constants.chainId2Chain[chainId],
                    Constants.chainId2Network[chainId],
                    methodID
                );

                    if (status == "success")
                    {
                        break;
                    }
                    await new WaitForSeconds(1);
                }
            }
            return "true";
        }
        catch (Exception e)
        {
            return e.ToString();
        }
    }

    struct NonmintedNFT
    {   
        public string tokenId;
        public string payment;
        public string tokenAddress;
        public string metaverseId;
        public string paymentToken;
        public string modelId;
        public string bidder;
        public string environment;
        public string uri;
        public string signature;
    }

    async public static Task<string> buyAndMintItem(
        string walletAddress,
        Dictionary<string, object> NFT,
        int chainId = 5
    )
    {
        try
        {
            JObject NFTMarketplace_metadata = JObject.Parse(Resources.Load<TextAsset>("NFTMarketplace_metadata").text);

            PaymentToken paymentToken = (PaymentToken)NFT["paymentToken"];

            Payment payment = (Payment)NFT["payment"];
            BigInteger paymentValue = payment.value;

            string[] subArgs = {
                
                NFT["tokenId"].ToString(),
                paymentValue.ToString(),
                NFT["tokenAddress"].ToString(),
                NFT["metaverseId"].ToString(),
                paymentToken.address,
                NFT["modelId"].ToString(),
                NFT["bidder"].ToString(),
                NFT["environment"].ToString(),
                NFT["uri"].ToString(),
                NFT["signature"].ToString()
            };

            object[] args = {
                walletAddress,
                subArgs,
                NFT["adminSignature"].ToString(),
                paymentValue.ToString()
            };

            string methodID = await Web3GL.SendContract(
                "buyAndMint",
                JsonConvert.SerializeObject(NFTMarketplace_metadata["output"]["abi"]),
                Constants.PROTOCOL_CONTRACTS[chainId],
                JsonConvert.SerializeObject(args),
                "0",
                "",
                ""
            );

            while (true)
            {
                string status = await EVM.TxStatus(
                    Constants.chainId2Chain[chainId],
                    Constants.chainId2Network[chainId],
                    methodID
                );

                if (status == "success")
                {
                    break;
                }
                await new WaitForSeconds(1);
            }

            return "true";
        }
        catch (Exception e)
        {
            return e.ToString();
        }
    }
}