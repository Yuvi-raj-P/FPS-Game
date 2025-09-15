using UnityEngine;
using System.Collections;
using Unity.VisualScripting.FullSerializer;

[RequireComponent(typeof(CharacterController))]
public class Boos2 : MonoBehaviour
{
    [Header("Boss Phase Settings")]
    public float phase2HealthThreshold = 0.6f;
    public float phase3HealthThreshold = 0.3f;
    private int currentPhase = 1;

    [Header("Boss Movement Settings")]
    public Transform player;
    public float maxSpeed = 6f;
    public float baseAcceleration = 1.5f;
    public float stoppingDistance = 8f;
    public float accelerationMultiplier = 1f;
    public float flightHeight = 5f;


    [Header("Boss Flying Animation Settings")]
    public float bobbingAmplitude = 0.8f;
    public float bobbingFrequency = 1.5f;
    public float bankingAngle = 30f;
    public float bankingSpeed = 2f;
    public float heightAdjustmentSpeed = 2f;
    public float naturalDrift = 0.5f;
    
}
