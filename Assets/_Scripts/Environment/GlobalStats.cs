using UnityEngine;
using TMPro;

public static class GlobalStats
{
    [Header("Statistics of Training")]
    public static int episode = 0;
    public static int success = 0;
    public static int fail = 0;
    public static int completedEpisodes = 0;

    [Header("Text for Statistics")]
    public static TextMeshProUGUI episodeText = null;
    public static TextMeshProUGUI successText = null;
    public static TextMeshProUGUI failText = null;
    public static TextMeshProUGUI successRateText = null;

    /// <summary>
    /// Updates the text for each statistic tracked in the environment. 
    /// Episode number, successes, fails, success rate. 
    /// </summary>
    public static void UpdateText()
    {
        // Find text objects if they are null 
        CheckText();

        // Calculate success rate %
        float successRate = (success / (float)(success + fail)) * 100;

        // Update text
        if (episodeText != null) { episodeText.text = $"Episode: <color=#F14A63>{episode}"; }
        if (successText != null) { successText.text = $"Success: <color=#F14A63>{success}"; }
        if (failText != null) { failText.text = $"Fail: <color=#F14A63>{fail}"; }

        // On start, set the percentage to 0%. Fixes NaN% display value.
        if (completedEpisodes == 0)
        {
            successRateText.text = $"Success Rate: <color=#F14A63>0.00%";
        }
        else
        {
            // Otherwise, update it with the real success rate value.
            if (successRateText != null) { successRateText.text = $"Success Rate: <color=#F14A63>{successRate.ToString("F2")}%"; }
        }
    }

    /// <summary>
    /// Gets the text for each statistic if they are null. 
    /// </summary>
    private static void CheckText()
    {
        if (episodeText == null) { episodeText = GameObject.Find("EpisodeText").gameObject.GetComponent<TextMeshProUGUI>(); }
        if (successText == null) { successText = GameObject.Find("SuccessText").gameObject.GetComponent<TextMeshProUGUI>(); }
        if (failText == null) { failText =  GameObject.Find("FailText").gameObject.GetComponent<TextMeshProUGUI>(); }
        if (successRateText == null) { successRateText = GameObject.Find("SuccessRateText").gameObject.GetComponent<TextMeshProUGUI>(); }
    }

    /// <summary>
    /// Reset stats on scene change. 
    /// </summary>
    public static void ResetStats()
    {
        episode = 0;
        success = 0;
        fail = 0;
        completedEpisodes = 0;
    }
}
