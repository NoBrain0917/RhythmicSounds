# RhythmicSounds
유니티에서 리듬게임을 만들때 유용한 유틸리티를 제공하는 라이브러리?    
매번 코드짜기 귀찮아서 복붙하는 용도로 만듦.   
<br>
[오디오 관련 실험한거 개소리 적은거 있어요](https://github.com/NoBrain0917/RhythmicSounds/blob/main/note.md). 
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
<br>


### RhythmicFMOD
- `FMOD`를 이용해서 리듬게임을 만들때 유용한 유틸을 제공.  
- 키음(타격음 포함)이 있거나 레이턴시가 중요한 리듬게임을 만들 목적이라면 `RhythmicFMOD`가 적합.
- 리겜만들때 FMOD 쌩으로 사용하면 귀찮은 부분이 많음. `RhythmicFMOD`를 사용하는 것을 강력 추천!@!@#!@#!@!@@!!@ (대충 따봉 200개 이모티콘)


