using UnityEngine;
using System.IO;
using NWaves.Signals;
using NWaves.Audio;

public class AudioIO {
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