using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// spawn and play grains from a grain model
public class TestGrainModel : MonoBehaviour
{

    public string audioFile = "mgmt";

    private GrainModel grainModel;

    // Start is called before the first frame update
    void OnEnable()
    {
        grainModel = SpawnLoadGrainModel(audioFile);
        grainModel.PlaySequentially();
    }

    private GrainModel SpawnLoadGrainModel(string path) {
        GrainModel newGrainModel = Instantiate(TsvrApplication.Config.grainModel, GameObject.Find("GrainParent").transform).GetComponent<GrainModel>();
        AudioIO.LoadAudioFromAssets("SamplePacks/tsvr-classic/" + audioFile, (signal) => {
            Debug.Log($"Loaded Audio | Num Samples: {signal.Length}.");
            newGrainModel.SetAudioBuffer(signal);
        });
        return newGrainModel;
    }

    int frameIndex = 0;
    int featureIndex = 0;
    void Update() {
        frameIndex++;
        // if (frameIndex % 30 == 0) {
        //     switch(featureIndex) {
        //         case 0:
        //             grainModel.Parameters.XFeature = AudioFeature.Contrast_0;
        //             break;
        //         case 1:
        //             grainModel.Parameters.YFeature = AudioFeature.Energy;
        //             break;
        //         case 2:
        //             grainModel.Parameters.XFeature = AudioFeature.RMS;
        //             break;
        //         case 3:
        //             grainModel.Parameters.YFeature = AudioFeature.MFCC_6;
        //             break;
        //         case 4:
        //             grainModel.Parameters.XFeature = AudioFeature.MFCC_2;
        //             break;
        //         case 5:
        //             grainModel.Parameters.YFeature = AudioFeature.MFCC_3;
        //             break;
        //         case 6:
        //             grainModel.Parameters.XFeature = AudioFeature.MFCC_4;
        //             break;
        //         case 7:
        //             grainModel.Parameters.YFeature = AudioFeature.MFCC_5;
        //             break;
        //     }
            
        //     featureIndex = (featureIndex + 1) % 7;
        // }
    }
    // void Update()
    // {}
}
