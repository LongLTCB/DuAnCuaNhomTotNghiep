using UnityEngine;
using Photon.Pun;

public class EnemyMeleeAI : MonoBehaviourPun
{
    public float moveSpeed = 2f;
    public float detectionRange = 5f;
    public float attackRange = 1f;
    public float attackCooldown = 1f;
    public int damage = 20;

    private float nextAttackTime;
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        Transform target = FindNearestPlayer();

        if (target == null)
            return;

        float dist =
            Vector2.Distance(
                transform.position,
                target.position);

        if (dist <= attackRange)
        {
            Attack(target);
        }
        else if (dist <= detectionRange)
        {
            Chase(target);
        }
    }

    void Chase(Transform target)
    {
        transform.position =
            EnemyMovementUtility.MoveTowardsWithoutPassingThroughWalls(
                transform.position,
                target.position,
                moveSpeed,
                Time.deltaTime);

        animator.SetBool("isWalking", true);
    }

    void Attack(Transform target)
    {
        animator.SetBool("isWalking", false);

        if (Time.time >= nextAttackTime)
        {
            animator.SetTrigger("attack");

            PhotonView pv =
                target.GetComponent<PhotonView>();

            if (pv != null)
            {
                pv.RPC(
                    "TakeDamage",
                    RpcTarget.All,
                    damage);
            }

            nextAttackTime =
                Time.time + attackCooldown;
        }
    }

    Transform FindNearestPlayer()
    {
        GameObject[] players =
            GameObject.FindGameObjectsWithTag(
                "Player");

        Transform nearest = null;

        float minDist = Mathf.Infinity;

        foreach (GameObject p in players)
        {
            float dist =
                Vector2.Distance(
                    transform.position,
                    p.transform.position);

            if (dist < minDist)
            {
                minDist = dist;
                nearest = p.transform;
            }
        }

        return nearest;
    }
}