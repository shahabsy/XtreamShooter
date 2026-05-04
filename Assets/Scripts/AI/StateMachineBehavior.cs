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

    public override void Initialize(Enemy owner, EnemyData enemyData)
    {
        base.Initialize(owner, enemyData);
        if (states == null || states.Length == 0) return;
        EnterState(0);
    }

    private void EnterState(int index)
    {
        if (index < 0 || index >= states.Length) return;
        currentStateIndex = index;
        StateDefinition state = states[currentStateIndex];
        stateTimer = state.duration;
        hasReachedDestination = false;

        if(state.useDestination)
        {
            float destX = UnityEngine.Random.Range(state.destinationMin.x, state.destinationMax.x);
            float destY = UnityEngine.Random.Range(state.destinationMin.y, state.destinationMax.y);
            currentDestination = new Vector2(destX, destY);
        }

        if(state.pauseWeaponIndices != null && state.pauseWeaponIndices.Count > 0)
        {
            enemy.SetWeaponsPaused(state.pauseWeaponIndices, true);
        }

        if (state.fireWeaponImmediately)
            enemy.FireWeaponImmediately(state.weaponIndexToFire);
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

        if(state.duration > 0 )
        {
            stateTimer -= deltaTime;
            if(stateTimer <= 0)
            {
                int nextIndex = System.Array.FindIndex(states, s => s.stateName == state.nextState);
                if(nextIndex >= 0)
                {
                    ExitState();
                    EnterState(nextIndex);
                }
                return;
            }
        }

        if(state.useDestination && !hasReachedDestination)
        {
            float distance = Vector2.Distance(enemy.transform.position, currentDestination);
            if(distance < 0.1f)
            {
                hasReachedDestination = true;
                int nextIndex = System.Array.FindIndex(states, s => s.stateName == state.nextState);
                if(nextIndex >= 0)
                {
                    ExitState();
                    EnterState(nextIndex);
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
            return direction * data.moveSpeed;
        }
        else
        {
            desiredVel = state.targetVelocity;
        }

        // Add Seperation force
        Vector2 seperation = GetSeperationForce();
        Vector2 finalVel = desiredVel + seperation * seperationStrength;
        finalVel = Vector2.ClampMagnitude(finalVel, data.moveSpeed);
        return finalVel;
    }
    private Vector2 GetSeperationForce()
    {
        Vector2 force = Vector2.zero;
        int count = 0;
        Collider2D[] hits = Physics2D.OverlapCircleAll(enemy.transform.position, seperationRadius, LayerMask.GetMask("Enemies"));
        foreach(var hit in hits)
        {
            if (hit.gameObject == enemy.gameObject) continue;
            Vector2 dir = enemy.transform.position - hit.transform.position;
            float dist = dir.magnitude;
            if(dist < seperationRadius && dist > 0.01f)
            {
                force += dir.normalized * (1f - dist / seperationRadius);
                count++;
            }
        }
        if (count > 0)
            force /= count;
        return force;
    }

    public override void ResetState()
    {
        base.ResetState();
        if (states != null && states.Length > 0)
            EnterState(0);
    }
}
