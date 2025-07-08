using UnityEngine;
using System.Collections;

public class Dreamer : MonoBehaviour
{
    public GameObject dicePrefab;
    public Transform spawnPoint;
    public float rollForce = 5f;
    public float torqueForce = 5f;
    public float throwForceMultiplier = 1f;
    void Start()
    {
        StartCoroutine(StartRollingDice());
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
        /*if (Input.GetKeyDown(KeyCode.W))
        {
            RollDice();
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (dicePrefab != null && spawnPoint != null)
            {
                GameObject die = Instantiate(dicePrefab, spawnPoint.position, Random.rotation);
                Rigidbody rb = die.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 throwDirection = (spawnPoint.forward + (Vector3.up * 1f));
                    throwDirection += new Vector3(
                        Random.Range(-0.5f, 0.5f),
                        Random.Range(-0.1f, 0.4f),
                        Random.Range(-0.5f, 0.5f)
                    );
                    rb1.AddForce(throwDirection.normalized * rollForce * throwForceMultiplier, ForceMode.Impulse);
                    rb1.AddTorque(Random.insideUnitSphere * torqueForce, ForceMode.Impluse);

                }
            }
        }*/
    }
}
