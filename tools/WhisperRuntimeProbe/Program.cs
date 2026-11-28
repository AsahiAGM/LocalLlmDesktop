using Whisper.net;
using Whisper.net.LibraryLoader;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

// CUDA以外のランタイムを候補から除外する。
// CUDAが利用できない場合は初期化に失敗させる。
RuntimeOptions.RuntimeLibraryOrder =
[
    RuntimeLibrary.Cuda
];

// リポジトリルートにある検証用モデルを指定する。
var modelPath = Path.GetFullPath(
    Path.Combine(
        "..",
        "..",
        "STT_Models",
        "test",
        "ggml-tiny.bin"
    )
);

Console.WriteLine($"Model: {modelPath}");

if (!File.Exists(modelPath))
{
    Console.WriteLine("ERROR: Model file not found.");
    return;
}

// 入力音声と変換後音声のパス
var audioPath = Path.GetFullPath(
    Path.Combine(
        "..",
        "..",
        "STT_Models",
        "test",
        "test01.wav"
    )
);

var convertedAudioPath = Path.Combine(
    Path.GetDirectoryName(audioPath)!,
    "test01_16k_mono.wav"
);

if (!File.Exists(audioPath))
{
    Console.WriteLine($"ERROR: Audio file not found: {audioPath}");
    return;
}

// WAVを読み込み、モノラル・16 kHzに変換する。
using (var reader = new AudioFileReader(audioPath))
{
    if (reader.WaveFormat.Channels != 2)
    {
        throw new InvalidOperationException(
            "Input audio must be stereo."
        );
    }

    // ステレオをモノラルに変換する。
    var monoProvider = new StereoToMonoSampleProvider(reader)
    {
        LeftVolume = 0.5f,
        RightVolume = 0.5f
    };

    // サンプリングレートを16 kHzに変換する。
    var resampledProvider = new WdlResamplingSampleProvider(
        monoProvider,
        16000
    );

    // 16 bit PCM形式のWAVとして保存する。
    WaveFileWriter.CreateWaveFile16(
        convertedAudioPath,
        resampledProvider
    );
}

Console.WriteLine($"Converted Audio: {convertedAudioPath}");

// Whisper
Console.WriteLine("Initializing Whisper...");

// WhisperFactory生成時にネイティブランタイムをロードする。
using var factory = WhisperFactory.FromPath(modelPath);

// Build時にモデルを初期化する。
using var processor = factory.CreateBuilder()
    .WithLanguage("ja")
    .Build();

Console.WriteLine($"Loaded Runtime: {RuntimeOptions.LoadedLibrary}");
Console.WriteLine("Runtime Information:");
Console.WriteLine(WhisperFactory.GetRuntimeInfo());

Console.WriteLine("Initialization completed.");

// 変換済み音声ファイルを読み込む。
using var audioStream = File.OpenRead(convertedAudioPath);

Console.WriteLine("Starting transcription...");

// 音声認識を実行し、認識されたセグメントを順番に表示する。
await foreach (var result in processor.ProcessAsync(audioStream))
{
    Console.WriteLine(
        $"{result.Start} -> {result.End}: {result.Text}"
    );
}

Console.WriteLine("Transcription completed.");
