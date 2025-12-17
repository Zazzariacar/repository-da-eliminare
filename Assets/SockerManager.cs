using System;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SockerManager : MonoBehaviour
{
    public static SockerManager Instance;

    [SerializeField, ReadOnly(true)] int punti = 0;
    [SerializeField] int puntiPerVittoria = 5;
    [SerializeField] string pallaTag = "palla";
    [SerializeField] string pallaSpawnPointTag = "pallaSpawnPoint";

    GameObject pallaObject;
    GameObject pallaSpawnPoint;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance && Instance != this)
        {
            Destroy(gameObject);
        }
        SceneManager.sceneLoaded += RefreshLevelReferences;
    }
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= RefreshLevelReferences;
    }

    private void RefreshLevelReferences(Scene scene, LoadSceneMode loadSceneMode)
    {
        pallaObject = GameObject.FindGameObjectWithTag(pallaTag);
        pallaSpawnPoint = GameObject.FindGameObjectWithTag(pallaSpawnPointTag);
        ResetPalla();


    }

    public void Gol(int _punti)
    {
        punti += _punti;
        Debug.Log("Punti: " + _punti);
        if (punti >= puntiPerVittoria) ResetPartita();
    }
    private void ResetPartita()
    {


        punti = 0;
        ResetPalla();
        Debug.Log("Hai vinto!");

    }
    public void ResetPalla()
    {
        if (pallaObject != null && pallaSpawnPoint)
        {
            pallaObject.transform.position = pallaSpawnPoint.transform.position;
            Rigidbody pallaB = pallaObject.GetComponent<Rigidbody>();
            pallaB.linearVelocity = Vector3.zero;
            pallaB.angularVelocity = Vector3.zero;
        }
    }
}