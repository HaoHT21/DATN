using UnityEngine;

public class BossAI : MonoBehaviour
{
    public float moveSpeed = 3f, detectionRange = 10f, attackRange = 2.5f;
    private bool _isDead;
    private Animator _animator;
    private SpriteRenderer _sprite;
    private Transform _target;

    private void Awake() { _animator = GetComponent<Animator>(); _sprite = GetComponent<SpriteRenderer>(); }

    private void Update()
    {
        if (_isDead) return;
        FindPlayer();
        if (_target == null) return;

        float dist = Vector3.Distance(transform.position, _target.position);
        if (dist <= detectionRange)
        {
            if (dist > attackRange)
            {
                Vector3 dir = (_target.position - transform.position).normalized;
                transform.position += dir * moveSpeed * Time.deltaTime;
                _sprite.flipX = dir.x < 0;
                _animator.SetBool("isWalking", true);
            }
            else { _animator.SetBool("isWalking", false); _animator.SetTrigger("Attack"); }
        }
    }

    void FindPlayer() { GameObject p = GameObject.FindGameObjectWithTag("Player"); if (p) _target = p.transform; }
    public void SetDead() { _isDead = true; _animator.SetBool("Death", true); Destroy(gameObject, 2f); }
}