using UnityEngine;

public class WallScripts : MonoBehaviour
{
    private Animator _animator;

    void OnEnable()
    {
        _animator = GetComponent<Animator>();
        _animator.Play("Appear", 0, 0);
    }
}
