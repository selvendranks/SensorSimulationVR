using TMPro;
using UnityEngine;

public class SliderValueLabel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI valueText;
    [SerializeField] private string prefix = "";
    [SerializeField] private string suffix = "";
    [SerializeField] private bool wholeNumbers = false;

    private void Awake()
    {
        if (valueText == null)
            valueText = GetComponent<TextMeshProUGUI>();
    }

    public void SetValue(float value)
    {
        if (valueText == null) return;

        string formattedValue = wholeNumbers
            ? Mathf.RoundToInt(value).ToString()
            : value.ToString("0.0");

        valueText.text = prefix + formattedValue + suffix;
    }
}