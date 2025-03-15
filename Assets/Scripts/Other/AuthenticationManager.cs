using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class AuthenticationManager : MonoBehaviour
{
    public static AuthenticationManager Instance;
    private string Username { get; set; }
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject leaderboardPanel;
    [SerializeField] private TextMeshProUGUI[] usernamesText;
    [SerializeField] private TextMeshProUGUI[] scoresText;
    [SerializeField] private GameObject errorPanel;
    [SerializeField] TextMeshProUGUI errorText;
    string url = "https://sid-restapi.onrender.com";
    public string Token { get; set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else { Destroy(gameObject); }

    }
    private void Start()
    {
        CheckAuthentication();
    }

    public void Login()
    {
        AuthResponse credentials = new AuthResponse();
        credentials.username = GameObject.Find("UsuarioInput")
            .GetComponent<TMP_InputField>().text;
        credentials.password = GameObject.Find("ContraseñaInput")
            .GetComponent<TMP_InputField>().text;

        string postDataJson = JsonUtility.ToJson(credentials);

        StartCoroutine(LoginPost(postDataJson));
    }

    IEnumerator RegisterPost(string postDataJson)
    {
        string path = "/api/usuarios";
        UnityWebRequest request = UnityWebRequest.Put(url + path, postDataJson);
        request.method = "POST";
        request.SetRequestHeader("Content-Type", "application/json");
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.LogError("Error de conexión: " + request.error);
            ShowErrorMessage("Error de conexión. Intenta de nuevo más tarde.");
        }
        else
        {
            switch (request.responseCode)
            {
                case 200: // Registro exitoso
                    Debug.Log("Registro exitoso");
                    StartCoroutine(LoginPost(postDataJson));
                    PlayerPrefs.DeleteKey("HighscoreAmount");
                    break;

                case 400: // Solicitud incorrecta
                    Debug.LogError("Error: Datos inválidos o faltantes.");
                    Debug.Log(request.responseCode + "|" + request.error);
                    ShowErrorMessage("Por favor, verifica los datos ingresados.");
                    break;

                case 409: // Conflicto (usuario ya existe)
                    Debug.LogError("Error: El usuario ya existe.");
                    Debug.Log(request.responseCode + "|" + request.error);
                    ShowErrorMessage("El nombre de usuario ya está en uso.");
                    break;

                case 500: // Error interno del servidor
                    Debug.LogError("Error interno del servidor.");
                    Debug.Log(request.responseCode + "|" + request.error);
                    ShowErrorMessage("Error del servidor. Intenta de nuevo más tarde.");
                    break;

                default:
                    Debug.LogError("Error desconocido: " + request.responseCode);
                    ShowErrorMessage("Error desconocido. Intenta de nuevo.");
                    break;
            }
        }
    }
    IEnumerator LoginPost(string postDataJson)
    {
        string path = "/api/auth/login";
        UnityWebRequest request = UnityWebRequest.Put(url + path, postDataJson);
        request.method = "POST";
        request.SetRequestHeader("Content-Type", "application/json");
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.LogError("Error de conexión: " + request.error);
            ShowErrorMessage("Error de conexión. Intenta de nuevo más tarde.");
        }
        else
        {
            switch (request.responseCode)
            {
                case 200: // Login exitoso
                    string json = request.downloadHandler.text;
                    AuthResponse response = JsonUtility.FromJson<AuthResponse>(json);
                    Token = response.token;
                    Username = response.usuario.username;
                    PlayerPrefs.SetString("token", Token);
                    PlayerPrefs.SetString("username", Username);
                    PlayerPrefs.Save();
                    CheckAuthentication();
                    loginPanel.SetActive(false);
                    startPanel.SetActive(true);
                    Debug.Log("Login exitoso");
                    break;

                case 400: // Solicitud incorrecta
                    Debug.LogError("Error: Datos inválidos o faltantes.");
                    ShowErrorMessage("Por favor, verifica los datos ingresados.");
                    break;

                case 401: // No autorizado
                    Debug.LogError("Error: Usuario o contraseña incorrectos.");
                    ShowErrorMessage("Usuario o contraseña incorrectos.");
                    break;

                case 500: // Error interno del servidor
                    Debug.LogError("Error interno del servidor.");
                    ShowErrorMessage("Error del servidor. Intenta de nuevo más tarde.");
                    break;

                default:
                    Debug.LogError("Error desconocido: " + request.responseCode);
                    ShowErrorMessage("Error desconocido. Intenta de nuevo.");
                    break;
            }
        }
    }

    public void Register()
    {
        AuthResponse credentials = new AuthResponse();
        credentials.username = GameObject.Find("UsuarioInput")
            .GetComponent<TMP_InputField>().text;
        credentials.password = GameObject.Find("ContraseñaInput")
            .GetComponent<TMP_InputField>().text;

        string postDataJson = JsonUtility.ToJson(credentials);

        StartCoroutine(RegisterPost(postDataJson));

    }
    IEnumerator GetProfile()
    {
        string path = "/api/usuarios";
        UnityWebRequest www = UnityWebRequest.Get(url + path);
        www.SetRequestHeader("x-token", Token);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(www.error);
        }
        else
        {
            if (www.responseCode == 200)
            {
                string json = www.downloadHandler.text;
                Debug.Log("Respuesta de la API: " + json);

                // Deserializar la respuesta de la API
                UsuariosResponse response = JsonUtility.FromJson<UsuariosResponse>(json);

                if (response != null && response.usuarios != null)
                {
                    // Buscar el usuario específico por su username
                    UserModel usuario = response.usuarios.Find(u => u.username == Username);

                    if (usuario != null)
                    {
                        Debug.Log("El usuario " + usuario.username + " se encuentra autenticado.");

                        // Verificar si el usuario tiene un puntaje
                        if (usuario.data != null)
                        {
                            Debug.Log("Puntaje: " + usuario.data.score);

                            if (PlayerPrefs.GetInt("HighscoreAmount") < usuario.data.score)
                            {
                                PlayerPrefs.SetInt("HighscoreAmount", usuario.data.score);
                                Highscore.SetAmount(PlayerPrefs.GetInt("HighscoreAmount"));
                            }
                        }
                        else
                        {
                            Debug.LogWarning("El usuario no tiene datos de puntaje.");
                        }

                        // Actualizar el puntaje si es necesario
                        UserModel model = new UserModel
                        {
                            data = new DataUser(),
                            username = Username
                        };

                        int amount = Highscore.GetAmount();
                        if (amount > model.data.score)
                        {
                            model.data.score = amount;
                            Debug.Log(JsonUtility.ToJson(model));
                            StartCoroutine("SetScore", JsonUtility.ToJson(model));
                        }
                    }
                    else
                    {
                        Debug.LogError("Error: No se encontró el usuario con el username " + Username);
                    }
                }
                else
                {
                    Debug.LogError("Error: La respuesta de la API no contiene datos válidos.");
                }
            }
            else
            {
                Debug.Log("Token Vencido... redireccionar a Login");
            }
        }
    }
    public void ShowLeaderboard()
    {
        StartCoroutine(GetLeaderboard());
    }
    IEnumerator GetLeaderboard()
    {
        UnityWebRequest request = UnityWebRequest.Get(url + "/api/usuarios");
        Debug.Log("Sending Request GetLeaderboard");
        request.SetRequestHeader("x-token", Token);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(request.error);
        }
        else
        {
            if (request.responseCode == 200)
            {
                UsuariosResponse data = JsonUtility.FromJson<UsuariosResponse>(request.downloadHandler.text);

                var orderedUsers = data.usuarios.OrderByDescending(user => user.data.score).Take(5).ToArray();
                ShowLeaderboard(orderedUsers);
            }
            else
            {
                Debug.Log(request.responseCode + "|" + request.error);
                Debug.Log("Usuario no autenticado");
            }
        }
    }

    private void ShowLeaderboard(UserModel[] orderedUsers)
    {
        leaderboardPanel.SetActive(true);
        for (int i = 0; i < orderedUsers.Length; i++)
        {
            usernamesText[i].text = orderedUsers[i].username;
            scoresText[i].text = orderedUsers[i].data.score.ToString();
            Debug.Log($"{orderedUsers[i].data.score}");
        }
    }
    public void GetScore(int score)
    {
        Debug.Log("Preparando datos para enviar a la API...");
        UserModel user = new UserModel
        {
            username = Username,
            data = new DataUser()
            {
                score = score
            }

        };
        Debug.Log("Datos del usuario creados: " + JsonUtility.ToJson(user));
        string jsonData = JsonUtility.ToJson(user);
        //StartCoroutine(SetScore(jsonData));
    }
    IEnumerator SetScore(string postDataJson)
    {
        string path = "/api/usuarios";
        UnityWebRequest request = UnityWebRequest.Put(url + path, postDataJson);
        request.method = "PATCH";
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("x-token", Token);
        yield return request.SendWebRequest();
        Debug.Log("Datos enviados: " + postDataJson);
        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(request.error);
        }
        else 
        {
            Debug.Log(request.downloadHandler.text);
            if (request.responseCode == 200)
            {
                string json = request.downloadHandler.text;
                Debug.Log("Respuesta de la API recibida: " + json);
                AuthResponse data = JsonUtility.FromJson<AuthResponse>(json);
                Debug.Log($"el usuario {data.usuario.username} se actualizo y ahora su puntaje es {data.usuario.data.score}");
            }
        }
    }
    private void ShowErrorMessage(string message)
    {
        errorPanel.SetActive(true);
        errorText.text = message;
        StartCoroutine(HideErrorMessageAfterDelay(3f));
    }
    private IEnumerator HideErrorMessageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (errorPanel != null)
        {
            errorPanel.SetActive(false);
        }
    }
    private bool IsTokenValid()
    {
        if (!PlayerPrefs.HasKey("token"))
        {
            Debug.Log("No hay token almacenado.");
            return false;
        }

        string token = PlayerPrefs.GetString("token");
        return true;
    }
    private void CheckAuthentication()
    {
        Token = PlayerPrefs.GetString("token");
        Username = PlayerPrefs.GetString("username");
        if (!IsTokenValid())
        {
            Debug.Log("Usuario no autenticado. Redirigiendo a la pantalla de autenticación...");
            RedirectToLogin();
        }
        else
        {
            Debug.Log("Usuario autenticado. Acceso permitido.");
            loginPanel.SetActive(false);
            startPanel.SetActive(true);
            StartCoroutine(GetProfile());
        }
    }
    public void Logout()
    {
        PlayerPrefs.DeleteKey("token");
        PlayerPrefs.Save(); // Guardar los cambios
        Application.Quit();
        Debug.Log("Sesión cerrada. Token eliminado.");
    }
    public void RedirectToLogin()
    {
        startPanel.SetActive(false);
        leaderboardPanel.SetActive(false);
        loginPanel.SetActive(true);
    }

}
[System.Serializable]
public class AuthResponse
{
    public string username;
    public string password;
    public UserModel usuario;
    public string token;
}
[System.Serializable]
public class UserModel
{
    public string _id;
    public string username;
    public DataUser data;
    public string estado;
}
[System.Serializable]
public class DataUser
{
    public int score;
}
[System.Serializable]
public class UsuariosResponse
{
    public List<UserModel> usuarios;
}
