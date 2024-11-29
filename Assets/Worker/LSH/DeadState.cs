using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadState : MonoBehaviourPun, IState
{
    private UnitController _unitController;

    private UnitData _data;

    private Animator _animator;

    private int _hashDead;

    private float _setFalsTime;

    private float _curTime;


    public DeadState(UnitController controller)
    {
        //������
        _unitController = controller;

        _data = _unitController.UnitData;

        _animator = _unitController.GetComponent<Animator>();

        _hashDead = Animator.StringToHash("Death");

        _setFalsTime = _data.SetFalseTime;

    }


    public void OnEnter()
    {
        _curTime = 0;
        Debug.Log("Dead���� ����");
        PlayDeadAnimation();
        _unitController.Audio.PlayOneShot(_data.AudioCLips[(int)ESound.Dead]);
    }

    public void OnUpdate()
    {
        _curTime += Time.deltaTime;
        // �ִϸ��̼� ���� �� setfalse
        if(_curTime > _setFalsTime)
        {
            GameSceneManager.Instance.CurUnitCounts[(int)_data.UnitType]--;
            _unitController.gameObject.SetActive(false);
        }
    }

    public void OnExit()
    {
        Debug.Log("Dead���� Ż��");
        StopAni();
    }

    // �״� �ִϸ��̼� ���� �� false ���ֱ�
    // �״� �ִϸ��̼��� �ݺ��� �ʿ�����Ƿ� roof false ���ֱ�.
    private void  PlayDeadAnimation()
    {
        _animator.Play(_hashDead);
    }

    private void StopAni()
    {
        _animator.StopPlayback();
    }


}