# XtreamShooter
Side Scroll Shooter Game

Below is a consolidated, project‑level summary and feature inventory based on the scripts and assets you have open. I included architecture, per‑system responsibilities (key scripts / data), current features, known issues discovered during inspection, and a prioritized remediation + testing checklist you can act on next.
Project summary — one line
•	2D side‑scroll shooter (tile‑based parallax background, pooled projectiles, wave/boss mission system, data‑driven with ScriptableObjects) implemented in Unity (target .NET Framework 4.7.1).
Architecture / main subsystems
•	Mission / Stage system
•	Purpose: drive ordered gameplay sequences (waves, dialog, boss).
•	Key scripts:
•	Assets/Scripts/StageScripts/StageController.cs — phase controller (Intro → Gameplay → BossIntro → BossFight → Outro). Executes StageAction[].
•	Assets/Scripts/MissionScripts/MissionData.cs — mission ScriptableObject (stage actions, tileset, rewards).
•	Assets/Scripts/MissionScripts/StageAction.cs — action definitions and parameters.
•	Assets/Scripts/MissionScripts/MissionTileSet.cs — background tileset and scroll settings.
•	Enemy spawning & waves
•	Purpose: spawn enemy waves and bosses, support procedural & authored wave definitions.
•	Key scripts:
•	Assets/Scripts/EnemyScripts/EnemySpawner.cs (and variants like StandardEnemySpawner, BaseEnemySpawner) — spawns waves and bosses; now supports SpawnWaveAndReturnId usage.
•	Assets/Scripts/EnemyScripts/WaveDatabase.cs & WaveDefinition.cs — defines wave entries (prefabs, counts, spacing) and boss entries.
•	Assets/Scripts/EnemyScripts/WaveNames.cs, SpawnEntry variants — data helpers.
•	Entity tracking & sync
•	Purpose: reliably know when enemies/bosses are alive to gate mission progression.
•	Key script:
•	Assets/Scripts/StageScripts/EntityTracker.cs — global enemy/boss sets + per‑wave mapping (enemy → waveId and waveId → set).
•	Enemy runtime / AI
•	Purpose: enemy behaviour, movement and shooting.
•	Key scripts:
•	Assets/Scripts/EnemyScripts/Enemy.cs — enemy lifecycle, health, shooting and OnDeath.
•	Assets/Scripts/AI/* — EnemyAIBehavior.cs (base), ZigzagBehavior.cs, SineWaveBehavior.cs, StraightBehavior.cs, AltitudeShiftBehavior.cs, StateMachineBehavior.cs.
•	Enemy shooting
•	Assets/Scripts/EnemyShootScripts/* — HomingShoot, BurstShoot, SpiralShoot, SpreadShoot, StraightShoot and EnemyShootBehavior.
•	Assets/Scripts/EnemyShootScripts/EnemyProjectile.cs — enemy projectile behavior.
•	Player & input
•	Purpose: player movement, firing, triggers.
•	Key scripts:
•	Assets/Scripts/PlayerScripts/PlayerController.cs — movement, camera bounds, shooting via pools, trigger events.
•	Assets/Scripts/PlayerScripts/* — PlayerSpawner, PlayerData, shooting behaviors (PlayerShootBehavior, PlayerHomingShoot, PlayerStraightShoot), PlayerProjectile.
•	Assets/InputSystem_Actions.cs — generated Input System bindings.
•	Boss system
•	Assets/Scripts/BossScripts/Boss.cs — boss health, phases, OnDefeat.
•	Assets/Scripts/BossScripts/BossEncounterController.cs — boss encounter orchestration (freeze scroll, spawn, UI, wait).
•	Assets/Scripts/BossScripts/BossData.cs — boss ScriptableObject.
•	Background & parallax
•	Assets/Scripts/Background/*:
•	BackgroundSpawner.cs — create per‑layer BackgroundLayer objects from MissionTileSet.
•	BackgroundLayer.cs — spawn/scroll/recycle tiles for one layer.
•	BackgroundTilePool.cs, BackgroundTile.cs, BackgroundTileData.cs, BackgroundLayerData.cs — pooling and tile metadata.
•	Uses SortingGroup + small Z offsets for parallax; spawn/cull coordinate conversion must use layer Z distance.
•	Pooling
•	Assets/Scripts/PoolingSystem/ObjectPooler.cs — generic pools for bullets and other prefabs (currently dequeues then immediately enqueues; see risks).
•	Assets/Scripts/Background/BackgroundTilePool.cs — dedicated tile pool with chunked expansion.
•	UI & Dialog
•	Assets/Scripts/UI/UIManager.cs — boss UI (health bar).
•	Assets/Scripts/DialogScripts/DialogManager.cs — dialog panel, skip/auto‑advance.
•	Utilities & others
•	Assets/Scripts/PRNG.cs, editor utilities (example asset creator, unused script scanner referenced earlier).
•	Assets/Scripts/Game/GameManager.cs, AudioScripts/AudioManager.cs.
Key features / gameplay flow
•	Data‑driven missions: designer composes sequences of StageActions in MissionData.
•	Wave spawning: WaveDatabase contains WaveDefinitions (prefab arrays, count, spacing).
•	Per‑wave instance tracking: EnemySpawner creates unique waveInstanceId per spawn; EntityTracker maps enemies to wave ids so StageController can wait on a specific wave.
•	Boss encounters: BossEncounterController handles arena freeze, spawn, UI binding, and waits for boss defeat (via EntityTracker).
•	Multiple enemy shooting behaviors and AI movement patterns.
•	Object pooling for bullets and tiles (background).
•	Parallax background built from layers (ScriptableObjects) with sorting groups and tile pooling.
