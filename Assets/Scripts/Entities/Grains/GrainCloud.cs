using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NWaves.Signals;

public class GranularBuffer {

    DiscreteSignal audioBuffer;

    AudioFeatureExtractor featureExtractor;

    public void PlayEvent(PlaybackEvent e) {
        // trigger a playback event for the buffer
    }

    public void ComputeFeatures() {

    }

    public float GetFeatureValue(AudioFeature feature, int index, bool normalized = true) {
        // here we can handle if a feature has not yet been computed
        // return featureExtractor.FeatureVectors[feature].GetNormalized(index, 0, 1);
        if (normalized)
            return featureExtractor.GetFeatureVector(feature).GetNormalized(index, 0, 1);

        return featureExtractor.GetFeatureVector(feature)[index];
    }
}


public class GrainCloud : MonoBehaviour {

    GranularBuffer granularBuffer;

    // grainCloud can ask granularBuffer for what kind of features
    // it has, depending on the parameter handler
    // granularBuffer handles the processing of audio and keeps
    // track of computed features

    // after asking, graincloud (which is in charge of grains) can
    // broadcast the updates to the grains

    // when we spawn grains, we only give them a playback event.
    // grain will have a delegate that we can subscribe to here in grain cloud
    // therefore, grain has no idea about any audio features, parameter handlers
    // it only has access to a playback event, that will always hold true for its
    // entire lifetime

    private ParameterHandler parameterHandler;

    private List<Grain> grains;

    public void InitializeParameters(GrainCloudParameterValues parameterValues) {
        // set the parameter handler
        // parameter handler will be in charge of updating the grains
        // with the new parameter values

        if (parameterHandler != null) {
            parameterHandler.onFeaturePosUpdate -= UpdateGrainPositions;
            // parameterHandler.onFeatureColUpdate -= OnFeatureColUpdate;
            // parameterHandler.onFeatureSclUpdate -= onFeatureSclUpdate;
        }
        
        parameterHandler = new ParameterHandler(parameterValues);
    }

    private void UpdateGrainPositions(AudioFeature[] features, Vector3 axisScale) {

        // first, ask the GranularBuffer to compute new features,
        // or rather, just get features...
        
        foreach(Grain grain in grains) {
            // basically, get some info about what id the grain has to index
            // into the audioFeatures. GranularBuffer can have some sort of
            // API for this. You provide AudioFeature, and an index, and you
            // get back a value. GranularBuffer can handle computing those if
            // needed

            Vector3 newPos = new Vector3(
                granularBuffer.GetFeatureValue(features[0], grain.GrainIndex),
                granularBuffer.GetFeatureValue(features[1], grain.GrainIndex),
                granularBuffer.GetFeatureValue(features[2], grain.GrainIndex)
            );
            grain.UpdatePosition(newPos);
        }
        
    }

    private void UpdateGrainColors(AudioFeature[] features, bool useHSV) {

        foreach(Grain grain in grains) {

        }
    }


    

}