using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Threading.Tasks;
using NWaves.Signals;

public class GrainSpawner : TsvrTool
{   
    public new TsvrToolType ToolType = TsvrToolType.GrainSpawner;
    public FileListUI fileListUI;

    // public InputActionMap ToolActions;

    private TwistLockAction twistLockModelScale;
    private TwistLockAction twistLockModelDist;


    private DiscreteSignal audioBuffer;
    private bool isSpawnReady = false;

    private float modelScale = 1f;
    private float modelDist = 0.5f;

    public void OnEnable() {

        // var files = TsvrApplication.AudioManager.GetDefaultAudioFiles();
        fileListUI.SetDirectory(TsvrApplication.AudioManager.GetDefaultAudioFilePath(), "wav");
        
        if (animations != null) {
            Debug.Log("Playing equip animation");
            animations.Play("GrainSpawnerEquip");
        }

        SubscribeActions();
        // StartCoroutine(DebugSpawnDelayed());
    }

    public void OnDisable() {
        UnsubscribeActions();
        if (animations != null) {
            Debug.Log("Playing unequip animation");
            // animations.Play("UnequipPlayWand");
        }
    }

    void SubscribeActions() {
        twistLockModelScale = new TwistLockAction(
            -180f, 180f, 
            1f, 10f,
            modelScale,
            TsvrApplication.Settings.wandDistIncrement,// <- eventually set these from global parameters
            ControllerActions.twistLock.action,
            ControllerActions.rotationAction.action,
            ChangeModelScale,
            ControllerActions.Hand == ControllerHand.Left
        );

        // changes the size of the wand
        // twistLockModelSize = new TwistLockAction(
        //     -180f, 180f,
        //     TsvrApplication.Settings.WandMinRadius, TsvrApplication.Settings.WandMaxRadius,
        //     0.5f,
        //     TsvrApplication.Settings.wandDistIncrement,
        //     ControllerActions.toolOptionButton.action, // <- eventually set this from global parameters
        //     ControllerActions.rotationAction.action,
        //     ChangeModelScale,
        //     ControllerActions.Hand == ControllerHand.Left
        // );



        ControllerActions.toolAxis2D.action.started += CycleSelection;
        ControllerActions.toolOptionButton.action.started += SpawnSelectedFile;
        // m_ControllerActions.SubscribeAction(m_ToolController.m_ControllerActions.grainSpawnerAction, OnGrainSpawnerAction);
    }

    void UnsubscribeActions() {
        twistLockModelScale.UnsubscribeActions();
        ControllerActions.toolAxis2D.action.started -= CycleSelection;
        ControllerActions.toolOptionButton.action.started -= SpawnSelectedFile;
        // m_ControllerActions.UnsubscribeAction(m_ToolController.m_ControllerActions.grainSpawnerAction, OnGrainSpawnerAction);
    }

    void Execute(InputAction.CallbackContext ctx) {
        switch(ctx.action.name) {
            case "GrainSpawnerAction":
                Debug.Log("Grain spawner action");
                break;
            default:
                Debug.Log("Unknown action");
                break;
        }
        Debug.Log("Joystick select" + ctx.ReadValue<Vector2>());
    }

    void CycleSelection(InputAction.CallbackContext ctx) {
        Debug.Log("Joystick select" + ctx.ReadValue<Vector2>());
        Vector2 joystick = ctx.ReadValue<Vector2>();
        if (joystick.y < 0f) {
            Debug.Log("Joystick select");
            fileListUI.NextFile();
        } else if (joystick.y > 0f) {
            Debug.Log("Joystick select");
            fileListUI.PreviousFile();
        }
    }

    void ChangeModelScale(float scale) {
        // modelScale = scale * 0.01f;
        // modelScale = Mathf.Clamp(modelScale, 0.01f, 0.5f);
        modelScale = scale;
        Debug.Log("Model scale: " + modelScale);
    }

    void ChangeModelDist(float dist) {
        modelDist += dist * 0.01f;
        modelDist = Mathf.Clamp(modelDist, 0.01f, 0.5f);
        Debug.Log("Model dist: " + modelDist);
    }

    async void SpawnSelectedFile(InputAction.CallbackContext ctx) {
        animations.Play("GrainSpawnerButton");
        GameObject newModel = Instantiate(
            TsvrApplication.Config.grainModel, 
            GameObject.Find("GrainParent").transform);
        GrainModel grainModel = newModel.GetComponent<GrainModel>();
        grainModel.Initialize(new Vector3(0,1,1), modelScale);

        Task<DiscreteSignal> loadAudio = Task.Run(() => {
            return AudioIO.ReadMonoAudioFile(fileListUI.CurrentlySelectedFile.FullName);
        });

        await loadAudio.ContinueWith(t => {
            Debug.Log($"Loaded Audio | Num Samples: {t.Result.Length}.");
            audioBuffer = t.Result;
            isSpawnReady = true;
        });
        StartCoroutine(SpawnAfterAudioLoad(grainModel));
        // selectedModel = gm;
    }

    IEnumerator SpawnAfterAudioLoad(GrainModel model) {
        yield return new WaitUntil(() => isSpawnReady);
        model.SetAudioBuffer(audioBuffer);
    }
}
