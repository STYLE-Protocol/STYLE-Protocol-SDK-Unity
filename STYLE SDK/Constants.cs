using System.Collections.Generic;

public static class Constants
{
    public static Dictionary<int, string> PROTOCOL_CONTRACTS
    {
        get
        {
            Dictionary<int, string> protocol_contract = new Dictionary<int, string>();

            protocol_contract.Add(80001, "0xFfe8B49e11883De88e110604DA018572b93f9f24");
            protocol_contract.Add(5, "0x87148553f8D5c32Ec2358Ab1f3b2eF9C3bBd0f6D");

            return protocol_contract;
        }
    }

    public static List<Dictionary<string, string>> metaversesJson
    {
        get
        {
            List<Dictionary<string, string>> metaversesJson_ = new List<Dictionary<string, string>>();

            metaversesJson_.Add(new Dictionary<string, string>());
            metaversesJson_.Add(new Dictionary<string, string>());
            metaversesJson_.Add(new Dictionary<string, string>());
            metaversesJson_.Add(new Dictionary<string, string>());

            metaversesJson_[0].Add("id", "0");
            metaversesJson_[0].Add("icon", "decentraland.svg");
            metaversesJson_[0].Add("name", "Decentraland");
            metaversesJson_[0].Add("slug", "decentraland");
            metaversesJson_[0].Add("price", "600");

            metaversesJson_[1].Add("id", "1");
            metaversesJson_[1].Add("icon", "sandbox.svg");
            metaversesJson_[1].Add("name", "The Sandbox");
            metaversesJson_[1].Add("slug", "sandbox");
            metaversesJson_[1].Add("price", "200");

            metaversesJson_[2].Add("id", "2");
            metaversesJson_[2].Add("icon", "somnium.svg");
            metaversesJson_[2].Add("name", "Somnium Space");
            metaversesJson_[2].Add("slug", "somnium_space");
            metaversesJson_[2].Add("price", "200");

            metaversesJson_[3].Add("id", "3");
            metaversesJson_[3].Add("icon", "cryptovoxels.svg");
            metaversesJson_[3].Add("name", "Cryptovoxels");
            metaversesJson_[3].Add("slug", "cryptovoxels");
            metaversesJson_[3].Add("price", "170.01");

            return metaversesJson_;
        }
    }

    public static List<Dictionary<string, string>> typesJson
    {
        get
        {
            List<Dictionary<string, string>> typesJson_ = new List<Dictionary<string, string>>();

            typesJson_.Add(new Dictionary<string, string>());
            typesJson_.Add(new Dictionary<string, string>());
            typesJson_.Add(new Dictionary<string, string>());
            
            typesJson_[0].Add("slug", "AVATAR");
            typesJson_[1].Add("slug", "WEARABLE");
            typesJson_[2].Add("slug", "MISC");

            return typesJson_;
        }
    }
    
    public static Dictionary<int, string> chainId2Chain
    {
        get
        {
            Dictionary<int, string> chainId2Chain_ = new Dictionary<int, string>();

            chainId2Chain_.Add(5, "ethereum");
            chainId2Chain_.Add(80001, "polygon");

            return chainId2Chain_;
        }
    }

    public static Dictionary<int, string> chainId2Network
    {
        get
        {
            Dictionary<int, string> chainId2Network_ = new Dictionary<int, string>();

            chainId2Network_.Add(5, "goerli");
            chainId2Network_.Add(80001, "mumbai");

            return chainId2Network_;
        }
    }

    public static string GATEWAY
    {
        get
        {
            return "styleprotocol.mypinata.cloud";
        }
    }

    public static string API_HOST
    {
        get
        {
            return "style-protocol-api.vercel.app";
        }
    }
}