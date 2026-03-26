namespace RapSuite.Infrastructure.AI;

public static class PromptTemplateService
{
    public static string BuildGenerationPrompt(string situation, string language, string mood, int durationMinutes)
    {
        int targetWords = durationMinutes * 150;
        int minWords = targetWords - 50;
        int maxWords = targetWords + 100;

        string languageInstruction = language.ToLowerInvariant() switch
        {
            "tamil" => @"Write the lyrics entirely in Tamil (தமிழ்). 
Use ONLY pure Tamil (செந்தமிழ்) words. Avoid English loanwords, Sanskrit borrowings, or Hindustani words.
Use classical Tamil vocabulary and poetic expressions. 
Write in Tamil script (not transliteration).",

            "hindi" => "Write the lyrics entirely in Hindi using Devanagari script. Use natural spoken Hindi idioms.",

            "spanish" => "Write the lyrics entirely in Spanish. Use vivid, natural Spanish expressions.",

            "korean" => "Write the lyrics entirely in Korean using Hangul. Use natural Korean expressions.",

            "japanese" => "Write the lyrics entirely in Japanese using a mix of Hiragana, Katakana, and Kanji as appropriate.",

            _ => $"Write the lyrics in {language}."
        };

        return $@"You are an expert songwriter and rapper who creates powerful, emotionally resonant lyrics.

TASK: Generate a complete rap/hip-hop song based on the following situation/theme.

SITUATION/THEME: {situation}

MOOD/VIBE: {mood}

LANGUAGE INSTRUCTIONS:
{languageInstruction}

STRUCTURE REQUIREMENTS:
- Include a catchy Hook/Chorus (repeat 2-3 times in the song)
- Write 3-4 Verses (each 8-16 bars)
- Include at least 1 Bridge or Pre-Chorus
- Label each section clearly: [Hook], [Verse 1], [Verse 2], [Bridge], etc.
- Target length: {minWords}-{maxWords} words (approximately {durationMinutes} minutes when performed)

STYLE REQUIREMENTS:
- Strong rhyme schemes (internal rhymes, multi-syllabic rhymes)
- Wordplay, metaphors, and punchlines
- Rhythmic flow that matches the {mood} mood
- Authentic emotional expression tied to the situation
- Make it suitable for performance

RESPONSE FORMAT:
First line: TITLE: [Song Title]
Then the full lyrics with section labels.
Last line: WORD_COUNT: [number]";
    }

    public static string BuildRephrasePrompt(string originalLyrics, string language, string mood, string style)
    {
        string languageInstruction = language.ToLowerInvariant() switch
        {
            "tamil" => @"Keep the lyrics in Tamil (தமிழ்). 
Convert to pure Tamil (செந்தமிழ்) words wherever possible. Replace any English or borrowed words with pure Tamil equivalents.
Use Tamil script (not transliteration).",

            _ => $"Keep the lyrics in {language}. Maintain the original language."
        };

        return $@"You are an expert rapper and lyricist who transforms songs into powerful rap/hip-hop format.

TASK: Rephrase and transform the following lyrics into a {style} song format.

ORIGINAL LYRICS:
{originalLyrics}

LANGUAGE INSTRUCTIONS:
{languageInstruction}

TARGET MOOD: {mood}

TRANSFORMATION REQUIREMENTS:
- Restructure into proper rap format: [Hook], [Verse 1], [Verse 2], [Verse 3], [Bridge]
- Add strong rhyme schemes (end rhymes, internal rhymes, multi-syllabic rhymes)
- Add wordplay, metaphors, and punchlines
- Maintain the core meaning and emotion of the original
- Add rhythmic flow appropriate for {style} style
- Make it at least as long as the original (target 400-550 words for 3-4 min duration)
- Make it suitable for performance/recording

RESPONSE FORMAT:
First line: TITLE: [Song Title]
Then the full rephrased lyrics with section labels.
Last line: WORD_COUNT: [number]";
    }
}
