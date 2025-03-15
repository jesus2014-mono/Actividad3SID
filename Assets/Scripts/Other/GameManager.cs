using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    [SerializeField] private TextMeshProUGUI scoreText; // Texto para mostrar el score
    [SerializeField] private GameObject endGamePanel; // Panel que se muestra al finalizar el juego
    [SerializeField] private TextMeshProUGUI finalScoreText; // Texto para mostrar el score final

    private int currentScore = 0; // Score actual

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        UpdateScoreText(); // Actualizar el texto del score al inicio
        endGamePanel.SetActive(false); // Asegurarse de que el panel de fin de juego est� desactivado al inicio
    }

    // M�todo para aumentar el score
    public void IncreaseScore()
    {
        currentScore++;
        UpdateScoreText();
    }

    // M�todo para finalizar el juego y enviar el score al leaderboard
    public void EndGame()
    {
        endGamePanel.SetActive(true); // Mostrar el panel de fin de juego
        finalScoreText.text = "Puntaje Final: " + currentScore.ToString(); // Mostrar el score final

        // Enviar el score al servidor
        AuthenticationManager.Instance.GetScore(currentScore);
    }

    // M�todo para actualizar el texto del score en la UI
    private void UpdateScoreText()
    {
        scoreText.text = "Puntaje: " + currentScore.ToString();
    }
}