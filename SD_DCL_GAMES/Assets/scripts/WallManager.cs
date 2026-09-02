using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class WallManager : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> _walls;
    [SerializeField]
    private GameObject _currentWall, _lastSpwnedWall;
    [SerializeField]
    private int _spawnWaitTimer, _wallTimer;

    private void Start()
    {
        StartCoroutine(SpawnRandomWall());
    }
    void ActivateWall()
    {
        if (_currentWall != null)
        {
            _currentWall.SetActive(true);
        }
    }

    void RemoveWall()
    {
        if (_currentWall != null)
        {
            _currentWall.SetActive(false);
            _currentWall = null;
        }
    }

    void ChooseRandomWall()
    {
        if(_lastSpwnedWall == null)
        {
            _currentWall = _walls[Random.Range(0, _walls.Count)];
            _lastSpwnedWall = _currentWall;
        }
        else
        {
            _currentWall = _walls[Random.Range(0, _walls.Count)];
            if (_currentWall == _lastSpwnedWall)
            {
                ChooseRandomWall();
            }
            else
            {
                _currentWall = _walls[Random.Range(0, _walls.Count)];
                _lastSpwnedWall = _currentWall;
            }
        }
    }

    IEnumerator SpawnRandomWall()
    {
        ChooseRandomWall();
        ActivateWall();
        yield return new WaitForSeconds(_wallTimer);
        RemoveWall();
        yield return new WaitForSeconds(_spawnWaitTimer);
        StartCoroutine(SpawnRandomWall());
    }
}
