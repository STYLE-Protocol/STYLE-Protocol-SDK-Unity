using System.Threading.Tasks;
using System.Numerics;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using UnityEngine;
using UnityEngine.Networking;
using Web3Unity.Scripts.Library.Ethers.Contracts;
using Web3Unity.Scripts.Library.Web3Wallet;

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

public class ContractsData
{   
    public JToken protocols;
    public JToken stakings;
    public string voxelsConnector;
    public string dclConnector;
    public JToken stables;
    public JToken styles;
    public JToken weths;
    public JToken metaversesJson;
    public JToken stablesDecimals;
    public JToken modelIDs;
    public JToken modelNames;
    public string storageMessage;
    public string storagePrefix;
    public string gateway;
}

public class UserProof
{
    public string walletAddress;
    public string signature;
}

public class STYLE_SDK
{    
    async public static Task<List<Dictionary<string, object>>> getRequestedNFTs(
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
            if (metaverseFilter.Count != 0) {
                url = String.Format("{0}&metaverseFilter={1}", url, JsonConvert.SerializeObject(metaverseFilter));
            }
            if (typeFilter.Count != 0) {
                url = String.Format("{0}&typeFilter={1}", url, JsonConvert.SerializeObject(typeFilter));
            }
            if (subtypeFilter.Count != 0) {
                url = String.Format("{0}&subtypeFilter={1}", url, JsonConvert.SerializeObject(subtypeFilter));
            }

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
        Dictionary<string, object> NFT,
        int chainId = 5
    )
    {
        try
        {
            string walletAddress = PlayerPrefs.GetString("Account");
            string ERC20_ABI = Resources.Load<TextAsset>("ERC20_ABI").text;

            PaymentToken paymentToken = (PaymentToken)NFT["paymentToken"];

            Payment payment = (Payment)NFT["payment"];
            BigInteger paymentValue = payment.value;

            
            var provider = RPC.GetInstance.Provider();
            var contract = new Contract(ERC20_ABI, paymentToken.address, provider);
            string protocolContract = (string)(await getContractsData()).protocols[chainId.ToString()];

            string allowanceStr = (await contract.Call("allowance", new object[]{ walletAddress, protocolContract }))[0].ToString();

            BigInteger allowance = BigInteger.Parse(allowanceStr);

            if (paymentValue > allowance)
            {
                #if UNITY_WEBGL

                string response = await Web3GL.SendContract(
                    "approve",
                    ERC20_ABI,
                    paymentToken.address,
                    JsonConvert.SerializeObject(new string[] { protocolContract, paymentValue.ToString() }),
                    "0",
                    "",
                    ""
                );

                #else

                var calldata = contract.Calldata(
                    "approve",
                    new object[] {
                        protocolContract,
                        paymentValue
                    }
                );

                await Web3Wallet.SendTransaction(chainId.ToString(), paymentToken.address, "0", calldata, "", "");
            
                #endif
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
        Dictionary<string, object> NFT,
        int chainId = 5
    )
    {
        try
        {
            string walletAddress = PlayerPrefs.GetString("Account");
            JObject NFTMarketplace_metadata = JObject.Parse(Resources.Load<TextAsset>("NFTMarketplace_metadata").text);

            PaymentToken paymentToken = (PaymentToken)NFT["paymentToken"];

            Payment payment = (Payment)NFT["payment"];
            BigInteger paymentValue = payment.value;

            string protocolContract = (string)(await getContractsData()).protocols[chainId.ToString()];

            var provider = RPC.GetInstance.Provider();

            #if UNITY_WEBGL

            string response = await Web3GL.SendContract(
                "buyAndMint",
                NFTMarketplace_metadata["output"]["abi"].ToString(),
                protocolContract,
                JsonConvert.SerializeObject(new object[] {
                    walletAddress,
                    new string[] {
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
                    },
                    NFT["adminSignature"].ToString(),
                    paymentValue.ToString()
                }),
                "0",
                "",
                ""
            );

            #else

            var contract = new Contract(NFTMarketplace_metadata["output"]["abi"].ToString(), protocolContract, provider);

            var calldata = contract.Calldata(
                "buyAndMint",
                new object[] {
                    walletAddress,
                    new object[] {
                        BigInteger.Parse(NFT["tokenId"].ToString()),
                        paymentValue,
                        NFT["tokenAddress"].ToString(),
                        BigInteger.Parse(NFT["metaverseId"].ToString()),
                        paymentToken.address,
                        BigInteger.Parse(NFT["modelId"].ToString()),
                        NFT["bidder"].ToString(),
                        NFT["environment"].ToString(),
                        NFT["uri"].ToString(),
                        StringBytesToByteArray(NFT["signature"].ToString())
                    },
                    StringBytesToByteArray(NFT["adminSignature"].ToString()),
                    paymentValue
                }
            );

            await Web3Wallet.SendTransaction(chainId.ToString(), protocolContract, "0", calldata, "", "");

            #endif

            return "true";
        }
        catch (Exception e)
        {
            return e.ToString();
        }
    }

    async public static Task<ContractsData> getContractsData()
    {
        string url = String.Format(@"https://{0}/api/contracts", Constants.API_HOST);
        JObject contractsDataRaw;
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            await webRequest.SendWebRequest();
            
            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log(webRequest.error);
                return null;
            }
            
            string res = webRequest.downloadHandler.text;
            contractsDataRaw = JObject.Parse(res);
        }   

        ContractsData contractsData = new ContractsData();
        contractsData.protocols = contractsDataRaw["protocols"];
        contractsData.stakings = contractsDataRaw["stakings"];
        contractsData.voxelsConnector = (string)contractsDataRaw["voxelsConnector"];
        contractsData.dclConnector = (string)contractsDataRaw["dclConnector"];
        contractsData.stables = contractsDataRaw["stables"];
        contractsData.styles = contractsDataRaw["styles"];
        contractsData.weths = contractsDataRaw["weths"];
        contractsData.metaversesJson = contractsDataRaw["metaversesJson"];
        contractsData.stablesDecimals = contractsDataRaw["stablesDecimals"];
        contractsData.modelIDs = contractsDataRaw["modelIDs"];
        contractsData.modelNames = contractsDataRaw["modelNames"];
        contractsData.storageMessage = (string)contractsDataRaw["storageMessage"];
        contractsData.storagePrefix = (string)contractsDataRaw["storagePrefix"];
        contractsData.gateway =(string)contractsDataRaw["gateway"];

        return contractsData;
    }

