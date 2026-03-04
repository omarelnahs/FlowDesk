using System.Text.RegularExpressions;

namespace FlowDesk.Domain.ValueObjects;

public static class Slug
{
    public static string Generate(string name)
    {
        var slug = name.ToLower().Trim();
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", ""); // remove non-alphanum
        slug = Regex.Replace(slug, @"\s+", "-");         // spaces -> dashes
        slug = Regex.Replace(slug, @"-{2,}", "-");       // collapse dashes
        slug = slug.Trim('-');                           // trim edges
        return slug.Length > 50 ? slug[..50] : slug;
    }
}
