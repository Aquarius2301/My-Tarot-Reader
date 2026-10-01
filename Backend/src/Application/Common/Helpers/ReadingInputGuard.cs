using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace MyTarotReader.Application.Common.Helpers;

/// <summary>
/// Screens the free text a user types into an AI reading before the AI is ever called.
/// </summary>
/// <remarks>
/// Every check runs against a normalized copy of the text (compatibility decomposed, accents
/// removed, lower-cased, punctuation turned into single spaces) so casing, diacritics and stray
/// symbols cannot be used to slip a phrase past the screen. The checks are deliberately narrow:
/// they reject crisis content and prompt-injection attempts, and detect keyboard mash. Whether a
/// question is a real, readable decision is left to the AI prompt, which answers it semantically.
/// </remarks>
public static partial class ReadingInputGuard
{
    /// <summary>
    /// A run of this many identical characters marks keyboard mash rather than real input.
    /// </summary>
    private const int RepeatedCharacterRunLength = 5;

    /// <summary>
    /// The fewest words before the distinct-word ratio carries any meaning.
    /// </summary>
    private const int MinimumWordsForRatioCheck = 4;

    /// <summary>
    /// The share of distinct words below which the text is only the same word repeated.
    /// </summary>
    private const double MinimumDistinctWordRatio = 0.4;

    /// <summary>
    /// Phrases that only show up when someone describes self-harm, suicide or hurting someone
    /// else. Written accent-free because the text is accent-stripped before it is matched, and
    /// matched on whole words so "tự tử" cannot collide with "tự túc" or "tư tưởng".
    /// </summary>
    private static readonly string[] UnsafePhrases =
    [
        // Self-harm / suicide intent, English.
        "suicide",
        "suicides",
        "suicidal",
        "kill myself",
        "killing myself",
        "hurt myself",
        "harm myself",
        "cut myself",
        "self harm",
        "selfharm",
        "overdose",
        "overdoses",
        "end my life",
        "end my own life",
        "end it all",
        "take my own life",
        "not worth living",
        "no reason to live",
        "better off dead",
        // Self-harm / suicide intent, Vietnamese.
        "tu tu",
        "tu sat",
        "tu gay hai",
        "tu lam khon",
        "khong muon song",
        "khong muon ton tai",
        "muon chet",
        "ket thuc cuoc doi",
        "chet nghinh",
        "nhay ra cua so",
        // The same intent typed with teencode or missing tone marks.
        "mun chet",
        "mun tu tu",
        "mun tu sat",
        "ko muon song",
        "ko muon ton tai",
        "hok muon song",
        "hem muon song",
        "khum muon song",
        "hong muon song",
        // Harm to other people.
        "how to kill",
        "how to poison",
        "how to hurt",
        "cach lam dau",
    ];

    /// <summary>
    /// The long crisis phrases matched again with every space removed, so a run-together spelling
    /// ("muonchet", "ketthuccuocdoi") cannot hide behind the missing spaces. Kept long enough to
    /// stay unambiguous; short phrases are left to the word-bounded list above.
    /// </summary>
    private static readonly string[] CompactUnsafePhrases =
    [
        "muonchet",
        "munchet",
        "khongmuonsong",
        "khongmuontontai",
        "ketthuccuocdoi",
        "tusat",
    ];

    /// <summary>
    /// The phrase list compiled once into a single word-bounded pattern.
    /// </summary>
    private static readonly Regex UnsafePhraseRegex = BuildPhraseRegex(UnsafePhrases);

    /// <summary>
    /// Catches the "I want to die" family that a keyword list alone would miss, while requiring
    /// the wish to act directly on the verb so "I want to know if this job will die" is not a hit.
    /// </summary>
    [GeneratedRegex(
        @"(?:\bwant|\bwanna|\bwish|\bhope|\bplan|\bplanning|\bintend)\s+(?:to\s+|to\s+not\s+)?(?:die|kill\s+myself|hurt\s+myself|harm\s+myself|end\s+my\s+life|be\s+dead)\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant
    )]
    private static partial Regex SelfHarmIntentRegex();

    /// <summary>
    /// Catches the usual ways of trying to steer the model out of its role: overriding the
    /// instructions, role-playing as a different model, or asking for the hidden prompt.
    /// </summary>
    [GeneratedRegex(
        @"(?:ignore|disregard|forget|override|bypass|discard|skip)\b.{0,25}\b(?:previous|prior|above|earlier|preceding|all|any|your|the|my)\b.{0,15}\b(?:instruction|prompt|rule|direction|guideline|command|constraint|context|message|safety|filter|guardrail)s?"
            + @"|(?:you\s+are|you're|youre|act\s+as|behave\s+as|pretend\s+to\s+be|pretend\s+you\s+are|simulate|imagine\s+you\s+are|from\s+now\s+on|starting\s+now)\b.{0,40}\b(?:ai|assistant|model|chatbot|gpt|gemini|unrestricted|uncensored|unfiltered|jailbroken|jailbreak|no\s+restrictions|evil)\b"
            + @"|(?:reveal|show|print|repeat|output|display|tell|disclose|leak|dump)\b.{0,30}\b(?:system\s+prompt|system\s+message|initial\s+instruction|hidden\s+instruction|your\s+instruction|your\s+prompt|your\s+rule|developer\s+message)\b"
            + @"|\brepeat\b.{0,25}\b(?:everything|all\s+the\s+text|the\s+text|word\s+for\s+word)\b.{0,20}\b(?:above|before|prior|preceding)\b"
            + @"|(?:stop|end|exit|quit)\b.{0,12}\b(?:being|acting\s+as)\b.{0,12}\b(?:tarot|reader|ai|assistant|model)\b"
            + @"|\brole\s?play\b|\bnew\s+persona\b|\bnew\s+role\b|\bdeveloper\s+mode\b|\bdan\s+mode\b"
            + @"|\bbo\s+qua\b.{0,20}\b(?:huong\s+dan|chi\s+dan|prompt)\b.{0,20}\b(?:truoc|o\s+tren|he\s+thong|goc|ban\s+dau)\b"
            + @"|\b(?:tiet\s+lo|in\s+ra|doc\s+lai|cho\s+toi\s+xem|hien\s+thi)\b.{0,25}\b(?:system\s+prompt|prompt\s+he\s+thong|huong\s+dan\s+goc|chi\s+dan\s+goc|huong\s+dan\s+he\s+thong)\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant
    )]
    private static partial Regex PromptInjectionRegex();

