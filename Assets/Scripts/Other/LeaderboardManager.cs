using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager Instance;
    private string usuariosUrl = "https://sid-restapi.onrender.com/api/usuarios";
    private UserModel User;
    private string Token;

    [SerializeField] private GameObject leaderboardPanel;

    void Start()
    {
        Token = PlayerPrefs.GetString("token");
    }

    
}

