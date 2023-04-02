using UnityEngine;

public interface ISequenceable {
    // Sequenceable GetSequenceable();
    void Play(float gain);
    public int ID { get; }
}

public interface IPositionedSequenceable : ISequenceable
{
    Vector3 Position { get; }
}
// public class Sequenceable {
//     // provide an api for anything including grains and clouds to be sequenced

// }