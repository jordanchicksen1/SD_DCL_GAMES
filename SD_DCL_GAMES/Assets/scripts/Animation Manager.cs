using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField]
    private List<string> _boolNames;

    [SerializeField]
    private Animator _animator;

    private Coroutine explosionCoroutine;
    private Coroutine shootCoroutine;

    public bool IsExploding { get; private set; }
    public bool IsShooting { get; private set; }

    private void Awake()
    {
        if (_animator == null)
        {
            _animator = GetComponent<Animator>();
        }
    }

    private void SetAllFalseExcept(int index)
    {
        if (_animator == null)
            return;

        for (int i = 0; i < _boolNames.Count; i++)
        {
            _animator.SetBool(_boolNames[i], i == index);
        }
    }

    public void PlayIdle()
    {
        if (IsExploding || IsShooting)
            return;

        SetAllFalseExcept(0);
    }

    public void PlayRun()
    {
        if (IsExploding || IsShooting)
            return;

        SetAllFalseExcept(1);
    }

    public void PlaySprint()
    {
        if (IsExploding || IsShooting)
            return;

        SetAllFalseExcept(2);
    }

    public void PlayShoot()
    {
        if (IsExploding)
            return;

        if (shootCoroutine != null)
        {
            StopCoroutine(shootCoroutine);
        }

        shootCoroutine = StartCoroutine(Shoot());
    }

    private IEnumerator Shoot()
    {
        IsShooting = true;

        SetAllFalseExcept(3);

        // Wait for the kick animation to finish.
        yield return new WaitForSeconds(0.5f);

        IsShooting = false;
        shootCoroutine = null;
    }

    public void PlayExplotion()
    {
        if (explosionCoroutine != null)
        {
            StopCoroutine(explosionCoroutine);
        }

        explosionCoroutine = StartCoroutine(Explotion());
    }

    private IEnumerator Explotion()
    {
        IsExploding = true;

        SetAllFalseExcept(4);

        yield return new WaitForSeconds(2f);

        IsExploding = false;

        PlayIdle();

        explosionCoroutine = null;
    }
}