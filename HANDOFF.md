# HANDOFF — ASTRA 진행 현황 / 인수인계

> **매 세션 시작 시 이 문서를 먼저 읽고** 진행 현황·다음 할 일을 파악한다.
> 설계 근거는 `./GDD.md`, 구현 규칙은 `./CLAUDE.md`. 본 문서는 **"지금까지 만든 것 + 다음에 할 것 + 함정"** 요약.
> 최종 갱신: 2026-07-01

---

## 0. 빠른 시작
- Unity **6000.3.7f1**, URP 2D. 메인 씬 `Assets/Scenes/SampleScene.unity`.
- **Play 모드**로 검증. Player Settings에 **Run In Background = ON**(영구) — 비포커스에서도 프레임 진행.
- MCP(UnityMCP)로 에디터 제어 가능. pixellab MCP로 픽셀아트 생성.
- 조작: 이동 방향키/WASD · 발사 `Z`/`Space` · 이탈 `X` · 봄 `C`/`B` · **시작 `Z`/`Enter` · 일시정지 `ESC`/`P` · 재시작 `R`**.
- 콘텐츠 저작: Unity 상단 메뉴 **`ASTRA/①무기 ②적기 ②-B편대 ③보스 ④레벨 에디터`**.
- **버전관리**: git 저장소, 원격 GitHub **https://github.com/Jotomate/ASTRA** (`main`). 작업 마무리마다 커밋·푸시(사용자 요청 시). `.gitignore`로 Library/Temp/Logs/Screenshots 제외, `.claude/`는 추적. 커밋 메시지 끝에 `Co-Authored-By: Claude ...`.

---

## 1. 구현 완료 (✅)

**플레이어 / 무기**
- 기체 이동(Transform+클램프), 무기-출력 Lv1~4(사망 시 리셋), 라이프, 부활 무적 점멸, 봄(화면 클리어).
- 무기 7종: `W01_Vulcan` `W90_WideShot` `W02_Laser`(관통) `W08_Reflect`(벽 반사) `W03_LockShot`(유도) `W10_Omni`(전방위 회전) `W06_LockOnLaser`(다중 락온 유도 레이저).
- 무기 픽업/즉시교체, **이탈(Eject)**=폭발+기본 복귀. 탄환 풀링(반사·유도 지원).

**적 / 편대 / 보스**
- `EnemyData` 이동 5종: Straight/Homing/PathTrajectory(웨이포인트)/Formation/FixedCannon. 발사·드롭·등장 페이드.
- `FormationData` 편대(Line/V/Column) + `FormationGroup` 전멸 보너스(wipeBonus).
- `BossData` 보스: 코어+파괴가능 파츠, 체력구간 **페이즈 FSM**(조준/n-way/원형 탄막), 기믹 **변신**(스프라이트/색+무적)·**분리**(사망 시 잔해 적)·**촉수**(FABRIK 다관절, 아래 §4 B2), HUD 체력바.

**시스템 / 연출**
- 중앙 충돌 `CollisionManager`(원-원, `IDamageable` 일반화: 자기탄→대상 / 적탄→기체 / 본체→기체 / AoE).
- 드롭(P/W), 점수, **콤보 배율 스코어링**(연속 처치 → ×1~8, 시간초과/피탄 시 리셋), **1UP(익스텐드)**, 게임오버/재시작, **멀티 스테이지 진행**.
- `StageDirector` 스폰 타임라인(웨이브→경고→보스→클리어), 배경 **다중 스크롤(별필드 3층)**, **지형 블록**(`TerrainBlock`, 파괴가능·탄차단·접촉피해, `SpawnKind.Terrain`).
- HUD: 점수/**콤보**/라이프/무기출력/무기명/봄/스테이지 · **타이틀**/**PAUSED**/GAME OVER/보스바/WARNING/STAGE CLEAR (legacy uGUI Text).
- pixellab 실제 스프라이트: 기체·적기·보스 본체. **기체 좌/우 뱅킹**(`PlayerBank`: 이동 입력에 따라 BankL/BankR/중립 스프라이트 전환, pixellab `create_object_state`로 생성).
- **5대 커스텀 에디터**(무기/적기/편대/보스/레벨) — 카드 목록·생성·삭제·편집.

