using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class preyAI : MonoBehaviour
{
    public enum Gender { Male, Female }

    private bool isStunned = false;
    public Gender gender; 
    public float maxEnergy = 100;
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
    public string fruitTag = "Fruit";
    private GameObject targetFruit;
    public Transform preyTransform;
    private float randomMovementTimer;
    public string predatorTag = "Predator";
    void Start()
    {
        speed = Random.Range(5f, 25f);
        sightLength = Random.Range(5f, 10f);
        preyTransform = transform;
        randomMovementTimer = randomMovementInterval;

       
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

        if (energy < 70)
        {
            SearchForFood();
        }
        else
        {
            SearchForMate();
        }
    }

    void SearchForFood()
    {
        bool hitFruit = false;
        bool spotPredator = false;
        for (int i = 0; i < numRays; i++)
        {
            float angle = fieldOfViewAngle * ((float)i / (numRays - 1)) - fieldOfViewAngle / 2.0f;
            Vector2 rayDirection = Quaternion.Euler(0, 0, angle) * preyTransform.up;

            RaycastHit2D[] rayHits = Physics2D.RaycastAll(preyTransform.position, rayDirection, sightLength);

            Debug.DrawRay(preyTransform.position, rayDirection * sightLength, Color.red);

            foreach (RaycastHit2D rayHit in rayHits)
            {
                if (rayHit.collider != null && rayHit.collider.gameObject.tag == fruitTag)
                {
                    hitFruit = true;
                    targetFruit = rayHit.collider.gameObject;
                    break;
                }
                else if (rayHit.collider != null && rayHit.collider.gameObject.tag == predatorTag )
                {
                    spotPredator = true;
                    break;
                }
            }
        }

        if (hitFruit)
        {

            Vector3 direction = (targetFruit.transform.position - preyTransform.position).normalized;
            preyTransform.Translate(direction * speed * Time.deltaTime);

            float distanceToTarget = Vector2.Distance(preyTransform.position, targetFruit.transform.position);
            if (distanceToTarget < 1.0f)
            {
                energy = Mathf.Min(maxEnergy, energy + energyGain);
                Destroy(targetFruit);
                targetFruit = null;
            }
        }
        else if (spotPredator)
        {
            MoveRandomly();
            spotPredator = false;
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
                preyTransform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);
            }
        }
    }

    void SearchForMate()
    {
        Collider2D[] nearbyPrey = Physics2D.OverlapCircleAll(preyTransform.position, sightLength);

        // Draw debug raycasts during the search for a mate
        for (int i = 0; i < numRays; i++)
        {
            float angle = fieldOfViewAngle * ((float)i / (numRays - 1)) - fieldOfViewAngle / 2.0f;
            Vector2 rayDirection = Quaternion.Euler(0, 0, angle) * preyTransform.up;

            RaycastHit2D[] rayHits = Physics2D.RaycastAll(preyTransform.position, rayDirection, sightLength);

            Debug.DrawRay(preyTransform.position, rayDirection * sightLength, Color.green);

            foreach (RaycastHit2D rayHit in rayHits)
            {
                if (rayHit.collider != null && rayHit.collider.CompareTag("Prey"))
                {
                    preyAI otherPrey = rayHit.collider.GetComponent<preyAI>();

                    if (otherPrey != null && otherPrey.gameObject != gameObject && otherPrey.gender != gender && otherPrey.energy >= reproductionEnergyThreshold)
                    {
                        // Mate with the other prey
                        MateWithPrey(otherPrey);
                        return;
                    }
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
            preyTransform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);
        }
    }


    void MateWithPrey(preyAI mate)
    {
        
        energy -= 30f;
        mate.energy -= 30f;

        InstantiateNewPrey();

        randomMovementTimer = randomMovementInterval;
    }

    void InstantiateNewPrey()
    {
        // Instantiate a new prey object at the current position
        Instantiate(gameObject, preyTransform.position, Quaternion.identity);

    }


    void Die()
    {
        Destroy(gameObject);
    }
    void MoveRandomly()
    {
        Vector2 randomDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
        preyTransform.Translate(randomDirection * speed * Time.deltaTime);
    }


    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Predator")
        {
            FleeFromPredator(collision.transform);
        }
        else if (collision.gameObject.tag == "Wall" && !isStunned)
        {
            StartCoroutine(Stun());
            Vector3 normal = collision.GetContact(0).normal;
            Vector3 direction = Vector3.Reflect(preyTransform.position, normal);
            preyTransform.position = direction;
        }
    }

    IEnumerator Stun()
    {
        isStunned = true;
        yield return new WaitForSeconds(5f);
        isStunned = false;
    }
    void FleeFromPredator(Transform predatorTransform)
    {
        // Calculate the direction away from the predator
        Vector3 fleeDirection = preyTransform.position - predatorTransform.position;

        // Normalize the direction to maintain the speed
        fleeDirection = fleeDirection.normalized;

        // Move away from the predator with a certain speed
        preyTransform.Translate(fleeDirection * speed * Time.deltaTime);
    }

}
