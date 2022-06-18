using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static InputManager InputManager;

    void Awake()
    {
        InputManager = GetComponent<InputManager>();
        //DontDestroyOnLoad(gameObject);
    }
}