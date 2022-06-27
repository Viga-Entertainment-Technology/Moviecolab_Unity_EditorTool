using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Text;
using System;
using System.Linq;
using SimpleJSON;
using ServerResponse;


public class MovieCollab_Login : EditorWindow
{

    string login_ID = "";
    string Password = "";
    bool button;    

    [MenuItem("Tools/MovieCollab")]

    public static void ShowWindow()
    {
        EditorWindow window1 = GetWindow<MovieCollab_Login>("Movie Collab");
        window1.Show();
    }
    private void OnGUI()
    {
        if (EditorPrefs.HasKey("RefreshToken"))
        {
            Switchwindow();
        }
        else
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.LabelField("User_Login", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            login_ID = EditorGUILayout.TextField("Login_ID", login_ID);
            Password = EditorGUILayout.PasswordField("Password", Password);
            button = GUILayout.Button("Login");
            if (button)
            {
                if (string.IsNullOrWhiteSpace(login_ID) || string.IsNullOrEmpty(login_ID) || string.IsNullOrWhiteSpace(Password) || string.IsNullOrEmpty(Password) || login_ID.Contains(" ") || Password.Contains(" "))
                {
                    EditorUtility.DisplayDialog("Empty Field OR Invalid Character! ", "Please Fill The Email and Password Correctly,Whitespace Characters Are Invalid", "OK");
                }
                else
                {
                    ServerLinks.RequestWebAction(login_ID, Password, Login_success).ConfigureAwait(true);
                }
            }
        }
    }

    public void Login_success(string[] str)
    {
        //Debug.Log(str[0]);
        //Debug.Log(str[1]);
        EditorPrefs.SetString("AccessToken", str[0]);
        EditorPrefs.SetString("RefreshToken", str[1]);

    }

    public void Switchwindow()
    {
        EditorWindow window2 = GetWindow<MovielCollab>("Movie Collab Editor", typeof(MovieCollab_Login));
        window2.Show();
        this.Close();
    }
}