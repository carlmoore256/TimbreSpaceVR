using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Threading.Tasks;
using NWaves.Signals;
using System;

public class GrainSpawner : TsvrTool
{   
    public override TsvrToolType ToolType { get => TsvrToolType.GrainSpawner; }
    public FileListUI fileListUI;

    // public InputActionMap ToolActions;

    private TwistLockAction twistLockModelScale;
    private TwistLockAction twistLockModelDist;


    private DiscreteSignal audioBuffer;
    private bool isSpawnReady = false;

    private float modelScale = 0.5f;
    private float modelDist = 1.5f;

    private GrainModelOld newGrainModel;

    public void OnEnable() {

        // var files = TsvrApplication.AudioManager.GetDefaultAudioFiles();
        // fileListUI.SetDirectory(TsvrApplication.AudioManager.GetDefaultAudioFilePath(), "wav");
        fileListUI.SetToBuiltinFiles();

        if (animations != null) {
            Debug.Log("Playing equip animation");
            animations.Play("GrainSpawnerEquip");
        }

        SubscribeActions();
        // StartCoroutine(DebugSpawnDelayed());
        newGrainModel = Instantiate(TsvrApplication.Config.grainModel, GameObject.Find("GrainParent").transform).GetComponent<GrainModelOld>();
    }

    public void OnDisable() {
        if (newGrainModel.State == GrainModelState.Unplaced && newGrainModel != null)
            Destroy(newGrainModel.gameObject);

        UnsubscribeActions();
        if (animations != null) {
            Debug.Log("Playing unequip animation");
            // animations.Play("UnequipPlayWand");
        }
    }

    void SubscribeActions() {

        // var options = new TwistLockOptions(
        //     minAngle: -180f, maxAngle: 180f,
        //     minValue: 0.3f, maxValue: 5f,
        //     initValue: modelScale, incrementAmount: TsvrApplication.Settings.WandDistIncrement,
        //     reverse: ControllerActions.Hand == ControllerHand.Left
        // );

        // twistLockModelScale = new TwistLockAction(
        //     options,
        //     ControllerActions.twistLock.action,
        //     ControllerActions.rotationAction.action,
        //     (scale) => modelScale = scale
        // );

        ControllerActions.toolAxis2D.action.started += CycleSelection;
        ControllerActions.toolOption.action.started += SpawnSelectedFile;
        // m_ControllerActions.SubscribeAction(m_ToolController.m_ControllerActions.grainSpawnerAction, OnGrainSpawnerAction);
    }

    void UnsubscribeActions() {
        twistLockModelScale.UnsubscribeActions();
        ControllerActions.toolAxis2D.action.started -= CycleSelection;
        ControllerActions.toolOption.action.started -= SpawnSelectedFile;
        // m_ControllerActions.UnsubscribeAction(m_ToolController.m_ControllerActions.grainSpawnerAction, OnGrainSpawnerAction);
    }

    void Update() {
        
        if (newGrainModel.State == GrainModelState.Unplaced) {
            newGrainModel.Reposition(
                ToolController.transform.position + ToolController.transform.forward * modelDist, 
                ToolController.transform.rotation, 
                new Vector3(modelScale, modelScale, modelScale)
            );
        }
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

        // newGrainModel.ChangeState(GrainModelState.Placed);
        newGrainModel.Place();

        Debug.Log("Spawn selected file " + fileListUI.CurrentlySelectedFile.Name);
        if (fileListUI.isBrowsingBuiltInFiles) {
            AudioIO.LoadAudioFromResources("Audio/" + fileListUI.CurrentlySelectedFile.Name, (signal) => {
                Debug.Log($"Loaded Audio | Num Samples: {signal.Length}.");
                newGrainModel.SetAudioBuffer(signal);
            });
            // AudioIO.LoadAddressableAudioClip("Audio/" + fileListUI.CurrentlySelectedFile.Name, (signal) => {
            //     Debug.Log($"Loaded Audio | Num Samples: {signal.Length}.");
            //     newGrainModel.SetAudioBuffer(signal);
            // });
        } else {
            Task<DiscreteSignal> loadAudio = Task.Run(() => {
                return AudioIO.ReadMonoAudioFile(fileListUI.CurrentlySelectedFile.FullName);
            });

            await loadAudio.ContinueWith(t => {
                Debug.Log($"Loaded Audio | Num Samples: {t.Result.Length}.");
                audioBuffer = t.Result;
                isSpawnReady = true;
            });

            StartCoroutine(SetGrainModelBuffer(newGrainModel, () => {
                newGrainModel = Instantiate(TsvrApplication.Config.grainModel, GameObject.Find("GrainParent").transform).GetComponent<GrainModelOld>();
            }));
        }

        // newGrainModel = Instantiate(TsvrApplication.Config.grainModel, GameObject.Find("GrainParent").transform).GetComponent<GrainModel>();
    }

    IEnumerator SetGrainModelBuffer(GrainModelOld model, Action onComplete = null) {
        yield return new WaitUntil(() => isSpawnReady);
        model.SetAudioBuffer(audioBuffer);
        onComplete?.Invoke();
    }
}




// twistLockModelScale = new TwistLockAction(
//     -180f, 180f, 
//     0.3f, 5f,
//     modelScale,
//     TsvrApplication.Settings.WandDistIncrement.value,// <- eventually set these from global parameters
//     ControllerActions.twistLock.action,
//     ControllerActions.toolOption.action,
//     (scale) => modelScale = scale,
//     ControllerActions.Hand == ControllerHand.Left
// );

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