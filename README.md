# RhythmicSounds
유니티에서 리듬게임을 만들때 유용한 유틸리티를 제공하는 라이브러리?    
매번 코드짜기 귀찮아서 복붙하는 용도로 만듦.   
<br>
<br>

## Difference
 - 레이턴시 최소화, 갑자기 10초동안 겜이 멈춘다던가 진짜 개똥컴 아니라면 99%는 레이턴시로 일어나는 싱크 밀림은 없음
 - '실제 출력되는' 오디오 시간과 스크립트 내 시간 오차를 줄임.
 - (FMOD기반 한정) 다양한 오디오 유틸리티 제공

----

## Usage
원하는 목적에 따라 둘중 하나 코드 복붙하고 플젝에 추가.    
유니티 인스펙터에 `RhythmicAudioSource` 또는 `RhythmicFMOD` 컴포넌트를 추가하면 끝.      
<br>
<br>

### RhythmicAudioSource
- `AudioSource`를 이용해서 리듬게임을 만들때 유용한 유틸을 제공.   
- 가볍게 만들거나 키음(타격음 포함)이 없는 리듬게임을 만들 목적이라면 `RhythmicAudioSource`가 적합.
#### 주요 유틸
```cs
float CurrentTime //현재 시간
UnityEvent OnAudioFinished //노래가 끝난후 이벤트 실행
float FinishedCalloffset //이벤트를 몇초 뒤 실행할지 결정

static void SetAudioBufferSize(int bufferSize) //버퍼사이즈 변경
void PlayMusic(AudioClip clip, float audioOffset = 0) //메인 음악 재생
void Stop() //음악 정지
void UnPause() //언퍼즈
void Pause() //퍼즈
//etc..
```


### RhythmicFMOD
- `FMOD`를 이용해서 리듬게임을 만들때 유용한 유틸을 제공.  
- 키음(타격음 포함)이 있거나 레이턴시가 중요한 리듬게임을 만들 목적이라면 `RhythmicFMOD`가 적합.
- 리겜만들때 FMOD 쌩으로 사용하면 귀찮은 부분이 많음. `RhythmicFMOD`를 사용하는 것을 강력 추천!@!@#!@#!@!@@!!@ (대충 따봉 200개 이모티콘)
#### 주요 유틸
```cs
float CurrentTime //현재 시간
UnityEvent OnAudioFinished //노래가 끝난후 이벤트 실행
float FinishedCalloffset //이벤트를 몇초 뒤 실행할지 결정
static DeviceInfo[] AvaliableDevices //사용 가능한 출력기기 리스트

static void SetOutputDevice(DeviceInfo deviceInfo) //출력 기기 설정
static void PlayScheduled() //소리 재생 예약
static void CreateSoundFromBytes(byte[] bytes, string codec) //bytes -> Sound
static void CreateSoundFromAudioClip(AudioClip clip) //AudioClip -> Sound
void ReloadFMOD() //FMOD 재시작
static void SetAudioBufferSize(int bufferSize) //버퍼사이즈 변경
void PlayMusic(Sound sound, float audioOffset = 0) //메인 음악 재생
void Stop() //음악 정지
void UnPause() //언퍼즈
void Pause() //퍼즈
//etc..
```

