using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using FMOD;
using FMODUnity;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using Debug = UnityEngine.Debug;

namespace RhythmicSounds
{



    /// <summary>
    /// FMODs are optimized for use in rhythm games.
    /// If you're not playing a rhythm game, you don't need to use this class.
    /// </summary>
    public class RhythmicFMOD : MonoBehaviour
    {
        public class DeviceInfo
        {
            public string Name;
            public int Index;
            public bool IsAsio;
        }

        /// <summary>
        /// Sound volume for general music
        /// </summary>
        public static float MusicVolume = 1;
        
        /// <summary>
        /// Sound volume for other(sfx etc..) music
        /// </summary>
        public static float OtherVolume = 1;
        
        /// <summary>
        /// When activated, the song ends, but the song's time doesn't stop, it continues endlessly.
        /// </summary>
        public bool AudioEndless;

        /// <summary>
        /// Whether the clip is playing.
        /// Returns false if the clip ends even if AudioEndless is on.
        /// </summary>
        public bool IsClipPlaying { get; private set; }

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
        
        /// <summary>
        /// Sound volume for all music
        /// </summary>
        public static float MasterVolume
        {
            get => _masterVolume;
            set
            {
                _masterVolume = value;
                MainChannelGroup.setVolume(MasterVolume);
                SfxChannelGroup.setVolume(MasterVolume);
            }
        }
        
        
        /// <summary>
        /// A list of available drivers.
        /// </summary>
        public static DeviceInfo[] AvaliableDevices { get; private set; }
        
        
        private static int FmodSample
        {
            get
            {
                if (_fmodSample == 0)
                {
                    RuntimeManager.CoreSystem.getSoftwareFormat(out _fmodSample, out var _, out var __);
                    _reciprocalSampleRate = 1.0 / _fmodSample;
                }

                return _fmodSample;
            }
        }
        
        
        private static ChannelGroup SfxChannelGroup
        {
            get
            {
                if (_sfxChannelGroup.Equals(default(ChannelGroup)))
                    RuntimeManager.CoreSystem.createChannelGroup("RhythmicFMOD_SFXs", out _sfxChannelGroup);
                
                return _sfxChannelGroup;
            }
        }
        
        private static ChannelGroup MainChannelGroup
        {
            get
            {
                if (_mainChannelGroup.Equals(default(ChannelGroup)))
                    RuntimeManager.CoreSystem.createChannelGroup("RhythmicFMOD_Main", out _mainChannelGroup);
                
                return _mainChannelGroup;
            }
        }



        // Enter Play Mode 버그 고치기
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Init()
        {
            _fmodSample = 0;
            
            if (!_mainChannelGroup.Equals(default(ChannelGroup)))
                _mainChannelGroup.release();
            
            if (!_sfxChannelGroup.Equals(default(ChannelGroup)))
                _sfxChannelGroup.release();

            foreach (var s in _loadedSounds.Values)
            {
                s.release();
                s.clearHandle();
            }
            _loadedSounds.Clear();


            var list = new List<DeviceInfo>();

            // fuck fmod fuckfuckfuckfuckfuckfuckfuckfuckfuckfuckfuckfuckfuckfuckfuckfuckfuckfuckfuckfuckfuckfuckfuckfuckfuckfuckfuckfuckfuckfuckfuckfuckfuckfuckfuck
            FMOD.Studio.System.create(out var fmodSimulate);
            fmodSimulate.getCoreSystem(out var system);
            system.setOutput(OUTPUTTYPE.ASIO);
            
            system.getNumDrivers(out var nums);
            for (var n = 0; n < nums; n++)
            {
                system.getDriverInfo(n, out var name, 64, out var guid, out var rate,
                    out var speakermode, out var speakermodechannels);

                var d = new DeviceInfo
                {
                    Index = n,
                    Name = name,
                    IsAsio = true
                };
                list.Add(d);
            }

            system.release();
            system.clearHandle();

            fmodSimulate.release();
            fmodSimulate.clearHandle();


            RuntimeManager.CoreSystem.setOutput(OUTPUTTYPE.WASAPI);
            RuntimeManager.CoreSystem.getNumDrivers(out nums);
            for (var n = 0; n < nums; n++)
            {
                RuntimeManager.CoreSystem.getDriverInfo(n, out var name, 64, out var guid, out var rate,
                    out var speakermode, out var speakermodechannels);

                var d = new DeviceInfo
                {
                    Index = n,
                    Name = name,
                    IsAsio = false
                };
                list.Add(d);
            }
            

            AvaliableDevices = list.ToArray();
            
            ReloadFMOD();
            GC.Collect();
        }
        
        
        


        private static ChannelGroup _mainChannelGroup;
        private static ChannelGroup _sfxChannelGroup;
        private static Dictionary<string, Sound> _loadedSounds = new();
        