**사운드 / 손맛 (juice)**
- `AudioManager`: **런타임 합성** SFX(발사/레이저/피격/폭발/보스폭발/픽업/파워업/봄/사망/경고) + 절차적 **BGM 루프**(외부 오디오 파일 없음). 8소스 라운드로빈 + 피치 변주.
- `CameraShake`(Main Camera): 보스 사망·봄·피탄 시 흔들림(unscaledDeltaTime).
- `EffectPool`: 격파 폭발 이펙트 풀(20개), 적/보스 사망 시 재생.
- **히트스톱**(`GameManager.HitStop`, 보스사망/봄/피탄 시 짧은 정지), 피격 플래시(Enemy 흰색), 발사·픽업·경고 SFX 등 전 이벤트 연결.

---

## 2. 아키텍처 / 진입점
- 네임스페이스 `ShootingGame.[기능]` (Player/Weapon/Bullet/Enemy/Boss/Stage/Item/Core/UI/Effect), 에디터 `ShootingGame.EditorTools`.
- 데이터 카드(SO): `WeaponData` `EnemyData` `FormationData` `BossData` `StageData` (모두 `ASTRA/...` CreateAssetMenu).
- 씬 매니저 GO: `CollisionManager` `GameManager` `BulletPool` `EnemyPool` `DropManager` `AudioManager` `EffectPool` `StageBackground` `StageDirector` `HUD Canvas`. `CameraShake`는 Main Camera에 부착. (`EnemySpawner`/`BossSpawner`는 비활성 — StageDirector가 구동)
- 코드 위치: `Assets/Scripts/{Player,Weapon,Bullet,Enemy,Boss,Stage,Item,Core,UI,Effect,Editor}/`.
- 콘텐츠 카드: `Assets/ScriptableObjects/{Weapons,Enemies,Formations,Bosses,Stages}/`.

---

