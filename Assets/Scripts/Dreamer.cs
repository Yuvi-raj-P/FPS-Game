using UnityEngine;
using System.Collections;

public class Dreamer : MonoBehaviour
{
    public GameObject dicePrefab;
    public Transform spawnPoint;
    public float rollForce = 5f;
    public float torqueForce = 5f;
    public float throwForceMultiplier = 1f;

    //Effects and Cinematic stuff
    public GameObject eyesOpenImage;
    public GameObject eyesClosedImage;
    public GameObject eyesShockedImage;

    void Start()
    {
        if (eyesOpenImage != null)
        {
            eyesOpenImage.SetActive(true);
        }
        if (eyesClosedImage != null)
        {
            eyesClosedImage.SetActive(false);

        }
        if (eyesShockedImage != null)
        {
            eyesShockedImage.SetActive(false);
        }
        else
        {
            Debug.LogWarning("MISSING DREAMER IMAGES BRO FIX THIS ASPAPSADJFIASD!");
        }
        StartCoroutine(CloseEyesSequence());
        StartCoroutine(StartRollingDice());
    }
    IEnumerator CloseEyesSequence()
    {
        yield return new WaitForSeconds(4f);
        if (eyesOpenImage != null)
        {
            eyesOpenImage.SetActive(false);
        }
        if (eyesClosedImage != null)
        {
            eyesClosedImage.SetActive(true);
        }

    }
    IEnumerator StartRollingDice()
    {
        while (true)
        {
            float randomDelay = Random.Range(60f, 120f);
            yield return new WaitForSeconds(randomDelay);
            RollDice();
        }
    }
    void RollDice()
    {
        if (dicePrefab == null || spawnPoint == null)
        {
            Debug.LogWarning("Die prefab or spawn point is not assigned in the Inspector.");
            return;
        }
        GameObject die1 = Instantiate(dicePrefab, spawnPoint.position, Random.rotation);
        Rigidbody rb1 = die1.GetComponent<Rigidbody>();
        if (rb1 != null)
        {
            Vector3 throwDirection = (spawnPoint.forward + (Vector3.up * 1f));
            throwDirection += new Vector3(
                Random.Range(-4f, 4f),
                Random.Range(-0.1f, 0.4f),
                Random.Range(-0.9f, 0.9f)
            );
            rb1.AddForce(throwDirection.normalized * rollForce * throwForceMultiplier, ForceMode.Impulse);
            rb1.AddTorque(Random.insideUnitSphere * torqueForce, ForceMode.Impulse);
            
            
        }
        

        int diceResult = Random.Range(1, 7);
        Debug.Log("Dice rolled: " + diceResult);
    }
    void Update()
    {
        
    
    }
}
