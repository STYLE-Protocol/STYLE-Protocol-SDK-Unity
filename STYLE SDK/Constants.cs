using System.Collections.Generic;

public static class Constants
{
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

            /*
            metaversesJson_[1].Add("id", "1");
            metaversesJson_[1].Add("icon", "sandbox.svg");
            metaversesJson_[1].Add("name", "The Sandbox");
            metaversesJson_[1].Add("slug", "sandbox");
            metaversesJson_[1].Add("price", "200");
            */

            metaversesJson_[1].Add("id", "1");
            metaversesJson_[1].Add("icon", "somnium.svg");
            metaversesJson_[1].Add("name", "Somnium Space");
            metaversesJson_[1].Add("slug", "somnium_space");
            metaversesJson_[1].Add("price", "200");

            metaversesJson_[2].Add("id", "2");
            metaversesJson_[2].Add("icon", "cryptovoxels.svg");
            metaversesJson_[2].Add("name", "Cryptovoxels");
            metaversesJson_[2].Add("slug", "cryptovoxels");
            metaversesJson_[2].Add("price", "170.01");

            metaversesJson_[3].Add("id", "3");
            metaversesJson_[3].Add("icon", "monaverse.svg");
            metaversesJson_[3].Add("name", "Monaverse");
            metaversesJson_[3].Add("slug", "monaverse");
            metaversesJson_[3].Add("price", "199");

            metaversesJson_[4].Add("id", "4");
            metaversesJson_[4].Add("icon", "fabwelt.svg");
            metaversesJson_[4].Add("name", "Fabwelt");
            metaversesJson_[4].Add("slug", "fabwelt");
            metaversesJson_[4].Add("price", "199");

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

    public static string CHOSEN_NFT
    {
        get
        {
            return "STYLE_SDK_CHOSEN_NFT";
        }
    }
}