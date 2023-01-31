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
            JObject NFTMarketplace_metadata = JObject.Parse(Resources.Load<TextAsset>("NFTMarketplace_metadata").text);
            string ERC20_ABI = Resources.Load<TextAsset>("ERC20_ABI").text;

            metaverseFilter = InputValidation.validateMetaverseFilter(metaverseFilter);
            typeFilter = InputValidation.validateMetaverseFilter(typeFilter);
            subtypeFilter = InputValidation.validateMetaverseFilter(subtypeFilter);
            /*
            if (metaverseFilter_.Count == 0 || typeFilter_.Count == 0) {
                return await Task.FromResult(new List<Dictionary<string, object>>());
            }
            */
            string[] args = { cursor.ToString(), amount.ToString() };
            string response = await EVM.Call(
                Constants.chainId2Chain[chainId],
                Constants.chainId2Network[chainId],
                Constants.PROTOCOL_CONTRACTS[chainId],
                JsonConvert.SerializeObject(NFTMarketplace_metadata["output"]["abi"]),
                "getStakes",
                JsonConvert.SerializeObject(args)
            );


            JObject stakesAndCursor = JObject.Parse(response);
            JToken stakes = stakesAndCursor["0"];

            Dictionary<string, JToken> stakesData = new Dictionary<string, JToken>();
            foreach (JToken stake in stakes)
            {
                // tokenId - 0, tokenAddress - 1, 
                stakesData[String.Format("{0}|{1}", stake[1].ToString().ToLower(), BigInteger.Parse(stake[0].ToString()).ToString())] = stake;
            }

            string url = String.Format(@"https://api.pinata.cloud/data/pinList?status=pinned&pageLimit=1000&metadata[name]=NonmintedNFT&metadata[keyvalues]={{%22chainId%22:{{%22value%22:%22{0}%22,%22op%22:%22eq%22}}", chainId.ToString());
            if (metaverseFilter.Count != 0 && metaverseFilter[0] != "")
            {
                url += String.Format(",%22metaverse%22:{{%22value%22:%22{0}%22,%22op%22:%22regexp%22}}", String.Join("|", metaverseFilter));
            }
            if (typeFilter.Count != 0 && typeFilter[0] != "")
            {
                url += String.Format(",%22type%22:{{%22value%22: %22{0}%22,%22op%22:%22regexp%22}}", String.Join("|", typeFilter));
            }
            if (subtypeFilter.Count != 0 && subtypeFilter[0] != "")
            {
                url += String.Format(",%22subtype%22:{{%22value%22:%22{0}%22,%22op%22:%22regexp%22}}", String.Join("|", subtypeFilter));
            }
            url += "}";

            /*
            List<Dictionary<string, object>> error = new List<Dictionary<string, object>>();
            Dictionary<string, object> errord = new Dictionary<string, object>();
            errord["url"] = url;
            error.Add(errord);
            return error;
            */

            JToken pinned;
            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            {
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("pinata_api_key", Secrets.PINATA_API_KEY);
                webRequest.SetRequestHeader("pinata_secret_api_key", Secrets.PINATA_SECRET_API_KEY);

                await webRequest.SendWebRequest();

                pinned = JObject.Parse(webRequest.downloadHandler.text)["rows"];
            }

            List<Dictionary<string, object>> allDataParsed = new List<Dictionary<string, object>>();
            foreach (JToken cur in pinned)
            {
                using (UnityWebRequest webRequest = UnityWebRequest.Get(String.Format(@"https://{0}/ipfs/{1}", Constants.GATEWAY, cur["ipfs_pin_hash"])))
                {
                    await webRequest.SendWebRequest();

                    JObject res = JObject.Parse(webRequest.downloadHandler.text);

                    Dictionary<string, object> data = new Dictionary<string, object>();
                    foreach (KeyValuePair<string, JToken> keyValuePair in res)
                    {
                        data[keyValuePair.Key] = keyValuePair.Value;
                    }

                    JToken curStake;
                    try {
                        curStake = stakesData[String.Format("{0}|{1}", data["tokenAddress"].ToString().ToLower(), BigInteger.Parse(data["tokenId"].ToString()).ToString())];
                    } catch (Exception) {
                        continue;
                    }

                    int numberOfDerivatives = Int32.Parse(curStake[2].ToString());
                    if (numberOfDerivatives == 0)
                    {
                        continue;
                    }

                    BigInteger decimalsT;
                    try
                    {
                        decimalsT = await ERC20.Decimals(
                            Constants.chainId2Chain[chainId],
                            Constants.chainId2Network[chainId],
                            res["paymentToken"].ToString()
                        );
                    }
                    catch (Exception)
                    {
                        continue;
                    };

                    data["numberOfDerivatives"] = numberOfDerivatives;
                    res["cid"] = cur["ipfs_pin_hash"];

                    Payment payment = new Payment();
                    payment.value = BigInteger.Parse(res["payment"].ToString());
                    payment.stringValue = (BigInteger.Parse(res["payment"].ToString()) / (BigInteger.Pow(10, (int)decimalsT))).ToString();
                    data["payment"] = payment;
                    
                    string ipfsUrl = data["uri"].ToString();

                    if (String.Equals(ipfsUrl.Substring(0, 4), "ipfs"))
                    {
                        ipfsUrl = String.Format("https://{0}/ipfs/{1}", Constants.GATEWAY, ipfsUrl.Substring(7));
                    }

                    using (UnityWebRequest webRequestNew = UnityWebRequest.Get(ipfsUrl))
                    {
                        await webRequestNew.SendWebRequest();

                        JObject assetTmp = JObject.Parse(webRequestNew.downloadHandler.text);

                        Asset asset = new Asset();
                        asset.name = assetTmp["name"].ToString();
                        asset.description = assetTmp["description"].ToString();
                        asset.image = assetTmp["image"].ToString();
                        asset.animation_url = assetTmp["animation_url"].ToString();
                        if (assetTmp["extraData"] != null) {
                            asset.extraData = assetTmp["extraData"].ToString();
                        }
                        data["asset"] = asset;
                    };
                    

                    string paymentTokenTmp = data["paymentToken"].ToString();
                    PaymentToken paymentToken = new PaymentToken();
                    paymentToken.name = await ERC20.Name(
                        Constants.chainId2Chain[chainId],
                        Constants.chainId2Network[chainId],
                        paymentTokenTmp
                    );
                    paymentToken.symbol = await ERC20.Symbol(
                        Constants.chainId2Chain[chainId],
                        Constants.chainId2Network[chainId],
                        paymentTokenTmp
                    ); 
                    paymentToken.address = paymentTokenTmp;
                    data["paymentToken"] = paymentToken;

                    allDataParsed.Add(data);
                    
                }
            }

            return allDataParsed;
        }
        catch (Exception e)
        {
            List<Dictionary<string, object>> error = new List<Dictionary<string, object>>();
            Dictionary<string, object> errord = new Dictionary<string, object>();
            errord["errord"] = e.ToString();
            error.Add(errord);
            return error;
        }
    }

    
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
            List<Dictionary<string, object>> error = new List<Dictionary<string, object>>();
            Dictionary<string, object> errord = new Dictionary<string, object>();
            errord["errord"] = e.ToString();
            error.Add(errord);
            return error;
        }
    }
    

    /*
    async public static Task<List<Dictionary<string, object>>> getListedNFTs(
        List<string> metaverseFilter,
        int cursor = 0,
        int amount = 100,
        int chainId = 5
    )
    {
        try
        {
            JObject NFTMarketplace_metadata = JObject.Parse(Resources.Load<TextAsset>("NFTMarketplace_metadata").text);
            string ERC20_ABI = Resources.Load<TextAsset>("ERC20_ABI").text;
            JObject Base_metadata = JObject.Parse(Resources.Load<TextAsset>("Base_metadata").text);

            metaverseFilter = InputValidation.validateMetaverseFilter(metaverseFilter);

            string[] args = { cursor.ToString(), amount.ToString() };
            string response = await EVM.Call(
                Constants.chainId2Chain[chainId],
                Constants.chainId2Network[chainId],
                Constants.PROTOCOL_CONTRACTS[chainId],
                JsonConvert.SerializeObject(NFTMarketplace_metadata["output"]["abi"]),
                "getListings",
                JsonConvert.SerializeObject(args)
            );

            JObject listingsAndCursor = JObject.Parse(response);
            JToken listings = listingsAndCursor["0"];

            List<Dictionary<string, object>> parsedData = new List<Dictionary<string, object>>();
            foreach (JToken cur in listings)
            {
                Dictionary<string, object> data = new Dictionary<string, object>();

                string metaverseId = await EVM.Call(
                    Constants.chainId2Chain[chainId],
                    Constants.chainId2Network[chainId],
                    cur["contract_"].ToString(),
                    JsonConvert.SerializeObject(Base_metadata["output"]["abi"]),
                    "metaverseId",
                    "[]"
                );

                string metaverseSlug = "";
                foreach (Dictionary<string, string> metaverse in Constants.metaversesJson)
                {
                    if (String.Equals(metaverseId, metaverse["id"]))
                    {
                        metaverseSlug = metaverse["slug"];
                        break;
                    }
                }

                
                // List<Dictionary<string, object>> temp = new List<Dictionary<string, object>>();
                // Dictionary<string, object> tempd = new Dictionary<string, object>();
                // tempd.Add("tempd", cur);
                // temp.Add(tempd);
                // return temp;
                
                if ((metaverseFilter.Count != 0 && metaverseFilter[0] == "") || metaverseFilter.Contains(metaverseSlug))
                {

                    data.Add("tokenId", cur["0"]);
                    data.Add("payment", cur["1"]);
                    data.Add("seller", cur["2"]);
                    data.Add("contract_", cur["3"]);
                    data.Add("paymentToken", cur["4"]);
                    data.Add("environment", cur["5"]);
                    data.Add("metaverse", metaverseSlug);

                    string[] argsNew = { data["tokenId"].ToString() };
                    string ipfsUrl = await EVM.Call(
                        Constants.chainId2Chain[chainId],
                        Constants.chainId2Network[chainId],
                        cur["contract_"].ToString(),
                        JsonConvert.SerializeObject(Base_metadata["output"]["abi"]),
                        "tokenURI",
                        JsonConvert.SerializeObject(argsNew)
                    );
                    if (String.Equals(ipfsUrl.Substring(0, 4), "ipfs"))
                    {
                        ipfsUrl = String.Format("https://{0}/ipfs/{1}", Constants.GATEWAY, ipfsUrl.Substring(7));
                    }

                    using (UnityWebRequest webRequest = UnityWebRequest.Get(ipfsUrl))
                    {
                        await webRequest.SendWebRequest();

                        string webResponse = webRequest.downloadHandler.text;
                        JObject metadata = JObject.Parse(webResponse);

                        data.Add("asset", metadata);
                    };

                    BigInteger decimals = await ERC20.Decimals(
                        Constants.chainId2Chain[chainId],
                        Constants.chainId2Network[chainId],
                        data["paymentToken"].ToString()
                    );

                    Payment payment = new Payment();
                    payment.value = BigInteger.Parse(data["payment"].ToString());
                    payment.stringValue = (BigInteger.Parse(data["payment"].ToString()) / (BigInteger.Pow(10, (int)decimals))).ToString();
                    data["payment"] = payment;

                    string name = await ERC20.Name(
                        Constants.chainId2Chain[chainId],
                        Constants.chainId2Network[chainId],
                        data["paymentToken"].ToString()
                    );

                    string symbol = await ERC20.Symbol(
                        Constants.chainId2Chain[chainId],
                        Constants.chainId2Network[chainId],
                        data["paymentToken"].ToString()
                    );
                    PaymentToken paymentToken = new PaymentToken();
                    paymentToken.name = name;
                    paymentToken.symbol = symbol;
                    paymentToken.address = data["paymentToken"].ToString();
                    data["paymentToken"] = paymentToken;

                    parsedData.Add(data);
                }
            }

            return parsedData;
        }
        catch (Exception e)
        {
            List<Dictionary<string, object>> error = new List<Dictionary<string, object>>();
            Dictionary<string, object> errord = new Dictionary<string, object>();
            errord.Add("errord", e.ToString());
            error.Add(errord);
            return error;
        }
    }
*/

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

    /*
    async public static Task<bool> buyItem(
        Dictionary<string, object> NFT,
        int chainId = 5
    )
    {
        try
        {            
            JObject NFTMarketplace_metadata = JObject.Parse(Resources.Load<TextAsset>("NFTMarketplace_metadata").text);

            Payment payment = (Payment)NFT["payment"];
            BigInteger paymentValue = payment.value;

            string[] args = {
                NFT["contract_"].ToString(),
                NFT["tokenId"].ToString(),
                paymentValue.ToString()
            };
            string methodID = await Web3GL.SendContract(
                "buy",
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

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
    */

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