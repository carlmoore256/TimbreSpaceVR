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

    [SerializeField]
    private DebugLogger _debugLogger;
    public static DebugLogger DebugLogger => Instance._debugLogger;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void FirstInitialize() {
        // anything that happens before awake here
        if (Instance._audioManager == null) {
            Instance._audioManager = Resources.Load<AudioManager>("ScriptableObjects/TSVR_AudioManager");
        }

        if (Instance._settings == null) {
            Instance._settings = Resources.Load<AppSettings>("ScriptableObjects/TSVR_AppSettings");
        }

        if (Instance._debugLogger == null) {
            Instance._debugLogger = Resources.Load<DebugLogger>("ScriptableObjects/TSVR_DebugLogger");
        }

        // Debug.Log(Application.persistentDataPath);
    }
}