    /// <summary>
    /// Determines whether the text describes self-harm, suicide or hurting another person.
    /// </summary>
    /// <param name="value">The raw user text to screen.</param>
    /// <returns><c>true</c> when the text must never reach the AI.</returns>
    public static bool ContainsUnsafeContent(string? value) =>
        Screen(
            value,
            text =>
                UnsafePhraseRegex.IsMatch(text)
                || SelfHarmIntentRegex().IsMatch(text)
                || ContainsCompactUnsafePhrase(text)
        );

    /// <summary>
    /// Determines whether the text tries to steer the model out of its tarot role.
    /// </summary>
    /// <param name="value">The raw user text to screen.</param>
    /// <returns><c>true</c> when the text looks like an instruction to the model.</returns>
    public static bool ContainsPromptInjection(string? value) =>
        Screen(value, PromptInjectionRegex().IsMatch);

    /// <summary>
    /// Determines whether the text is obvious keyboard mash rather than something a reader can
    /// interpret; an empty or whitespace-only text is left to the request validators.
    /// </summary>
    /// <param name="value">The raw user text to screen.</param>
    /// <returns><c>true</c> when the text carries no readable content.</returns>
    public static bool IsNonsensical(string? value)
    {
        var text = Normalize(value);

        if (text.Length == 0)
        {
            // Punctuation-only input normalizes to nothing but is still not a readable question.
            return !string.IsNullOrWhiteSpace(value);
        }

        if (!text.Any(char.IsLetter))
        {
            return true;
        }

        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (words.Any(HasRepeatedCharacterRun))
        {
            return true;
        }

        return words.Length >= MinimumWordsForRatioCheck
            && words.Distinct(StringComparer.Ordinal).Count()
                < words.Length * MinimumDistinctWordRatio;
    }

    private static bool Screen(string? value, Func<string, bool> predicate)
    {
        var text = Normalize(value);

        return text.Length > 0 && predicate(text);
    }

    private static bool HasRepeatedCharacterRun(string word)
    {
        var run = 1;

        for (var i = 1; i < word.Length; i++)
        {
            run = word[i] == word[i - 1] ? run + 1 : 1;

            if (run >= RepeatedCharacterRunLength)
            {
                return true;
            }
        }

        return false;
    }

    private static bool ContainsCompactUnsafePhrase(string text)
    {
        var compact = text.Replace(" ", string.Empty, StringComparison.Ordinal);

        return CompactUnsafePhrases.Any(phrase =>
            compact.Contains(phrase, StringComparison.Ordinal)
        );
    }

    /// <summary>
    /// Compiles the accent-free phrases into one alternation where each phrase must start and end
    /// on a word boundary; the spaces inside a phrase tolerate any run of whitespace.
    /// </summary>
    private static Regex BuildPhraseRegex(IEnumerable<string> phrases)
    {
        var pattern = string.Join(
            "|",
            phrases.Select(phrase => $@"\b{Regex.Escape(phrase).Replace(@"\ ", @"\s+")}\b")
        );

        return new Regex(
            pattern,
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled
        );
    }

    /// <summary>
    /// Turns the text into a lowercase, accent-free, space-separated form so a phrase can only be
    /// hidden from the screen by changing its look, never its letters.
    /// </summary>
    private static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var decomposed = value.Normalize(NormalizationForm.FormKD);

        var builder = new StringBuilder(decomposed.Length);
        var pendingSeparator = false;

        foreach (var character in decomposed)
        {
            // Diacritics carry no meaning here: "tự tử" and "tu tu" must hit the same phrase.
            if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            if (!char.IsLetterOrDigit(character))
            {
                pendingSeparator = true;
                continue;
            }

            if (pendingSeparator && builder.Length > 0)
            {
                builder.Append(' ');
            }

            pendingSeparator = false;
            builder.Append(FoldCharacter(character));
        }

        return builder.ToString();
    }

    /// <summary>
    /// Lowercases a letter and folds <c>đ</c>, which Unicode decomposition leaves untouched, onto
    /// plain <c>d</c> so accent-free phrases still match ("cuộc đời" and "cuoc doi").
    /// </summary>
    private static char FoldCharacter(char character)
    {
        var lowered = char.ToLowerInvariant(character);

        return lowered == 'đ' ? 'd' : lowered;
    }
}
