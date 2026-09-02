using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    [SerializeField]
    private List<string> _boolNames;
    [SerializeField]
    private Animator _animator;

    private void Start()
    {
        _animator = GetComponent<Animator>();
    }

    private void SetAllFalseExcept(int index)
    {
        for (int i = 0; i < _boolNames.Count; i++)
        {
            _animator.SetBool(_boolNames[i], i == index);
        }
    }

    public void PlayIdle()
    {
        SetAllFalseExcept(0);
    }

    public void PlayRun()
    {
        SetAllFalseExcept(1);
    }

    public void PlaySprint()
    {
        SetAllFalseExcept(2);
    }

    public void PlayShoot()
    {
        SetAllFalseExcept(3);
    }

    public void PlayExplotion()
    {
        StartCoroutine(Explotion());
    }

    IEnumerator Explotion()
    {
        SetAllFalseExcept(4);
        yield return new WaitForSeconds(2);
        PlayIdle();
    }
}