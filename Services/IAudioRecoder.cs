using System.Threading;
using System.Threading.Tasks;

namespace LocalLlmDesktop.Services;

public interface IAudioRecorder
{
    bool StartRecording();
    Task<string> StopRecordingAsync(CancellationToken cancellationToken);
}