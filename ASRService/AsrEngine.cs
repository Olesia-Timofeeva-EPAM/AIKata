using System;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;

namespace ASRService
{
    public class AsrService
    {
        private readonly string _subscriptionKey;
        private readonly string _region;

        public AsrService(string subscriptionKey, string region = "eastus")
        {
            _subscriptionKey = subscriptionKey;
            _region = region;
        }

        /// <summary>
        /// Transcribe a single audio file (wav, mp3, etc.) asynchronously.
        /// </summary>
        public async Task<string> TranscribeFileAsync(string filePath)
        {
            var config = SpeechConfig.FromSubscription(_subscriptionKey, _region);
            config.SpeechRecognitionLanguage = "en-US";

            using var audioInput = AudioConfig.FromWavFileInput(filePath);
            using var recognizer = new SpeechRecognizer(config, audioInput);

            Console.WriteLine($"[ASR] Starting recognition for file: {filePath}");
            var result = await recognizer.RecognizeOnceAsync();

            if (result.Reason == ResultReason.RecognizedSpeech)
            {
                Console.WriteLine($"[ASR] Recognized: {result.Text}");
                return result.Text;
            }

            if (result.Reason == ResultReason.NoMatch)
            {
                Console.WriteLine("[ASR] No speech could be recognized.");
                return string.Empty;
            }

            if (result.Reason == ResultReason.Canceled)
            {
                var cancellation = CancellationDetails.FromResult(result);
                Console.WriteLine($"[ASR] Canceled: {cancellation.Reason}");
                Console.WriteLine($"[ASR] Error: {cancellation.ErrorDetails}");
                throw new Exception(cancellation.ErrorDetails);
            }

            return string.Empty;
        }

        /// <summary>
        /// Start live transcription from microphone until the user stops.
        /// </summary>
        public async Task TranscribeFromMicAsync()
        {
            var config = SpeechConfig.FromSubscription(_subscriptionKey, _region);
            config.SpeechRecognitionLanguage = "en-US";

            using var audioInput = AudioConfig.FromDefaultMicrophoneInput();
            using var recognizer = new SpeechRecognizer(config, audioInput);

            Console.WriteLine("Speak into your microphone. Press Enter to stop.");

            recognizer.Recognizing += (s, e) =>
                Console.WriteLine($"[Partial] {e.Result.Text}");

            recognizer.Recognized += (s, e) =>
                Console.WriteLine($"[Final] {e.Result.Text}");

            recognizer.Canceled += (s, e) =>
                Console.WriteLine($"[Canceled] {e.Reason}: {e.ErrorDetails}");

            recognizer.SessionStopped += (s, e) =>
                Console.WriteLine("[Session Stopped]");

            await recognizer.StartContinuousRecognitionAsync();
            Console.ReadLine();
            await recognizer.StopContinuousRecognitionAsync();
        }
    }
}
