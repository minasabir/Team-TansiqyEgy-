namespace TansiqyV1.DAL.Helpers;

/// <summary>
/// Provides comprehensive Arabic text normalization for intelligent search.
/// Handles letter variations, diacritics removal, and morphological transformations.
/// This works GENERICALLY for all Arabic text using algorithmic rules.
/// </summary>
public static class ArabicTextNormalizer
{
    // Arabic letter Unicode mappings for normalization
    private static readonly Dictionary<char, char> LetterMappings = new()
    {
        // Ta marbuta variations
        { '\u0629', '\u0647' }, // ة (ta marbuta) → ه (ha)
        
        // Alif variations with hamza
        { '\u0623', '\u0627' }, // أ (hamza above) → ا (alif)
        { '\u0625', '\u0627' }, // إ (hamza below) → ا (alif)
        { '\u0622', '\u0627' }, // آ (madda above) → ا (alif)
        
        // Alif maksura
        { '\u0649', '\u064A' }, // ى (alif maksura) → ي (ya)
        
        // Hamza on waw
        { '\u0624', '\u0648' }, // ؤ (hamza on waw) → و (waw)
        
        // Hamza on ya
        { '\u0626', '\u064A' }, // ئ (hamza on ya) → ي (ya)
    };

    // Diacritics (tashkeel) Unicode range
    private const int DiacriticsStart = 0x064B; // FATHATAN
    private const int DiacriticsEnd = 0x065F;   // WAVY HAMZA BELOW
    
    // Additional special characters to remove
    private static readonly HashSet<char> SpecialChars = new()
    {
        '\u0640', // TATWEEL (ـ)
        '\u0670', // SUPERSCRIPT ALEF (ٰ)
        '\u0671', // ALEF WASLA (ٱ)
        '\u0600', // Arabic Number Sign
        '\u0601', // Arabic Sign Sanah
        '\u0602', // Arabic Footnote Marker
        '\u0603', // Arabic Sign Safha
    };

    /// <summary>
    /// Normalizes Arabic text for search by applying algorithmic transformations.
    /// Handles all letter variations and removes diacritics.
    /// </summary>
    public static string Normalize(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var result = new System.Text.StringBuilder(input.Length);

        foreach (var ch in input)
        {
            // Skip diacritics and special characters
            if (IsDiacriticOrSpecial(ch))
                continue;

            // Map letter variations to standard forms
            if (LetterMappings.TryGetValue(ch, out var normalizedChar))
            {
                result.Append(normalizedChar);
            }
            else
            {
                result.Append(ch);
            }
        }

        return NormalizeWhitespace(result.ToString());
    }

    /// <summary>
    /// Extracts the root form of Arabic text by removing common affixes.
    /// This helps match singular/plural and related word forms.
    /// </summary>
    public static string ExtractRoot(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // First normalize
        var normalized = Normalize(input);
        
        // Remove definite article "ال" from beginning
        if (normalized.StartsWith("ال"))
        {
            normalized = normalized[2..];
        }

        // Remove common suffixes for root extraction (in order of priority)
        var suffixes = new[] { "ات", "ان", "ين", "ون", "وا", "يه", "ية", "ه", "ا", "ي" };
        
        foreach (var suffix in suffixes)
        {
            if (normalized.EndsWith(suffix) && normalized.Length > suffix.Length + 2)
            {
                normalized = normalized[..^suffix.Length];
                break; // Remove only one suffix
            }
        }

        // Remove common prefixes (after suffix removal)
        var prefixes = new[] { "است", "ال", "م", "ت", "ن", "ي", "ا" };
        
        foreach (var prefix in prefixes)
        {
            if (normalized.StartsWith(prefix) && normalized.Length > prefix.Length + 2)
            {
                normalized = normalized[prefix.Length..];
                break;
            }
        }

        return normalized;
    }

