using UnityEngine;
using TMPro;

public static class GlobalStats
{
    public static int episode = 0;
    public static int success = 0;
    public static int fail = 0;

    public static TextMeshProUGUI episodeText = null;
    public static TextMeshProUGUI successText = null;
    public static TextMeshProUGUI failText = null;
    public static TextMeshProUGUI successRateText = null;

    public static void UpdateText()
    {
        // Find text objects if they are null 
        CheckText();

        // Calculate success rate %
        float successRate = (success / (float)(success + fail)) * 100;

        // Update text
        if (episodeText != null) { episodeText.text = $"Episode: {episode}"; }
        if (successText != null) { successText.text = $"Success: {success}"; }
        if (failText != null) { failText.text = $"Fail: {fail}"; }
        if (successRateText != null) { successRateText.text = $"Success Rate: {successRate}%"; }
    }

    private static void CheckText()
    {
        if (episodeText == null) { episodeText = GameObject.Find("EpisodeText").gameObject.GetComponent<TextMeshProUGUI>(); }
        if (successText == null) { successText = GameObject.Find("SuccessText").gameObject.GetComponent<TextMeshProUGUI>(); }
        if (failText == null) { failText =  GameObject.Find("FailText").gameObject.GetComponent<TextMeshProUGUI>(); }
        if (successRateText == null) { successRateText = GameObject.Find("SuccessRateText").gameObject.GetComponent<TextMeshProUGUI>(); }
    }
}
