using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
[CreateAssetMenu(fileName = "StateMachineBehavior", menuName = "Game/Enemy/AI/StateMachine")]
public class StateMachineBehavior : EnemyAIBehavior
{
    [System.Serializable]
    public class StateDefinition
    {
        public string stateName;
        public float duration = -1f; // indefinite
        public string nextState;
        public Vector2 targetVelocity;
        public bool useDestination = false;
        public Vector2 destinationMin;
        public Vector2 destinationMax;
        public List<int> pauseWeaponIndices;
        public bool fireWeaponImmediately = false;
        public int weaponIndexToFire = 0;
        public bool isExitState = false;
    }

    public StateDefinition[] states;
    public float acceleration = 10f;

    [Header("Seperation")]
    public float seperationRadius = 2f;
    public float seperationStrength = 2f;

    private int currentStateIndex;
    private float stateTimer;
    private Vector2 currentDestination;
    private bool hasReachedDestination;

    private Dictionary<string, int> stateNameToIndex;
    private static Collider2D[] overlapBuffer = new Collider2D[32];
    private static int enemyLayerMask = -1;

    private void ComputerLayerMaskOnce()
    {
        if (enemyLayerMask == -1)
            enemyLayerMask = LayerMask.GetMask("Enemies");
    }

    public override void Initialize(Enemy owner, EnemyData enemyData)
    {
        base.Initialize(owner, enemyData);
        ComputerLayerMaskOnce();
        BuildTransitionMap();
        if (states == null || states.Length == 0) return;
        EnterState(0);
    }
    private void BuildTransitionMap()
    {
        if (states == null) return;
        stateNameToIndex = new Dictionary<string, int>(states.Length);
        for(int i = 0; i < states.Length; i++)
        {
            if (!string.IsNullOrEmpty(states[i].stateName))
                stateNameToIndex[states[i].stateName] = i;
        }
    }

    private void EnterState(int index)
    {
        if (index < 0 || index >= states.Length) return;
        currentStateIndex = index;
        StateDefinition state = states[currentStateIndex];
        stateTimer = state.duration;
        hasReachedDestination = false;

        if(state.useDestination && !state.isExitState)
        {
            float destX = UnityEngine.Random.Range(state.destinationMin.x, state.destinationMax.x);
            float destY = UnityEngine.Random.Range(state.destinationMin.y, state.destinationMax.y);
            currentDestination = new Vector2(destX, destY);
        } 
        else if(state.isExitState)
        {
            if(Camera.main != null)
            {
                float randY = UnityEngine.Random.Range(0f, 1f);
                Vector3 viewportPoint = new Vector3(-0.2f, randY, 0f);
                currentDestination = Camera.main.ViewportToWorldPoint(viewportPoint);
            }
            else
            {
                currentDestination = new Vector2(-15, 0);
            }
        }

        if (state.pauseWeaponIndices != null && state.pauseWeaponIndices.Count > 0)
        {
            enemy.SetWeaponsPaused(state.pauseWeaponIndices, true);
        }

        if (state.fireWeaponImmediately)
            enemy.FireWeaponImmediately(state.weaponIndexToFire);
    }
    private Vector2 ClampToScreenBoundary(Vector2 worldPos)
    {
        if(Camera.main == null) return worldPos;
        Vector3 bottomleft = Camera.main.ViewportToWorldPoint(Vector3.zero);
        Vector3 topRight = Camera.main.ViewportToWorldPoint(Vector3.one);
        float margin = 2f;
        worldPos.x = Mathf.Clamp(worldPos.x, bottomleft.x - margin, topRight.x + margin);
        worldPos.y = Mathf.Clamp(worldPos.y, bottomleft.y - margin, topRight.y + margin);
        return worldPos;
    }
    private void ExitState()
    {
        StateDefinition state = states[currentStateIndex];
        if (state.pauseWeaponIndices != null && state.pauseWeaponIndices.Count > 0)
            enemy.SetWeaponsPaused(state.pauseWeaponIndices, false);
    }

    public override void UpdateLogic(float deltaTime)
    {
        base.UpdateLogic(deltaTime);
        if (states == null || states.Length == 0) return;

        StateDefinition state = states[currentStateIndex];

        if(state.duration > 0 && !state.isExitState)
        {
            stateTimer -= deltaTime;
            if(stateTimer <= 0 && stateNameToIndex.TryGetValue(state.nextState, out int next))
            {
                ExitState();
                EnterState(next);
                return;
            }
        }

        if(state.useDestination && !hasReachedDestination)
        {
            float distance = Vector2.Distance(enemy.transform.position, currentDestination);
            if(distance < 0.2f)
            {
                hasReachedDestination = true;
                if(state.isExitState)
                {
                    enemy.Die();
                    return;
                }
                if(stateNameToIndex.TryGetValue(state.nextState, out int next))
                {
                    ExitState();
                    EnterState(next);
                }
            }
        }
    }

    public override Vector2 GetVelocity()
    {
        if (states == null || states.Length == 0)
            return Vector2.left * data.moveSpeed;

        StateDefinition state = states[currentStateIndex];
        Vector2 desiredVel;
        if(state.useDestination && !hasReachedDestination)
        {
            Vector2 direction = (currentDestination - (Vector2)enemy.transform.position).normalized;
            desiredVel = direction * data.moveSpeed;
        }
        else
        {
            desiredVel = state.targetVelocity;
        }

        // Add Seperation force
        Vector2 seperation = GetSeperationForce();
        Vector2 finalVel = desiredVel + seperation * seperationStrength;

        // Allow seperation to dominate but still respect speed limit
        float maxSpeed = data.moveSpeed;
        if (finalVel.magnitude > maxSpeed * 1.5f)
            finalVel = finalVel.normalized * maxSpeed * 1.5f;

        return finalVel;
    }
    private Vector2 GetSeperationForce()
    {
        Vector2 force = Vector2.zero;

        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(enemyLayerMask);
        filter.useLayerMask = true;
        int hitCount = Physics2D.OverlapCircle(enemy.transform.position,
            seperationRadius, filter, overlapBuffer);
        for(int i = 0; i < hitCount; i++)
        {
            Collider2D hit = overlapBuffer[i];
            if (hit == null || hit.gameObject == enemy.gameObject) continue;
            Vector2 dir = (Vector2)(enemy.transform.position - hit.transform.position);

            float dist = dir.magnitude;
            if (dist < 0.001f) continue;

            float strength = (seperationRadius - dist) / seperationRadius;
            force += dir.normalized * strength * strength;
        }
        return force;
    }

    public override void ResetState()
    {
        base.ResetState();
        if (states != null && states.Length > 0 && stateNameToIndex != null && stateNameToIndex.TryGetValue(states[0].stateName, out int index))
            EnterState(index);
        else if (states != null && states.Length > 0)
            EnterState(0);
    }
}
