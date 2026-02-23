using System.Text;

namespace SpikeCli;

public static class StringExtensions
{
    public static string[] ToArgArray(this string str)
    { 
        var partBuilder = new StringBuilder();
        var partList = new List<string>();
        var isInsideString = false;
        
        foreach (var c in str)
        {
            switch (c)
            {
                case '"':
                    isInsideString = !isInsideString;
                    continue;
                
                case ' ' when !isInsideString:
                    partList.Add(partBuilder.ToString());
                    partBuilder.Clear();
                    break;
                
                default:
                    partBuilder.Append(c);
                    break;
            }
        }
        
        if (partBuilder.Length > 0) 
            partList.Add(partBuilder.ToString());

        partList.RemoveAll(string.IsNullOrWhiteSpace);
        
        return partList.ToArray();
    }
}