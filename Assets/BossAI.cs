using UnityEngine;
using Photon.Pun;

public class BossAI : MonoBehaviourPun
{
    public float moveSpeed = 2f;

    public float detectRange = 10f;

    public float attackRange = 2f;

    public int damage = 50;

    public float attackCooldown = 2f;

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

        Transform player =
            FindNearestPlayer();

        if (player == null)
            return;

        float distance =
            Vector2.Distance(
                transform.position,
                player.position);

        if (distance > attackRange)
        {
            Chase(player);
        }
        else
        {
            Attack(player);
        }
    }

    void Chase(Transform player)
    {
        transform.position =
            EnemyMovementUtility.MoveTowardsWithoutPassingThroughWalls(
                transform.position,
                player.position,
                moveSpeed,
                Time.deltaTime);

        if (animator != null)
            animator.SetBool("Run", true);
    }

    void Attack(Transform player)
    {
        if (animator != null)
            animator.SetBool("Run", false);

        if (Time.time >= nextAttackTime)
        {
            if (animator != null)
                animator.SetTrigger("Attack");

            PlayerHealth hp =
                player.GetComponent<PlayerHealth>();

            if (hp != null)
            {
                hp.NhanSatThuong(damage);
            }

            nextAttackTime =
                Time.time + attackCooldown;
        }
    }

    Transform FindNearestPlayer()
    {
        GameObject[] players =
            GameObject.FindGameObjectsWithTag("Player");

        Transform nearest = null;

        float minDistance =
            Mathf.Infinity;

        foreach (GameObject p in players)
        {
            float dist =
                Vector2.Distance(
                    transform.position,
                    p.transform.position);

            if (dist < minDistance)
            {
                minDistance = dist;
                nearest = p.transform;
            }
        }

        return nearest;
    }
}