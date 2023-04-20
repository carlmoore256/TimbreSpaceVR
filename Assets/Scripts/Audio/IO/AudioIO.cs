using UnityEngine;
using System.IO;
using NWaves.Signals;
using NWaves.Operations;
using NWaves.Audio;
using UnityEngine.Networking;
using System;
// using UnityEngine.AddressableAssets;
using System.Threading.Tasks;

public class AudioIO {

    public static DiscreteSignal AudioClipToDiscreteSignal(AudioClip clip, bool resample = true) {
        float[] samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);
        DiscreteSignal signal = new DiscreteSignal(clip.frequency, samples);
        // if (signal.SamplingRate != TsvrApplication.AudioManager.SampleRate && resample) {
        //     Debug.Log("Resampling audio file from " + signal.SamplingRate + " to " + TsvrApplication.AudioManager.SampleRate);
        //     signal = new Resampler().Resample(signal, TsvrApplication.AudioManager.SampleRate);
        // }
        return signal;
    }

    public static void LoadAudioFromResources(string path, Action<DiscreteSignal> callback, bool resample = true) {
        var handler = Resources.LoadAsync<AudioClip>(path);
        Debug.Log("Loading audio file: " + path);
        var currentTime = Time.timeAsDouble;
        handler.completed += (op) => {
            AudioClip clip = handler.asset as AudioClip;
            if (clip == null) {
                Debug.LogError("Failed to load audio file: " + path);
                return;
            }
            var signal = AudioClipToDiscreteSignal(clip);
            Debug.Log($"Finished loading audio file in {Time.timeAsDouble - currentTime} seconds");
            callback(signal);
        };
        // callback(AudioClipToDiscreteSignal(clip));
    }

    public static Task<DiscreteSignal> LoadAudioFromResources(string path, bool resample = true) {
        var handler = Resources.LoadAsync<AudioClip>(path);
        Debug.Log("Loading audio file: " + path);
        var currentTime = Time.timeAsDouble;
        var task = new TaskCompletionSource<DiscreteSignal>();
        handler.completed += (op) => {
            AudioClip clip = handler.asset as AudioClip;
            if (clip == null) {
                Debug.LogError("Failed to load audio file: " + path);
                task.SetResult(default);
                return;
            }
            var signal = AudioClipToDiscreteSignal(clip);
            Debug.Log($"Finished loading audio file in {Time.timeAsDouble - currentTime} seconds");
            task.SetResult(signal);
        };
        return task.Task;
    }

    // public static async void LoadAddressableAudioClip(string path, Action<DiscreteSignal> callback) {
    //     var resourceRequest = Addressables.LoadAssetAsync<AudioClip>(path);
    //     resourceRequest.Completed += (op) => {
    //         if (resourceRequest.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded) {
    //             AudioClip clip = resourceRequest.Result;
    //             callback(AudioClipToDiscreteSignal(clip));
    //         }
    //     };
    //     await resourceRequest.Task;
    // }

    public static async Task<DiscreteSignal> LoadAudioFromPersistentData(string relpath) {
        var path = Path.Combine(Application.persistentDataPath, relpath);
        return await LoadAudioFromURI(path);
    } 

    public static async Task<DiscreteSignal> LoadAudioFromURI(string path)
    {
        // if system is not windows, prefix with file://	
        if (Application.platform != RuntimePlatform.WindowsEditor && Application.platform != RuntimePlatform.WindowsPlayer) {	
            path = "file://" + path;	
        }
        UnityWebRequest webRequest = UnityWebRequest.Get(path);
        webRequest.timeout = 160;
        var operation = webRequest.SendWebRequest();
        while (!operation.isDone)
        {
            Debug.Log($"[*] Downloading audio from {path} | Progress: {webRequest.downloadProgress}");
            await System.Threading.Tasks.Task.Delay(300);
        }

        if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error downloading audio file: " + webRequest.error);
            return default; 
        }

        DiscreteSignal signal = null;

        using (MemoryStream stream = new MemoryStream(webRequest.downloadHandler.data))
        {
            var waveFile = new WaveFile(stream);
            if (waveFile.Signals.Count == 0)
            {
                Debug.LogError("Invalid number of channels for audio file: " + path);
            }

            signal = waveFile[Channels.Average];
        }

        // callback?.Invoke(signal);
        return signal;
    }

    public static DiscreteSignal ReadMonoAudioFile(string path) {
        DiscreteSignal signalMono;
        Debug.Log("Reading audio file: " + path);
        using (var stream = new FileStream(path, FileMode.Open))
        {
            var waveFile = new WaveFile(stream);
            if (waveFile.Signals.Count == 0) {
                Debug.LogError("Invalid number of channels for audio file: " + path);
                return null;
            }
            signalMono = waveFile[Channels.Average];
        }
        Debug.Log("Finished reading audio file!");
        return signalMono;
    }

    public static DiscreteSignal[] ReadStereoAudioFile(string path) {
        DiscreteSignal[] signalStereo = new DiscreteSignal[2];
        using (var stream = new FileStream(path, FileMode.Open))
        {
            var waveFile = new WaveFile(stream);
            if (waveFile.Signals.Count == 0) {
                Debug.LogError("Invalid number of channels for audio file: " + path);
                return null;
            }
            if (waveFile.Signals.Count == 1)
            {
                signalStereo[0] = waveFile[Channels.Average];
                signalStereo[1] = waveFile[Channels.Average];
            }
            else if (waveFile.Signals.Count >= 2) {
                signalStereo[0] = waveFile[Channels.Left];
                signalStereo[1] = waveFile[Channels.Right];
            }
        }
        return signalStereo;
    }

    public static void WriteAudioFileMono(string path, DiscreteSignal signalMono) {
        using (var stream = new FileStream(path, FileMode.Create))
        {
            var waveFile = new WaveFile(signalMono);
            waveFile.SaveTo(stream);
        }
    }

    public static void WriteAudioFileStereo(string path, DiscreteSignal[] signalStereo) {
        using (var stream = new FileStream(path, FileMode.Create))
        {
            var waveFile = new WaveFile(signalStereo);
            waveFile.SaveTo(stream);
        }
    }
}


// public async void ReadAudioBuffered(string path, int bufferSize, Action<DiscreteSignal> callback) {
//     byte[] result;
//     using (FileStream stream = File.OpenRead(path)) {
//         int byteIdx = 0;
//         while(byteIdx < stream.Length) {
//             // stream.Seek(byteIdx, SeekOrigin.Begin);
//             int bytesToRead = Math.Min(bufferSize, (int)stream.Length - byteIdx);
//             byte[] buffer = new byte[bytesToRead];
//             await stream.ReadAsync(buffer, byteIdx, bytesToRead);
//             byteIdx += bytesToRead;
//             // callback.Invoke(new WaveFile(buffer));
//          }
//     } 
// }

// public static void CaptureMicInput()