    /// <summary>
    /// Generates search variations for a given Arabic search term.
    /// Creates multiple forms to maximize matching probability.
    /// </summary>
    public static List<string> GenerateSearchVariations(string? searchTerm)
    {
        var variations = new HashSet<string>(StringComparer.Ordinal);
        
        if (string.IsNullOrWhiteSpace(searchTerm))
            return variations.ToList();

        var normalized = Normalize(searchTerm);
        
        // Base normalized form
        variations.Add(normalized);
        
        // With definite article (if not present)
        if (!normalized.StartsWith("ال"))
        {
            variations.Add("ال" + normalized);
        }
        
        // Without definite article (if present)
        if (normalized.StartsWith("ال") && normalized.Length > 2)
        {
            variations.Add(normalized[2..]);
        }

        // Root form for broader matching
        var root = ExtractRoot(searchTerm);
        if (!string.IsNullOrEmpty(root) && root.Length >= 2 && !variations.Contains(root))
        {
            variations.Add(root);
            
            // Root with article
            if (!root.StartsWith("ال"))
            {
                variations.Add("ال" + root);
            }
        }

        // Generate pattern variations for common forms
        GeneratePatternVariations(normalized, variations);

        return variations.ToList();
    }

    /// <summary>
    /// Generates additional variations based on common Arabic patterns.
    /// </summary>
    private static void GeneratePatternVariations(string normalized, HashSet<string> variations)
    {
        // Add feminine form if ends with ه (ta marbuta normalized to ه)
        if (!normalized.EndsWith("ه") && normalized.Length > 2)
        {
            variations.Add(normalized + "ه");
            variations.Add(normalized + "ات");
        }

        // Add masculine plural forms
        if (normalized.Length > 2)
        {
            variations.Add(normalized + "ون");
            variations.Add(normalized + "ين");
        }

        // Add nisba form
        if (!normalized.EndsWith("ي") && normalized.Length > 2)
        {
            variations.Add(normalized + "ي");
        }
    }

    /// <summary>
    /// Checks if a character is a diacritic or special character to be removed.
    /// </summary>
    private static bool IsDiacriticOrSpecial(char ch)
    {
        var code = (int)ch;
        
        // Main diacritics range
        if (code >= DiacriticsStart && code <= DiacriticsEnd)
            return true;
        
        // Special characters
        if (SpecialChars.Contains(ch))
            return true;

        return false;
    }

    /// <summary>
    /// Normalizes whitespace by collapsing multiple spaces and trimming.
    /// </summary>
    private static string NormalizeWhitespace(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var parts = input.Split(new[] { ' ', '\t', '\n', '\r', '\u200C', '\u200F' }, 
            StringSplitOptions.RemoveEmptyEntries);
        return string.Join(" ", parts);
    }

    /// <summary>
    /// Compares two Arabic strings for equality after normalization.
    /// </summary>
    public static bool AreEqual(string? str1, string? str2)
    {
        return Normalize(str1) == Normalize(str2);
    }

    /// <summary>
    /// Checks if normalized text contains normalized search term.
    /// </summary>
    public static bool Contains(string? text, string? searchTerm)
    {
        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(searchTerm))
            return false;

        var normalizedText = Normalize(text);
        var normalizedSearch = Normalize(searchTerm);

        return normalizedText.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Calculates relevance score for a search match.
    /// Higher score = better match.
    /// </summary>
    public static int CalculateRelevance(string text, string searchTerm)
    {
        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(searchTerm))
            return 0;

        var normalizedText = Normalize(text);
        var normalizedSearch = Normalize(searchTerm);
        var rootSearch = ExtractRoot(searchTerm);

        int score = 0;

        // Exact match (highest score)
        if (normalizedText == normalizedSearch)
            score += 100;

        // Starts with search term
        else if (normalizedText.StartsWith(normalizedSearch))
            score += 80;

        // Contains search term
        else if (normalizedText.Contains(normalizedSearch))
            score += 60;

        // Root match (lower score but still relevant)
        if (!string.IsNullOrEmpty(rootSearch) && rootSearch.Length >= 2)
        {
            if (normalizedText.Contains(rootSearch))
                score += 40;
        }

        // Bonus for shorter texts (more specific match)
        if (normalizedText.Length < 20)
            score += 10;

        return score;
    }
}
