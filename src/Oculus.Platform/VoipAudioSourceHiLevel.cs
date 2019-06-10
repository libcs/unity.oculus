namespace Oculus.Platform
{
    using System;
    using UnityEngine;

    public class VoipAudioSourceHiLevel : MonoBehaviour
    {
        // This is a delegate that exists as a surface for OnAudioFilterRead
        // It will be callled on unity's audio thread
        public class FilterReadDelegate : MonoBehaviour
        {
            public VoipAudioSourceHiLevel parent;
            float[] scratchBuffer;

            void Awake()
            {
                var bufferSizeElements = (int)CAPI.ovr_Voip_GetOutputBufferMaxSize();
                scratchBuffer = new float[bufferSizeElements];
            }

            void OnAudioFilterRead(float[] data, int channels)
            {
                var sizeToFetch = data.Length / channels;
                var sourceBufferSize = sizeToFetch;
                if (sourceBufferSize > scratchBuffer.Length)
                {
                    Array.Clear(data, 0, data.Length);
                    throw new Exception(string.Format("Audio system tried to pull {0} bytes, max voip internal ring buffer size {1}", sizeToFetch, scratchBuffer.Length));
                }

                var available = parent.pcmSource.PeekSizeElements();
                if (available < sourceBufferSize)
                {
                    if (verboseLogging)
                    {
                        Debug.LogFormat(
                          "Voip starved! Want {0}, but only have {1} available",
                          sourceBufferSize,
                          available);
                    }
                    return;
                }

                var copied = parent.pcmSource.GetPCM(scratchBuffer, sourceBufferSize);
                if (copied < sourceBufferSize)
                {
                    Debug.LogWarningFormat(
                      "GetPCM() returned {0} samples, expected {1}",
                      copied,
                      sourceBufferSize);
                    return;
                }

                var dest = 0;
                var tmpPeakAmp = -1F;
                for (var i = 0; i < sizeToFetch; i++)
                {
                    var val = scratchBuffer[i];
                    for (var j = 0; j < channels; j++)
                    {
                        data[dest++] = val;
                        if (val > tmpPeakAmp)
                            tmpPeakAmp = val;
                    }
                }
                parent.peakAmplitude = tmpPeakAmp;
            }
        }

        int initialPlaybackDelayMS;
        public ulong senderID
        {
            set => pcmSource.SetSenderID(value);
        }

        public AudioSource audioSource;
        public float peakAmplitude;

        protected IVoipPCMSource pcmSource;

        static int audioSystemPlaybackFrequency;
        static bool verboseLogging = false;

        protected void Stop() { }

        VoipSampleRate SampleRateToEnum(int rate)
        {
            switch (rate)
            {
                case 48000: return VoipSampleRate.HZ48000;
                case 44100: return VoipSampleRate.HZ44100;
                case 24000: return VoipSampleRate.HZ24000;
                default: return VoipSampleRate.Unknown;
            }
        }

        protected void Awake()
        {
            CreatePCMSource();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.gameObject.AddComponent<FilterReadDelegate>();
            var filterDelegate = audioSource.gameObject.GetComponent<FilterReadDelegate>();
            filterDelegate.parent = this;
            initialPlaybackDelayMS = 40;
            audioSystemPlaybackFrequency = AudioSettings.outputSampleRate;
            CAPI.ovr_Voip_SetOutputSampleRate(SampleRateToEnum(audioSystemPlaybackFrequency));
            if (verboseLogging)
                Debug.LogFormat("freq {0}", audioSystemPlaybackFrequency);
        }

        void Start() => audioSource.Stop();

        protected virtual void CreatePCMSource() => pcmSource = new VoipPCMSourceNative();

        protected static int MSToElements(int ms) => ms * audioSystemPlaybackFrequency / 1000;

        void Update()
        {
            pcmSource.Update();
            if (!audioSource.isPlaying && pcmSource.PeekSizeElements() >= MSToElements(initialPlaybackDelayMS))
            {
                if (verboseLogging)
                    Debug.LogFormat("buffered {0} elements, starting playback", pcmSource.PeekSizeElements());
                audioSource.Play();
            }
        }
    }
}
