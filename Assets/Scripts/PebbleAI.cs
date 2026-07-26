using UnityEngine;
using Photon.Pun;

public class PebbleAI : MonoBehaviourPun
{
    public float moveSpeed = 1f;
    public float detectRange = 5f;
    public float attackRange = 1.5f;

    public int damage = 35;

    private float nextAttackTime;
    public float attackCooldown = 2.5f;

    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        Transform player = FindNearestPlayer();

        if (player == null) return;

        float distance =
            Vector2.Distance(
                transform.position,
                player.position);

        if (distance > attackRange)
        {
            transform.position =
                EnemyMovementUtility.MoveTowardsWithoutPassingThroughWalls(
                    transform.position,
                    player.position,
                    moveSpeed,
                    Time.deltaTime);

            animator.SetBool("Run", true);
        }
        else
        {
            animator.SetBool("Run", false);

            if (Time.time >= nextAttackTime)
            {
                animator.SetTrigger("Attack");

                PlayerHealth hp =
                    player.GetComponent<PlayerHealth>();

                if (hp != null)
                    hp.NhanSatThuong(damage);

                nextAttackTime =
                    Time.time + attackCooldown;
            }
        }
    }

    Transform FindNearestPlayer()
    {
        GameObject[] players =
            GameObject.FindGameObjectsWithTag("Player");

        Transform nearest = null;
        float minDistance = Mathf.Infinity;

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