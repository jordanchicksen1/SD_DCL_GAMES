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

    public bool IsExploding { get; private set; }

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
        if (IsExploding)
            return;

        SetAllFalseExcept(0);
    }

    public void PlayRun()
    {
        if (IsExploding)
            return;

        SetAllFalseExcept(1);
    }

    public void PlaySprint()
    {
        if (IsExploding)
            return;

        SetAllFalseExcept(2);
    }

    public void PlayShoot()
    {
        if (IsExploding)
            return;

        SetAllFalseExcept(3);
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