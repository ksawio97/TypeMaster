namespace TypeMaster.DataModels;

public static class SearchResultExtensions 
{ 
    public static bool IsNullOrEmpty(this SearchResult? searchResult)
    {
        return searchResult == null ||
               searchResult.Title.Equals(string.Empty) ||
               searchResult.Id <= 0;
    }
}
