from pydub import AudioSegment
def get_audio_info(file):
    audio_file = AudioSegment.from_wav(file)
    return {
        "duration": round(audio_file.duration_seconds, 4),
        "channels": audio_file.channels,
    }