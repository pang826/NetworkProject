using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class WalkState : MonoBehaviour, IState
{
    private UnitController _unitController;

    private UnitData _data;

    private AStar _aStar;

    private Vector3 _currentAttackTarget;

    private float _checkAttackTargetTime;

    public WalkState(UnitController controller)
    {
        //생성자
        _unitController = controller;
        _data = _unitController.UnitData;
        _aStar = _unitController.AStar;
    }


    public void OnEnter()
    {
        Debug.Log("Walk상태 진입");

        // 경로 재탐색을 위한 초기화.
        if (_data.AttackTarget != null)
        {
            _currentAttackTarget = _data.AttackTarget.transform.position;
        }
        _checkAttackTargetTime = 0;
    }

    //순서 바꾸지 말기!
    public void OnUpdate()
    {
        _checkAttackTargetTime += Time.deltaTime;

        // 이동 마지막 경로까지 이동을 끝냈다면, HasReceivedMove false.
        if (_data.PathIndex == _data.Path.Count)
        {
            _data.HasReceivedMove = false;
        }

        //상태전환 
        if (_data.HP <= 0)
        {
            _unitController.ChangeState(_unitController.States[(int)EStates.Dead]);
        }

        if (_data.HasReceivedMove == false)
        {
            if (_data.Path.Count == _data.PathIndex && _data.AttackTarget == null)
            {
                _unitController.ChangeState(_unitController.States[(int)EStates.Idle]);
            }
            if (_data.HitObject != null)
            {
                _unitController.ChangeState(_unitController.States[(int)EStates.Attack]);
            }
        }

        // 재탐색
        if (_data.AttackTarget != null && (_data.AttackTarget.transform.position != _currentAttackTarget && _checkAttackTargetTime > _data.FindLoadTime))
        {
            ReSearchPath();
        }

        // 이동
        if(_data.PathIndex < _data.Path.Count)
        {
            DoWalk(_data.Path[_data.PathIndex]);
        }

    }

    public void OnExit()
    {
        Debug.Log("Walk상태 탈출");
    }

    // 경로 재탐색
    public void ReSearchPath()
    {
        Debug.Log("경로 변환!!!");
        _currentAttackTarget = _data.AttackTarget.transform.position;
        Vector2Int startPos = new Vector2Int((int)_unitController.transform.position.x, (int)_unitController.transform.position.y);
        Vector2Int endPos = new Vector2Int((int)_data.AttackTarget.transform.position.x, (int)_data.AttackTarget.transform.position.y);
        _aStar.DoAStar(startPos, endPos);
        _data.Path.Clear();
        _data.PathIndex = 0;
        _data.Path = _aStar.Path;
        _checkAttackTargetTime = 0;
    }

    public void DoWalk(Vector2Int pathPoint)
    {
        Debug.Log("Walk중!");
        // 현재 위치에서 목표 지점으로 이동
        _unitController.transform.position = Vector2.MoveTowards(_unitController.transform.position, pathPoint, _data.MoveSpeed * Time.deltaTime);

        // 목표 지점에 도달했는지 확인
        if ((Vector2)_unitController.transform.position == pathPoint)
        {
            _data.PathIndex++;  // 다음 지점으로 이동
        }

    }


}