        private static int _fmodSample;
        private static double _reciprocalSampleRate;
        private static float _masterVolume = 1;

        private double _audioStartTimeSamples;
        private double _audioLength;
        private ulong _lastDspSamples;
        private double _interpolatedDspTime;
        private double _lastTime;
        private double _lastTimeBySamples;
        private bool _actionInvoked;
        private Channel _currentPlayingAudio;
        

        /// <summary>
        /// Reload the current FMOD
        /// </summary>
        public static void ReloadFMOD()
        {
            var fmodInstance =
                typeof(RuntimeManager).GetField("instance", (BindingFlags)15420)?.GetValue(null) as RuntimeManager;

            if (fmodInstance != null)
                DestroyImmediate(fmodInstance.gameObject);
        }



        /// <summary>
        /// Play the music. It plays after a delay equal to the maximum latency that can occur.
        /// </summary>
        /// <param name="sound">Sound</param>
        /// <param name="audioOffset">The amount of additional time to wait for the audio.</param>
        public void PlayMusic(Sound sound, float audioOffset = 0)
        {
            var delay = 2f;

            // Mobile is estimated to have a maximum delay of 400ms
            if (Application.platform == RuntimePlatform.Android)
                delay += 1f; // Delay by a generous 1000ms or so
            
            // If it does lag, increase the delay by 5000ms or so.
            if (1f / Time.unscaledDeltaTime < 30)
                delay += 5f;
            

            PlayMusic(sound, delay, audioOffset);
        }
        
        
        


        /// <summary>
        /// Play the music. Wait the amount of time you specify.
        /// </summary>
        /// <param name="sound">Sound</param>
        /// <param name="delay">The amount of time to wait. The unit is seconds. Lower delays can cause latency in the audio.</param>
        /// <param name="audioOffset">The amount of additional time to wait for the audio.</param>
        public void PlayMusic(Sound sound, float delay, float audioOffset)
        {
            if (IsPlaying)
                Stop();
            
            else
            {
                _currentPlayingAudio.getPosition(out var time, TIMEUNIT.MS);
                if(time>0)
                    Stop();
            }
            
            // audioOffset이 양수 -> 음악을 더 늦게 재생
            // audioOffset이 음수 -> 음악을 더 빨리 재생
            MainChannelGroup.getDSPClock(out var dspclock, out var __);
            
            if (audioOffset < 0)
                delay += Math.Abs(audioOffset);

            enabled = true;
            WaitingDelay = delay;
            IsPlaying = true;
            IsClipPlaying = true;
            _audioStartTimeSamples = (dspclock / (double)FmodSample);

            sound.getLength(out var v, TIMEUNIT.MS);
            _audioLength = (v * 0.001) + WaitingDelay;
            
            
            
            if (delay+audioOffset <= 0)
            {
                RuntimeManager.CoreSystem.playSound(sound, MainChannelGroup, false, out _currentPlayingAudio);
                _currentPlayingAudio.setVolume(MusicVolume);
                _currentPlayingAudio.setPriority(0);
            }
            else
            {
                PlayScheduled(sound, dspclock + ConvertTimeToSamples(delay+audioOffset), out _currentPlayingAudio);
                _currentPlayingAudio.setVolume(MusicVolume);
                _currentPlayingAudio.setPriority(0);
            }
        }
        
        
        /// <summary>
        /// Pause the audio. Using Pause on an AudioSource can be buggy and is not recommended.
        /// </summary>
        public void Pause()
        {
            if (!IsPlaying) return;

            _currentPlayingAudio.setPaused(true);
            MainChannelGroup.setPaused(true);
            
            IsClipPlaying = false;
            IsPlaying = false;
        }
        
        
        /// <summary>
        /// UnPause the audio. Using UnPause in AudioSource can be buggy and is not recommended.
        /// </summary>
        public void UnPause()
        {
            if (IsPlaying) return;
            
            
            _currentPlayingAudio.getPosition(out var time, TIMEUNIT.MS);
            if (time == 0) return;
            
            _lastTime = Time.unscaledTimeAsDouble;

            _currentPlayingAudio.setPaused(false);
            MainChannelGroup.setPaused(false);
            
            IsClipPlaying = true;
            IsPlaying = true;
        }


        /// <summary>
        /// Stops the audio.
        /// </summary>
        public void Stop()
        {
            enabled = false;
            IsPlaying = false;
            IsClipPlaying = false;
            _actionInvoked = false;

            if (!_currentPlayingAudio.Equals(default(Channel)))
            {
                _currentPlayingAudio.setPaused(false);
                MainChannelGroup.setPaused(false);

                _currentPlayingAudio.isPlaying(out var v);
                if (v)
                    _currentPlayingAudio.stop();
                
                _currentPlayingAudio.clearHandle();
            }
            //_originalAudioSource.Stop();
            //_originalAudioSource.enabled = false;
            //_actionInvoked = false;
        }


