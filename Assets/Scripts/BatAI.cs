using UnityEngine;
using Photon.Pun;

public class BatAI : MonoBehaviourPun
{
    [Header("Movement")]
    public float moveSpeed = 3f;

    [Header("Detection")]
    public float detectRange = 6f;
    public float attackRange = 1.2f;

    [Header("Attack")]
    public int damage = 20;
    public float attackCooldown = 1.5f;

    private float nextAttackTime;

    private Animator animator;
    private EnemyHealth enemyHealth;

    void Start()
    {
        animator = GetComponent<Animator>();
        enemyHealth = GetComponent<EnemyHealth>();
    }

    void Update()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (enemyHealth != null &&
            enemyHealth.GetCurrentHealth() <= 0)
            return;

        Transform player = FindNearestPlayer();

        if (player == null)
        {
            animator.SetBool("Run", false);
            return;
        }

        float distance =
            Vector2.Distance(
                transform.position,
                player.position);

        // Ngoài vùng phát hiện
        if (distance > detectRange)
        {
            animator.SetBool("Run", false);
            return;
        }

        FaceTarget(player);

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

        animator.SetBool("Run", true);
    }

    void Attack(Transform player)
    {
        animator.SetBool("Run", false);

        if (Time.time >= nextAttackTime)
        {
            photonView.RPC(
                "SyncAttack",
                RpcTarget.All);

            PhotonView playerPV =
                player.GetComponent<PhotonView>();

            if (playerPV != null)
            {
                playerPV.RPC(
                    "TakeDamage",
                    RpcTarget.All,
                    damage);
            }

            nextAttackTime =
                Time.time + attackCooldown;
        }
    }

    [PunRPC]
    void SyncAttack()
    {
        animator.SetTrigger("Attack");
    }

    void FaceTarget(Transform target)
    {
        Vector3 scale =
            transform.localScale;

        if (target.position.x >
            transform.position.x)
        {
            scale.x =
                Mathf.Abs(scale.x);
        }
        else
        {
            scale.x =
                -Mathf.Abs(scale.x);
        }

        transform.localScale =
            scale;
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

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(
            transform.position,
            detectRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(
            transform.position,
            attackRange);
    }
}