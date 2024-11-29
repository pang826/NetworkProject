using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public enum EStates { Idle, Walk, Attack, Dead, Size}
public class UnitController : MonoBehaviourPun, IDamageable
{

    [SerializeField] private AStar _aStar;
    public AStar AStar { get { return _aStar; } set { _aStar = value; } }

    [SerializeField] private UnitData _unitData;
    public UnitData UnitData { get { return _unitData; } set { } }

    private IState _currentState;

    private IState[] _states = new IState[(int)EStates.Size];
    public IState[] States { get { return _states; } set { } }

    [SerializeField] private AudioSource _audio;

    public AudioSource Audio { get { return _audio; } private set { } }


    private void Awake()
    {
        _states[(int)EStates.Idle] = new IdleState(this);
        _states[(int)EStates.Walk] = new WalkState(this);
        _states[(int)EStates.Attack] = new AttackState(this);
        _states[(int)EStates.Dead] = new DeadState(this);
       
    }
    private void OnEnable()
    {
        //�װ� Pull���� �ٽ� �����ɶ�, Data Reset���ֱ�
        ResetData();
        photonView.RPC("SetTrue", RpcTarget.All);
        photonView.RPC("ChangeState", RpcTarget.All, (int)EStates.Idle);
    }

    private void Update()
    {
        // AttackTarget�� ������ �ִ� ���¿��� �� Attack Target�� �׾��ٸ� AttackTarget �ʱ�ȭ.
        if (_unitData.AttackTarget != null && _unitData.AttackTarget.activeSelf == false)
        {
            _unitData.AttackTarget = null;
        }

        // ���� ������ ���� üũ
        _unitData.HitColiders = Physics2D.OverlapCircleAll(this.transform.position, _unitData.HitRadius);

        // Length == 1 ��, ������ ������� x. Player�� ���������� ���ݴ���� �������� ���. �������ݴ���� null ��
        if (_unitData.HitColiders.Length == 1)
        {
            _unitData.HitObject = null;
        } 
        else //  Hit�Ҽ� �ִ� ������ ���� ���
        {
            // ���Unit�� ���� ���� ����, �̷� ��� �������� ��� ���ؼ� �����ϱ�
            for (int i = 0; i < _unitData.HitColiders.Length; i++)
            {
                UnitData otherUnit = _unitData.HitColiders[i].GetComponent<UnitData>();
                PlayerData otherPlayer = _unitData.HitColiders[i].GetComponent<PlayerData>();
                // �ڱ� �ڽ��̳� ��ֹ�, �Ʊ� ���� �� Player��  Hit������� x,
                if (_unitData.HitColiders[i].gameObject != gameObject && _unitData.HitColiders[i].tag != "Obstacle" && ((otherUnit != null && otherUnit.UnitType != _unitData.UnitType) ||( otherPlayer != null && otherPlayer.UnitType != _unitData.UnitType)))
                {
                    _unitData.HitObject = _unitData.HitColiders[i];
                    // ���� ���ݴ���� ������ ���, ���ݴ���� HitObject�� ����.  -> ���� ������ AttackTarget�� HitColiders�� ���, AttackTarget�� ������ ���� �߻�.
                    if (_unitData.AttackTarget != null && Array.Exists(_unitData.HitColiders, c => c.gameObject == _unitData.AttackTarget))
                    {
                        _unitData.HitObject = _unitData.AttackTarget.GetComponent<Collider2D>();
                    }
                    break;
                }
                else
                {
                    _unitData.HitObject = null;
                }

            }
        }

        _currentState?.OnUpdate();
    }

    private void ResetData()
    {
        _unitData.HP = 100;
        _unitData.Path.Clear();
        _unitData.PathIndex = 0;
        _unitData.HitObject = null;
        _unitData.AttackTarget = null;
        _unitData.HasReceivedMove = false;
        
    }

    [PunRPC]
    public void SetTrue()
    {
        gameObject.SetActive(true);
    }

    [PunRPC]
    public void ChangeState(int newStateIndex)
    {
        if (_currentState != null)
        {
            _currentState.OnExit();
        }

        _currentState = _states[newStateIndex];
        _currentState.OnEnter();

    }

    public void GetDamage(int damage)
    {
        _unitData.HP -= damage;
    }

}