        /*
         * FMOD 실험 결과
         * 렉걸리더라도 dsptime이랑 노래 시간이랑 오차 X
         * 그리고 FMOD개발자왈) getPosition보다 getDSPClock이 더 정확하다.
         */
        
        private void InterpolateDspTime()
        {
            MainChannelGroup.getDSPClock(out var dspclock, out var __);
            if (_lastDspSamples == dspclock)
            {
                _interpolatedDspTime = _lastTimeBySamples + (Time.unscaledTimeAsDouble - _lastTime);
            }
            else
            {
                _lastDspSamples = dspclock;
                _interpolatedDspTime = dspclock * _reciprocalSampleRate;
                _lastTimeBySamples = _interpolatedDspTime;
                _lastTime = Time.unscaledTimeAsDouble;
            } 
        }


        private void Update()
        {
            if (!IsPlaying) return;
            
            // fmod는 항상 dsp타임 기준으로
            InterpolateDspTime();
            CurrentTime = _interpolatedDspTime - _audioStartTimeSamples;

            // 클립이 끝났는지 체크
            if (CurrentTime >= _audioLength && IsClipPlaying)
            {
                IsClipPlaying = false;

                if (FinishedCalloffset == 0)
                {
                    if(!_actionInvoked)
                        OnAudioFinished?.Invoke();
                    _actionInvoked = true;
                    if (!AudioEndless)
                    {
                        Stop();
                        return;
                    }
                }

            }

            
            if (!IsClipPlaying)
            {
                // AudioEndless가 True -> False로 바뀔때 대응
                if (!AudioEndless && _actionInvoked)
                {
                    Stop();
                    return;
                }
                
                
                // 일정 시간이 지난 후에 이벤트 리스너 실행
                if (FinishedCalloffset > 0 && CurrentTime - _audioLength >= FinishedCalloffset)
                {
                    if(!_actionInvoked)
                        OnAudioFinished?.Invoke();
                    _actionInvoked = true;
                    if (!AudioEndless)
                        Stop();
                }
            }
        }


        private void OnDestroy()
        {
            EnableUnityAudio();
            
            if (!_currentPlayingAudio.Equals(default(Channel)))
            {
                _currentPlayingAudio.isPlaying(out var v);
                if (v)
                    _currentPlayingAudio.stop();
                
                _currentPlayingAudio.clearHandle();
            }
        }
        

        private void Awake()
        {
            OnAudioFinished = new UnityEvent();
            DisableUnityAudio();
        }



        /// <summary>
        /// Set the device to output to
        /// </summary>
        /// <param name="deviceInfo">DeviceInfo Class</param>
        public static void SetOutputDevice(DeviceInfo deviceInfo)
        {
            if (deviceInfo == null) return;
            RuntimeManager.CoreSystem.setOutput(deviceInfo.IsAsio ? OUTPUTTYPE.ASIO : OUTPUTTYPE.WASAPI);
            RuntimeManager.CoreSystem.setDriver(deviceInfo.Index);
        }

        
        /// <summary>
        /// Convert the time to Samples.
        /// </summary>
        /// <param name="seconds">Seconds</param>
        /// <returns>Samples with Time Conversion</returns>
        public static ulong ConvertTimeToSamples(float seconds)
        {
            return (ulong)(seconds * FmodSample);
        }


        /// <summary>
        /// Play the sound once, right away.
        /// This is useful for playing sounds like SFX.
        /// </summary>
        /// <param name="sound">Sound</param>
        /// <param name="pitch">pitch</param>
        public static void PlayOneShot(Sound sound, float pitch = 1)
        {
            RuntimeManager.CoreSystem.playSound(sound, SfxChannelGroup, false, out var channel);
            channel.setVolume(OtherVolume);
            channel.setPitch(pitch);
        }
        
        
        /// <summary>
        /// It acts like Unity's AudioSource.PlayScheduled.
        /// </summary>
        /// <param name="sound">Sound</param>
        /// <param name="timeSamples">The location of the samples to play the sound</param>
        /// <param name="channelGroup">Channel Group</param>
        /// <param name="channel">Output Channel</param>
        public static void PlayScheduled(Sound sound, ulong timeSamples, ChannelGroup channelGroup, out Channel channel)
        {
            RuntimeManager.CoreSystem.playSound(sound, channelGroup, true, out channel);
            channel.setDelay(timeSamples, 0, false);
            channel.setPaused(false);
        }
        
        
        /// <summary>
        /// It acts like Unity's AudioSource.PlayScheduled.
        /// </summary>
        /// <param name="sound">Sound</param>
        /// <param name="timeSamples">The location of the samples to play the sound</param>
        /// <param name="channel">Output Channel</param>
        public static void PlayScheduled(Sound sound, ulong timeSamples, out Channel channel)
        {
            RuntimeManager.CoreSystem.playSound(sound, MainChannelGroup, true, out channel);
            channel.setDelay(timeSamples, 0, false);
            channel.setPaused(false);
        }
        

