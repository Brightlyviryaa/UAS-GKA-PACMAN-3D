using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TEST_MazeBlock : MonoBehaviour
{
    [SerializeField]
    private GameObject _leftWall;

    [SerializeField]
    private GameObject _rightWall;

    [SerializeField]
    private GameObject _frontWall;

    [SerializeField]
    private GameObject _rearWall;

    [SerializeField]
    private GameObject _centerFill;

    public void SetAllWalls()
    {
        _leftWall.SetActive(true);
        _rightWall.SetActive(true);
        _frontWall.SetActive(true);
        _rearWall.SetActive(true);
    }

    public void ClearAllWalls()
    {
        _leftWall.SetActive(false);
        _rightWall.SetActive(false);
        _frontWall.SetActive(false);
        _rearWall.SetActive(false);
    }

    public void ClearRandomWall()
    {
        int randomWall = Random.Range(0, 4);
        switch (randomWall)
        {
            case 0: _leftWall.SetActive(false); break;
            case 1: _rightWall.SetActive(false); break;
            case 2: _frontWall.SetActive(false); break;
            case 3: _rearWall.SetActive(false); break;
        }
    }

    public void ClearFrontWall() => _frontWall.SetActive(false);
    public void ClearRearWall() => _rearWall.SetActive(false);
    public void ClearLeftWall() => _leftWall.SetActive(false);
    public void ClearRightWall() => _rightWall.SetActive(false);
    public void ClearCenterFill() => _centerFill.SetActive(false);

    public void SetFrontWall()
    {
        _frontWall.SetActive(true);
    }

    public void SetRearWall()
    {
        _rearWall.SetActive(true);
    }

    public void SetLeftWall()
    {
        _leftWall.SetActive(true);
    }

    public void SetRightWall()
    {
        _rightWall.SetActive(true);
    }

    public void SetCenterFill()
    {
        _centerFill.SetActive(true);
    }

    public bool HasFrontWall() => _frontWall.activeSelf;
    public bool HasRearWall() => _rearWall.activeSelf;
    public bool HasLeftWall() => _leftWall.activeSelf;
    public bool HasRightWall() => _rightWall.activeSelf;
    public bool HasCenterFill() => _centerFill.activeSelf;
}

