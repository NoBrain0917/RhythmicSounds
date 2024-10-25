using System;
using UnityEngine;
using UnityEngine.Events;


namespace RhythmicSounds
{

    /// <summary>
    /// Unity's AudioSource is optimized for use in rhythm games.
    /// If you're not playing a rhythm game, you don't need to use this class.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class RhythmicAudioSource : MonoBehaviour
    {
        /// <summary>
        /// The AudioSource used by this class.
        /// </summary>
        public AudioSource OriginalAudioSource => _originalAudioSource;

        private AudioSource _originalAudioSource;
        
        /// <summary>
        /// Sound volume for general music
        /// </summary>
        public static float MusicVolume = 1;
        
        
        /// <summary>
        /// Sound volume for other(sfx etc..) music
        /// </summary>
        public static float OtherVolume = 1;
        
        
        /// <summary>
        /// Sound volume for all music
        /// </summary>
        public static float MasterVolume
        {
            get => AudioListener.volume;
            set => AudioListener.volume = value;
        }
        

        /// <summary>
        /// When activated, the song ends, but the song's time doesn't stop, it continues endlessly.
        /// </summary>
        public bool AudioEndless;

        /// <summary>
        /// Whether the clip is playing.
        /// Returns false if the clip ends even if AudioEndless is on.
        /// </summary>
        public bool IsClipPlaying => _originalAudioSource.isPlaying;

        /// <summary>
        /// Whether this audio is currently being used.
        /// If AudioEndless is off, this is like a normal AudioSource.isPlaying.
        /// </summary>
        public bool IsPlaying { get; private set; }

        /// <summary>
        /// The number of seconds to wait for audio.
        /// </summary>
        public float WaitingDelay { get; private set; }

        /// <summary>
        /// Whether the specified delay is over.
        /// </summary>
        /// <returns></returns>
        public bool IsWaitingDelayFinished => (AudioSettings.dspTime - _audioStartDspTime) >= WaitingDelay;

        /// <summary>
        /// Gets the current time.
        /// Includes the time that is now being delayed.
        /// </summary>
        public double CurrentTime { get; private set; }
        
        /// <summary>
        /// Gets the time position of the actual song being played.
        /// </summary>
        public double CurrentActualTime => Math.Max(0,CurrentTime - WaitingDelay);
        
        /// <summary>
        /// An action that runs when the audio is finished playing.
        /// It is not affected by the Stop method.
        /// </summary>
        public UnityEvent OnAudioFinished;

        /// <summary>
        /// Determines after how many seconds to fire OnAudioFinished.
        /// </summary>
        public float FinishedCalloffset;

        private double _lastDspTime;
        private double _lastTime;
        private int _lastSamples;
        private double _lastTimeBySamples;
        private double _interpolatedTimeBySamples;
        private double _interpolatedDspTime;
        
        private bool _actionInvoked;
        private double _audioStartDspTime;
        private double _reciprocalSampleRate;
        private double _offset = -1;



        /// <summary>
        /// Set the buffer size for the audio.
        /// </summary>
        /// <param name="bufferSize">The buffer size. A power of 2 is recommended.</param>
        public static void SetAudioBufferSize(int bufferSize)
        {
            var c = AudioSettings.GetConfiguration();
            c.dspBufferSize = bufferSize;

            AudioSettings.Reset(c);
        }


        
        /// <summary>
        /// Play the sound once, right away.
        /// This is useful for playing sounds like SFX.
        /// </summary>
        /// <param name="clip">Audio clip</param>
        /// <param name="pitch">pitch</param>
        public void PlayOneShot(AudioClip clip, float pitch = 1)
        {
            _originalAudioSource.enabled = true;
            _originalAudioSource.pitch = pitch;
            _originalAudioSource.PlayOneShot(clip, OtherVolume);
        }
        


        /// <summary>
        /// Play the music. It plays after a delay equal to the maximum latency that can occur.
        /// </summary>
        /// <param name="clip">Audio clip</param>
        /// <param name="audioOffset">The amount of additional time to wait for the audio.</param>
        public void PlayMusic(AudioClip clip, float audioOffset = 0)
        {
            var delay = 2f;

            // Mobile is estimated to have a maximum delay of 400ms
            if (Application.platform == RuntimePlatform.Android)
                delay += 1f; // Delay by a generous 1000ms or so

            // If it does lag, increase the delay by 5000ms or so.
            if (1f / Time.unscaledDeltaTime < 30)
                delay += 5f;

            PlayMusic(clip, delay, audioOffset);
        }
        



        /// <summary>
        /// Play the music. Wait the amount of time you specify.
        /// </summary>
        /// <param name="clip">Audio clip</param>
        /// <param name="delay">The amount of time to wait. The unit is seconds. Lower delays can cause latency in the audio.</param>
        /// <param name="audioOffset">The amount of additional time to wait for the audio.</param>
        public void PlayMusic(AudioClip clip, float delay, float audioOffset)
        {
            if (IsPlaying || _originalAudioSource.time > 0)
                Stop();
            
            // audioOffset이 양수 -> 음악을 더 늦게 재생
            // audioOffset이 음수 -> 음악을 더 빨리 재생

            if (audioOffset < 0)
                delay += Math.Abs(audioOffset);

            IsPlaying = true;
            WaitingDelay = delay;

            _reciprocalSampleRate = 1.0 / clip.frequency;
            _audioStartDspTime = AudioSettings.dspTime;
            _originalAudioSource.enabled = true;
            enabled = true;
            _originalAudioSource.pitch = 1;
            _originalAudioSource.clip = clip;
            _originalAudioSource.volume = MusicVolume;
            _originalAudioSource.PlayScheduled(_audioStartDspTime + delay + audioOffset);

        }


        /// <summary>
        /// Pause the audio. Using Pause on an AudioSource can be buggy and is not recommended.
        /// </summary>
        public void Pause()
        {
            if (!IsPlaying) return;

            _originalAudioSource.Pause();
            IsPlaying = false;
        }


        /// <summary>
        /// UnPause the audio. Using UnPause in AudioSource can be buggy and is not recommended.
        /// </summary>
        public void UnPause()
        {
            if (IsPlaying) return;
            if (_originalAudioSource.time == 0) return;

            _lastDspTime = AudioSettings.dspTime;
            _interpolatedDspTime = _lastDspTime;

            _lastSamples = _originalAudioSource.timeSamples;
            _interpolatedTimeBySamples = _lastSamples * _reciprocalSampleRate;
            _lastTime = Time.unscaledTimeAsDouble;

            _originalAudioSource.UnPause();
            IsPlaying = true;
        }


        /// <summary>
        /// Stops the audio. Using Stop on an AudioSource can be buggy and is not recommended.
        /// </summary>
        public void Stop()
        {
            IsPlaying = false;
            _originalAudioSource.Stop();
            _originalAudioSource.enabled = false;
            _actionInvoked = false;
            _offset = -1;
            enabled = false;
        }


        private void InterpolateDspTime()
        {
            if (_lastDspTime == AudioSettings.dspTime)
            {
                _interpolatedDspTime = _lastDspTime + (Time.unscaledTimeAsDouble-_lastTime);
            }
            else
            {
                _lastDspTime = AudioSettings.dspTime;
                _interpolatedDspTime = _lastDspTime;
                _lastTime = Time.unscaledTimeAsDouble;
            }
        }

        
      
        /*
         * 굳이 dspTime 계속 안쓰고 samples 쓰는이유
         * 노래 재생할때 렉걸리면 dspTime이랑 노래 시간이랑 오차 생김
         * 이유는 모름
         */
        
        private void InterpolateAudioTime()
        {
            if (_lastSamples == _originalAudioSource.timeSamples)
            {
                _interpolatedTimeBySamples = _lastTimeBySamples + (Time.unscaledTimeAsDouble-_lastTime);
            }
            else
            {
                _lastSamples = _originalAudioSource.timeSamples;
                _lastTime = Time.unscaledTimeAsDouble;
                _interpolatedTimeBySamples = _lastSamples * _reciprocalSampleRate;
                _lastTimeBySamples = _interpolatedTimeBySamples;

            }
        }


        private void Update()
        {
            if (!IsPlaying) return;

            if (IsWaitingDelayFinished)
            {
                // 오디오가 끝났는가
                if (!_originalAudioSource.isPlaying)
                {
                    if (AudioEndless || FinishedCalloffset > 0)
                    {
                        // AudioEndless가 True -> False로 바뀔때 대응
                        if (!AudioEndless && _actionInvoked)
                        {
                            Stop();
                            return;
                        }

                        // 오디오가 없으니 다시 Dsp기반으로
                        InterpolateDspTime();
                        if (_offset == -1)
                            _offset = (_interpolatedDspTime - _audioStartDspTime) - WaitingDelay -
                                      _originalAudioSource.clip.length;
                        CurrentTime = _interpolatedDspTime - _audioStartDspTime - _offset;

                        // 특정 시간 지나면 OnAudioFinished 실행
                        if (!_actionInvoked && CurrentTime - _originalAudioSource.clip.length - WaitingDelay >=
                            FinishedCalloffset)
                        {
                            _actionInvoked = true;
                            OnAudioFinished?.Invoke();
                            

                            if (!AudioEndless)
                                Stop();
                        }
                    }
                    else
                    {
                        // 노래 끝나면 바로 OnAudioFinished 실행

                        if (!_actionInvoked)
                            OnAudioFinished?.Invoke();
                        Stop();
                    }
                }
                else
                {
                    // 오디오가 안끝난 상태, 샘플 기반 시간 설정
                    InterpolateAudioTime();
                    CurrentTime = _interpolatedTimeBySamples + WaitingDelay;
                }
            }
            else
            {
                // 딜레이 중일때, dsp 기반 시간 설정
                InterpolateDspTime();
                CurrentTime = _interpolatedDspTime - _audioStartDspTime;
            }
            
            


        }


        private void Awake()
        {
            TryGetComponent(out _originalAudioSource);


            OnAudioFinished = new UnityEvent();

            _originalAudioSource.playOnAwake = false;
            _originalAudioSource.loop = false;
            _originalAudioSource.enabled = false;
            _originalAudioSource.priority = 0;
            _originalAudioSource.spatialBlend = 0;
            _originalAudioSource.pitch = 1;

        }


    }

}