        /// <summary>
        /// Set the buffer of the current FMOD.
        /// It is not recommended to call this too many times.
        /// </summary>
        /// <param name="buffersize"></param>
        public static void SetAudioBufferSize(int buffersize)
        {
            try
            {
                var asm = Assembly.GetAssembly(typeof(RuntimeManager));

                var propertyAccessors = asm.GetType("FMODUnity.Platform")
                    .GetNestedType("PropertyAccessors", (BindingFlags)15420);
                var dspBufferLength = propertyAccessors.GetField("DSPBufferLength", (BindingFlags)15420);
                var dspBufferCount = propertyAccessors.GetField("DSPBufferCount", (BindingFlags)15420);
                var setterMethod = dspBufferLength?.FieldType.GetMethod("Set");

                var fmodSettings = Settings.Instance;
                var currentPlatform = (Platform)typeof(Settings).GetMethod("FindCurrentPlatform", (BindingFlags)15420)
                    ?.Invoke(fmodSettings, new object[] { });

                setterMethod?.Invoke(dspBufferLength.GetValue(null), new object[] { currentPlatform, buffersize });
                setterMethod?.Invoke(dspBufferCount?.GetValue(null), new object[] { currentPlatform, 2 });

                ReloadFMOD();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to change buffer. wtf?: {e.Message}");
            }
        }


        /// <summary>
        /// Reads the bytes of an audio file and creates it as a Sound.
        /// </summary>
        /// <param name="bytes">bytes of an audio</param>
        /// <param name="codec">The extension name of the codec used. For example) mpeg -> mp3</param>
        /// <returns>Generated Sound</returns>
        public static Sound CreateSoundFromBytes(byte[] bytes, string codec)
        {
            var path = Path.Combine(Path.GetTempPath(), "fmodbytesload") + "." + codec.ToLower();
            File.WriteAllBytes(path, bytes);
            RuntimeManager.CoreSystem.createSound(path, MODE.CREATESAMPLE, out var s);
            File.Delete(path);

            return s;
        }


        /// <summary>
        /// Create an AudioClip as a Sound.
        /// </summary>
        /// <param name="clip">AudioClip</param>
        /// <returns>Converted Sound</returns>
        public static Sound CreateSoundFromAudioClip(AudioClip clip)
        {

            var samples = new float[clip.samples * clip.channels];
            clip.GetData(samples, 0);

            var lenbytes = (uint)(clip.samples * clip.channels * sizeof(float));

            var soundinfo = new CREATESOUNDEXINFO();
            soundinfo.length = lenbytes;
            soundinfo.format = SOUND_FORMAT.PCMFLOAT;
            soundinfo.defaultfrequency = clip.frequency;
            soundinfo.numchannels = clip.channels;
            soundinfo.cbsize = Marshal.SizeOf(typeof(CREATESOUNDEXINFO));

            RuntimeManager.CoreSystem.createSound(soundinfo.fileuserread_internal,
                MODE.OPENUSER | MODE._2D | MODE.CREATESAMPLE | MODE.LOOP_OFF, ref soundinfo, out var sound);

            sound.@lock(0, lenbytes, out var ptr1, out var ptr2, out var len1, out var len2);
            Marshal.Copy(samples, 0, ptr1, (int)(len1 / sizeof(float)));
            if (len2 > 0)
                Marshal.Copy(samples, (int)(len1 / sizeof(float)), ptr2, (int)(len2 / sizeof(float)));

            sound.unlock(ptr1, ptr2, len1, len2);

            return sound;
        }
        
        
        
        /// <summary>
        /// Disable Unity Audio.
        /// </summary>
        public static void DisableUnityAudio()
        {
            if (AudioListener.volume > 0)
            {
                PlayerSettings.muteOtherAudioSources = true;
                AudioListener.volume = 0f;
                AudioListener.pause = true;

                var audioListener = FindAnyObjectByType<AudioListener>();
                if (audioListener != null)
                    audioListener.enabled = false;
            }
        }
        
        
        /// <summary>
        /// Enable Unity Audio.
        /// </summary>
        public static void EnableUnityAudio()
        {
            if (AudioListener.volume == 0 && !AudioListener.pause)
            {
                PlayerSettings.muteOtherAudioSources = false;
                AudioListener.volume = 1f;
                AudioListener.pause = false;

                var audioListener = FindAnyObjectByType<AudioListener>();
                if (audioListener != null)
                    audioListener.enabled = true;
            }
        }
        
    }

}