## 3. ⚠️ 함정 / 컨벤션 (반드시 숙지 — 메모리에도 기록됨)
1. **UnityMCP `execute_code`는 CodeDom(C# 6)**: `using` 금지(메서드 본문), `System.*`/`UnityEngine.SceneManagement`는 정규화, `Object`는 `UnityEngine.Object`로. 프로젝트 타입은 `Type.GetType("ShootingGame.X.Y, Assembly-CSharp")`, 에디터 타입은 `, Assembly-CSharp-Editor`.
2. **네임스페이스=타입명 충돌**: `ShootingGame.Bullet.Bullet` `Player.Player` `Enemy.Enemy` `Boss.Boss` → 다른 ns에서 `using` 후 bare 이름 쓰면 CS0118. **정규화 또는 `using Alias=...`**.
3. **URP 2D 게임플레이 스프라이트는 Unlit 필수**(`Assets/Art/Materials/SpriteUnlit.mat`). Lit이면 Game 뷰에서 안 보임. 새 SpriteRenderer마다 적용.
4. **에셋 생성→씬 참조 와이어링 시 순서**: `CreateAsset` → `AssetDatabase.SaveAssets()` → `SerializedObject` 참조 연결 → `SaveScene`. (GUID 미확정 시 도메인 리로드 후 null)
5. **풀링 객체는 재사용 시 모든 상태 리셋**(탄 스프라이트가 안 바뀌던 버그 사례). `WeaponController.Equip`은 `spinAngle=0` 리셋.
6. **MCP 검증**: 비포커스 시 `Time.deltaTime≈0` → "한 호출 스폰, 다음 호출 확인"이 프레임 미진행으로 실패 가능. **private 메서드(LateUpdate 등) 직접 invoke**로 결정적 검증. 반복 재활성화로 ghost player 생기면 `GameManager.player`로 검증.
7. `AssetDatabase.DeleteAsset`/`File.*` 등은 `execute_code`의 `safety_checks=false` 필요.

---

## 4. 다음 구현 목표 (우선순위)

### A. 체감/완성도
- ✅ **A1 손맛(juice)** 완료: 피격 플래시·격파 폭발·화면 흔들림. (히트스톱·머즐 플래시는 추가 여지)
- ✅ **A2 사운드** 완료: 합성 SFX + 절차적 BGM. (실제 오디오 에셋/믹싱은 추후 고급화 여지)
- ✅ **A3 게임 플로우** 완료: 타이틀 화면 + 상태머신(Title/Playing/Paused/GameOver) + 일시정지(ESC/P) + 재시작. `GameManager.IsPlaying`로 발사/이탈/봄 게이트, timeScale로 정지.
- ✅ **A5 히트스톱** 완료: 보스사망/봄/피탄 시 `GameManager.HitStop(초)` 짧은 정지.
- **A4 UI 폴리시**(남음): HUD가 빌트인 폰트(legacy uGUI Text) — 픽셀 TTF/TMP 또는 pixellab `create_font` 필요(폰트 에셋 파이프라인 작업). **머즐 플래시·콤보 팝업·화면 플래시**도 남은 폴리시.

### B. 미구현 심화
- ✅ **B4 W06 록온레이저** 완료: `CollisionManager.FindNearestTargets`로 다중 락온 → 각 적에 유도 레이저(`WeaponController.FireLockOn`, `Bullet.lockTarget`).
- ✅ **B3 leaderBreakScatter** 완료: 편대 리더(members[0]) 격파 시 나머지 흩어짐(`Enemy.Scatter`, `FormationGroup.ScatterRest`). FormationData 3기능(wipe/scatter/leadPath) 중 2개 완료.
- ✅ **B1 지형(대표)**: `TerrainBlock`(IDamageable) 파괴 가능 스크롤 지형 블록 — 자기탄 차단·기체 접촉 피해. `SpawnKind.Terrain`으로 타임라인 배치. (풀 타일맵 네비/이동차단은 후속)
- ✅ **B2 촉수(대표)**: `BossTentacle` FABRIK 분절 체인 + **공격 상태머신**(Idle 꿈틀→Windup 코일·주황예고→Strike 채찍타격→Recover), 위상차로 번갈아 공격, 팁 접촉 피해. **길이 튜닝**(B01: 분절 25·reach 10 ≈ 기존의 3.5배 ~9.6), 타격/복귀 속도를 reach에 비례시켜 화면 건너 플레이어까지 도달. BossData `useTentacles`로 부착. (분리→합체, 완전 다관절 IK 무기화는 후속)
- 남음(대형, 별도 세션): **B3+ 편대 snake(leadPath 링버퍼)**, **B1+ 풀 타일맵 네비게이션/이동차단**, **B2+ 보스 합체·관절 IK**.

### C. 시스템/정리
- ✅ **C1 콤보 스코어링** 완료: 연속 처치 → ×1~8 배율, 시간초과/피탄 리셋(`GameManager` Combo/AddKillScore).
- ✅ **C3 문서 동기화** 완료: GDD.md / CLAUDE.md를 구현 현황으로 갱신.
- **C2** 풀링 보완(픽업·이펙트·FormationGroup은 현재 Instantiate/Destroy) — 남음.
- **C4** pixellab 스프라이트 확장(탄·아이템·이펙트·추가 적/보스) — 남음.
- 난이도 곡선(루프별 강화)도 후속.

---

## 5. 알려진 제약 / 메모
- 픽업·드롭·FormationGroup·이펙트는 **아직 풀링 아님**(저빈도라 허용, 물량 늘면 C2 필요).
- 보스 "분리"는 잔해 적 방사까지(="합체"는 미구현).
- **스테이지는 `S01_Space` 1개만 존재**, `StageDirector.loop=true`로 반복(스테이지 번호만 증가) — 전용 Stage2 없음. 촉수 보스 `B01`은 매 루프 끝 등장(= Stage1·2 동일 보스).
- HUD는 legacy uGUI Text + 빌트인 폰트(TMP 에센셜 미임포트).
- 검증 스크린샷은 `Assets/Screenshots/`(gitignore 대상).
- 함정 상세는 Claude 메모리(`~/.claude/projects/.../memory/`)에도 4건 기록됨(execute_code 제약·네임스페이스 충돌·Unlit·pixellab 파이프라인).
