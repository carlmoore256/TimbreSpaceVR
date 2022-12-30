using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "TSVR_AppManager", menuName = "TSVR/Application (Singleton)")]
public class TsvrApplication : SingletonScriptableObject<TsvrApplication>
{
    [SerializeField]
    private AudioManager _audioManager;
    public static AudioManager AudioManager => Instance._audioManager; 

    [SerializeField]
    private AppSettings _settings; 
    public static AppSettings Settings => Instance._settings;

    [SerializeField]
    private AppConfig _config;
    public static AppConfig Config => Instance._config;


    [SerializeField] 
    private InputActionAsset _inputActions;
    public static InputActionAsset InputActions => Instance._inputActions;

    // [SerializeField]
    // private GameObject _DefaultXRRig;
    // public static GameObject DefaultXRRig => Instance._DefaultXRRig;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void FirstInitialize() {
        // anything that happens before awake here
        if (Instance._audioManager == null) {
            Instance._audioManager = Resources.Load<AudioManager>("ScriptableObjects/TSVR_AudioManager");
        }

        if (Instance._settings == null) {
            Instance._settings = Resources.Load<AppSettings>("ScriptableObjects/TSVR_AppSettings");
        }

        // Debug.Log(Application.persistentDataPath);
    }
}
