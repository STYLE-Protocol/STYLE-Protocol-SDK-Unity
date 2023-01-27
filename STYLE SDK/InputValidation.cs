using System.Collections.Generic;
using System.Linq;

public static class InputValidation
{
    public static List<string> validateMetaverseFilter(string metaverseFilter)
    {
        List<string> metaverseFilter_ = new List<string>();
        metaverseFilter_.Add(metaverseFilter.ToLower());
        
        List<string> availiableMetaverses = new List<string>();
        foreach (Dictionary<string, string> metaverse in Constants.metaversesJson)
        {
            availiableMetaverses.Add(metaverse["slug"].ToLower());
        }

        if (availiableMetaverses.Contains(metaverseFilter_[0]))
        {
            return metaverseFilter_;
        }
        return new List<string>();
    }

    public static List<string> validateMetaverseFilter(List<string> metaverseFilter)
    {
        if (metaverseFilter.Count == 0)
        {
            metaverseFilter.Add("");
            return metaverseFilter;
        }
            
        List<string> availiableMetaverses = new List<string>();
        foreach (Dictionary<string, string> metaverse in Constants.metaversesJson)
        {
            availiableMetaverses.Add(metaverse["slug"].ToLower());
        }

        for (int i = 0; i < metaverseFilter.Count; i++)
        {
            metaverseFilter[i] = metaverseFilter[i].ToLower();
            if (!availiableMetaverses.Contains(metaverseFilter[i]))
            {
                return new List<string>();
            }
        }
            
        HashSet<string> metaverseFilterSet = new HashSet<string>();
        foreach (string metaverse in metaverseFilter)
        {
            metaverseFilterSet.Add(metaverse.ToLower());
        }

        return metaverseFilterSet.ToList();
    }

    public static List<string> validateTypeFilter(string typeFilter)
    {
        List<string> typeFilter_ = new List<string>();
        typeFilter_.Add(typeFilter.ToUpper());

        List<string> availiableTypes = new List<string>();
        foreach (Dictionary<string, string> type in Constants.typesJson)
        {
            availiableTypes.Add(type["slug"].ToUpper());
        }

        if (availiableTypes.Contains(typeFilter_[0]))
        {
            return typeFilter_;
        }
        return new List<string>();
    }

    public static List<string> validateTypeFilter(List<string> typeFilter)
    {
        if (typeFilter.Count == 0)
        {
            typeFilter.Add("");
            return typeFilter;
        }

        List<string> availiableTypes = new List<string>();
        foreach (Dictionary<string, string> type in Constants.typesJson)
        {
            availiableTypes.Add(type["slug"].ToUpper());
        }

        for (int i = 0; i < typeFilter.Count; i++)
        {
            typeFilter[i] = typeFilter[i].ToUpper();
            if (!availiableTypes.Contains(typeFilter[i]))
            {
                return new List<string>();
            }
        }

        HashSet<string> typeFilterSet = new HashSet<string>();
        foreach (string type in typeFilter)
        {
            typeFilterSet.Add(type.ToUpper());
        }

        return typeFilterSet.ToList();
    }

    public static List<string> validateSubtypeFilter(string subtypeFilter)
    {
        List<string> subtypeFilter_ = new List<string>();
        subtypeFilter_.Add(subtypeFilter);

        return subtypeFilter_;
    }

    public static List<string> validateSubtypeFilter(List<string> subtypeFilter)
    {
        if (subtypeFilter.Count == 0)
        {
            subtypeFilter.Add("");
            return subtypeFilter;
        }

        HashSet<string> subtypeFilterSet = new HashSet<string>();
        foreach (string metaverse in subtypeFilter)
        {
            subtypeFilterSet.Add(metaverse);
        }

        return subtypeFilterSet.ToList();
    }

}