    private static byte[] StringBytesToByteArray(String hex)
    {
        hex = hex.Substring(2, hex.Length - 2).ToUpper();

        int NumberChars = hex.Length;
        byte[] bytes = new byte[NumberChars / 2];
        for (int i = 0; i < NumberChars; i += 2)
            bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
        return bytes;
    }

    async public static Task<Dictionary<string, string>> getUserProof()
    {
        try
        {
            string userProofStr = PlayerPrefs.GetString("STYLE_SDK_PROOF");

            Dictionary<string, string> userProof = new Dictionary<string, string>();
            if (userProofStr == "") {
                string walletAddress = PlayerPrefs.GetString("Account");
    
                string storageMessage = (string)(await getContractsData()).storageMessage;

                
                userProof.Add("walletAddress", walletAddress);

                #if UNITY_WEBGL

                string signHashed = await Web3GL.Sign(storageMessage);

                userProof.Add("signature", signHashed);

                #else

                string signature = await Web3Wallet.Sign(storageMessage);
                
                userProof.Add("signature", signature);

                #endif
                
                PlayerPrefs.SetString("STYLE_SDK_PROOF", JsonConvert.SerializeObject(userProof));
                PlayerPrefs.Save();
            } else {
                userProof = JsonConvert.DeserializeObject<Dictionary<string, string>>(userProofStr);
            }

            return userProof;
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
            return null;
        }
    }

    async public static Task<List<Dictionary<string, object>>> getOwnedDerivatives(
        List<string> metaverseFilter,
        List<string> typeFilter,
        List<string> subtypeFilter,
        Dictionary<string, string> userProof,
        int chainId = 5
    )
    {
        try {
            string url = String.Format(@"https://{0}/api/nfts/get-owned-derivatives?endpoint={1}&alchemyKey={2}&userProof={3}",
                Constants.API_HOST,
                Secrets.ENDPOINTS[chainId],
                Secrets.ALCHEMY_KEY,
                JsonConvert.SerializeObject(userProof)
            );
            if (metaverseFilter.Count != 0) {
                url = String.Format("{0}&metaverseFilter={1}", url, JsonConvert.SerializeObject(metaverseFilter));
            }
            if (typeFilter.Count != 0) {
                url = String.Format("{0}&typeFilter={1}", url, JsonConvert.SerializeObject(typeFilter));
            }
            if (subtypeFilter.Count != 0) {
                url = String.Format("{0}&subtypeFilter={1}", url, JsonConvert.SerializeObject(subtypeFilter));
            }


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
}