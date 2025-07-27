using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WispController : EnemyBaseClass
{
    public float chaseDistance = 5f;
    public float attackDistance = 1f;
    private float realmSpeed = 2f;

    private Transform playerTransform;

    void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        realmSpeed = speed * 2;
    }

    void Update()
    {
        if (Vector3.Distance(transform.position, playerTransform.position) < chaseDistance)
        {
            ChasePlayer();
        }
    }

    void ChasePlayer()
    {
        float speed = RealityManager.Instance.currentRealityType == RealityType.Spirit ? realmSpeed : this.speed;
        Vector3 direction = (playerTransform.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;

        if (Vector3.Distance(transform.position, playerTransform.position) < attackDistance)
        {
            if (canAttack)
            {
                AttackPlayer();
                lastAttackTime = Time.time;
                canAttack = false;
            }
        }
    }

    void AttackPlayer()
    {
        // Implement attack logic here
        Debug.Log("Attacking Player!");
    }
}
