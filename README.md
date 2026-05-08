# XtreamShooter
Side Scroll Shooter Game

XtreamShooter is a mission-based 2D scrolling space shooter (shmup/bullet hell game) built in Unity with extensive RPG-like progression systems, sophisticated enemy AI, and complex boss encounters.

Core Game Type
    Genre: 2D scrolling space shooter with mission progression
    Style: Modern bullet hell/shoot 'em up with RPG elements
    Platform: Unity-based (Windows compatible)
Feature List
    Core Gameplay Features
        Player Control System: Physics-based 2D movement with acceleration/deceleration
        Multiple Weapon Systems: Weapon switching between different projectile types
        Resource Management: Health, Shield (with regeneration), and Energy systems
        Damage System: Separate damage absorption for shields vs health with invincibility periods
    Enemy & AI Features
        Enemy Hierarchy: Normal → Elite (2x health) → Boss (5x health) enemies
        AI Behavior Variety:
        Straight pathing
        Zigzag patterns
        Sine wave movements
        Altitude shifting
        State machine behaviors (Idle, Patrol, PositionCannon, ShootCannon)
        Spawn System Types: Positioned, Rapid Fire, Simultaneous spawning
        Wave Management: Comprehensive wave definitions with spawn patterns
    Boss System Features
        Phase-based Combat: Boss health thresholds trigger phase transitions
        Dialog System: Boss taunts during phase changes
        Custom Behaviors: Unique movement patterns and attack sequences
        Boss Health UI: Auto-hiding health bars with phase tracking
    Mission & Progression System
        Phase-based Missions: Sequential actions per mission phase
        Action Types: Enemy spawning, dialog sequences, boss encounters, scrolling control
        Mission Chaining: Automatic loading of next missions upon completion
        Progression Triggers: Wave completion or player-based triggers
    Weapon & Combat Systems
        Projectile Types: Standard bullets, homing missiles, spread shots, spiral patterns
        Shooting Behaviors: Burst fire, spread patterns, homing projectiles
        Object Pooling: Performance-optimized projectile management
        Weapon Switching: Dynamic weapon selection during gameplay
Visual & Audio Features
    Parallax Backgrounds: Multi-layer scrolling backgrounds with tile recycling
    Particle Effects: Combat visuals and environmental effects
    Audio Management: SFX and music with spatial audio support
    UI/HUD System: Comprehensive health bars, mission timers, dialog displays
Technical Features
    Input System: Unity's Input System package with configurable controls
    Event-Driven Architecture: C# events for system communication
    Data-Driven Design: ScriptableObject-based configuration
    Performance Optimization: Object pooling and entity tracking
    Persistence: Mission progression and high score tracking
Game Flow Features
    Mission Start: Background initialization + UI timer
    Phase Execution: Sequential action execution
    Combat: Enemy wave patterns with AI behaviors
    Boss Battles: Phase-based encounters
    Mission Completion: Reward calculation and progression
    Game Over: Restart/quit options with persistence