using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using Random = UnityEngine.Random;

public class PredatorAI : MonoBehaviour
{
    public enum Gender { Male, Female }

    public Gender gender;
    public float maxEnergy = 200;
    public float energy = 50;
    public float energyGain = 10;
    public float energyLossPerSecond = 2;
    public float speed;
    public float sightLength;
    public int numRays = 10;
    public float rotationSpeed = 45.0f;
    public float fieldOfViewAngle = 60f;
    public float reproductionEnergyThreshold = 80;
    public float randomMovementInterval = 10.0f;
    public string PreyTag = "Prey";
    private GameObject targetPrey;
    private GameObject FoundMater;
    public Transform predatorTransform;
    private float randomMovementTimer;

    void Start()
    {
        speed = Random.Range(10f, 25f);
        sightLength = Random.Range(10f, 20f);
        predatorTransform = transform;
        randomMovementTimer = randomMovementInterval;

        // Assign a random gender to the predator
        gender = (Random.value < 0.5f) ? Gender.Male : Gender.Female;
    }

    void Update()
    {
        energy -= energyLossPerSecond * Time.deltaTime;
        energy = Mathf.Clamp(energy, 0, maxEnergy);

        if (energy <= 0)
        {
            Die();
            return;
        }

        randomMovementTimer -= Time.deltaTime;

        
    }

    private void FixedUpdate()
    {
        if (energy < 70)
        {
            SearchForPrey();
        }
        else
        {
            SearchForMate();
        }
    }

    void SearchForPrey()
    {
        bool foundPrey = false;

        for (int i = 0; i < numRays; i++)
        {
            float angle = fieldOfViewAngle * ((float)i / (numRays - 1)) - fieldOfViewAngle / 2.0f;
            Vector2 rayDirection = Quaternion.Euler(0, 0, angle) * predatorTransform.up;

            RaycastHit2D[] rayHits = Physics2D.RaycastAll(predatorTransform.position, rayDirection, sightLength);

            Debug.DrawRay(predatorTransform.position, rayDirection * sightLength, Color.red);

            foreach (RaycastHit2D rayHit in rayHits)
            {
                if (rayHit.collider != null && rayHit.collider.gameObject.tag == PreyTag)
                {
                    foundPrey = true;
                    targetPrey = rayHit.collider.gameObject;
                    break;
                }
            }
        }

        if (foundPrey)
        {
            Vector3 direction = (targetPrey.transform.position - predatorTransform.position).normalized;
            predatorTransform.Translate(direction * speed * Time.deltaTime);

            float distanceToTarget = Vector2.Distance(predatorTransform.position, targetPrey.transform.position);
            if (distanceToTarget < 1.0f)
            {
                energy = Mathf.Min(maxEnergy, energy + energyGain);
                Destroy(targetPrey);
                targetPrey = null;
            }
        }
        else
        {
            if (randomMovementTimer <= 0)
            {
                MoveRandomly();
                randomMovementTimer = randomMovementInterval;
            }
            else
            {
                predatorTransform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);
            }
        }
    }

    void SearchForMate()
    {
        
        for (int i = 0; i < numRays; i++)
        {
            float angle = fieldOfViewAngle * ((float)i / (numRays - 1)) - fieldOfViewAngle / 2.0f;
            Vector2 rayDirection = Quaternion.Euler(0, 0, angle) * predatorTransform.up;

            RaycastHit2D[] rayHits = Physics2D.RaycastAll(predatorTransform.position, rayDirection, sightLength);

            Debug.DrawRay(predatorTransform.position, rayDirection * sightLength, Color.green);

            foreach (RaycastHit2D rayHit in rayHits)
            {
                PredatorAI otherPredator = rayHit.collider.GetComponent<PredatorAI>();

                if (otherPredator != null && otherPredator.gameObject != gameObject && otherPredator.gender != gender && otherPredator.energy >= reproductionEnergyThreshold)
                {
                    // Mate with the other prey
                    MateWithPredator(otherPredator);
                    return;
                }
            }
        }
        

        // If no mate is found, continue random movement
        if (randomMovementTimer <= 0)
        {
            MoveRandomly();
            randomMovementTimer = randomMovementInterval;
        }
        else
        {
            predatorTransform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);
        }
    }

    void MateWithPredator(PredatorAI mate)
    {
        energy -= energyGain;
        mate.energy -= energyGain;
        
        InstantiateNewPredator();
        
        randomMovementTimer = randomMovementInterval;
    }

    void InstantiateNewPredator()
    {
        
        Instantiate(gameObject, predatorTransform.position, Quaternion.identity);
    }

    void Die()
    {
        Destroy(gameObject);
    }

    void MoveRandomly()
    {
        Vector2 randomDirection = new Vector2(Random.Range(-80f, 80f), Random.Range(-80f, 80f)).normalized;
        predatorTransform.Translate(randomDirection * speed * Time.deltaTime);
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Wall")
        {
            Stun();
        }
        
    }

    void Stun()
    {
        StartCoroutine(StunCoroutine());
    }

    IEnumerator StunCoroutine()
    {
        speed = 0;
        yield return new WaitForSeconds(5.0f);
        speed = Random.Range(5f, 10f);
        MoveOppositeOfWall();
    }

    void MoveOppositeOfWall()
    {
        // Determine the opposite direction of the wall and move in that direction
        Vector3 direction = -predatorTransform.right;
        predatorTransform.Translate(direction * speed * Time.deltaTime);
    }
}

