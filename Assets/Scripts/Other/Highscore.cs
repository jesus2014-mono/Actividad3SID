using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Highscore : MonoBehaviour
{

    private static int amount;
    private static TextMeshProUGUI highscoreText;
    // Start is called before the first frame update
    void Start()
    {
        amount = PlayerPrefs.GetInt("HighscoreAmount", 0);
        highscoreText = this.GetComponent<TextMeshProUGUI>();
        DisplayAmount();
    }
    public static int GetAmount()
    {
        return amount;
    }
    public static void SetAmount(int amountToSet)
    {
        amount = amountToSet;
        DisplayAmount();
        PlayerPrefs.SetInt("HighscoreAmount", amount);

    }
    private static void DisplayAmount()
    {
        highscoreText.text = "Highest score achieved: " + amount.ToString();
    }
}
