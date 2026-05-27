using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    public int currentHealth = 100;
    private Animator _animator;
    private Collider2D _collider;

    private void Awake() { _animator = GetComponent<Animator>(); _collider = GetComponent<Collider2D>(); }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        _animator?.SetTrigger("Hit");
        if (currentHealth <= 0) StartCoroutine(DieSequence());
    }

    private IEnumerator DieSequence()
    {
        if (_collider) _collider.enabled = false;
        _animator?.SetBool("Death", true);
        yield return new WaitForSeconds(0.8f);
        Destroy(gameObject);
    }
}