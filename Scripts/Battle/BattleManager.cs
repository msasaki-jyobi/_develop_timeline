using Cinemachine;
using develop_battle;
using develop_common;
using develop_easymovie;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace develop_battle
{
    public enum EGameState
    {
        None,
        Init,
        Wait,
        Check,
        Clear,
        Miss,
    }
    public enum EKeyType
    {
        Down,
        Left,
        Up,
        Right,
        Fire1,
        Fire2,
    }

    public class BattleManager : MonoBehaviour
    {
        public AnimatorStateController PlayerAnimatorState;
        public AnimatorStateController EnemyAnimatorState;

        public List<BattleScene> BattleScenes = new List<BattleScene>();

        public float SpanTime = 3f;

        public EKeyType SelectKey { get; private set; }
        public int CommandNum { get; private set; } // �����p�F�����_���ɑI��A���l�ɉ������Z
        public EGameState OldGameState { get; private set; }
        public ReactiveProperty<EGameState> GameState { get; private set; } = new ReactiveProperty<EGameState>();



        private float _timer;
        private bool _playerWin;

        private void Start()
        {
            GameState
                .Subscribe((x) =>
                {
                    // �X�e�[�g��؂�ւ�����
                    switch (GameState.Value) // ���݂̃X�e�[�g
                    {
                        case EGameState.None:
                            break;
                        case EGameState.Init:
                            break;
                        case EGameState.Wait:

                            break;
                        case EGameState.Check:
                            int ran = UnityEngine.Random.Range(0, 4);
                            ran = 0;

                            _playerWin = SelectKey == (EKeyType)ran; // �L�[����v���邩�H
                            CheckBattleScene();
                            break;
                        case EGameState.Clear:
                            break;
                        case EGameState.Miss:
                            break;
                    }
                });




            ChangeState(EGameState.Init);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.W))
                SelectKey = EKeyType.Up;
            if (Input.GetKeyDown(KeyCode.A))
                SelectKey = EKeyType.Left;
            if (Input.GetKeyDown(KeyCode.S))
                SelectKey = EKeyType.Right;
            if (Input.GetKeyDown(KeyCode.D))
                SelectKey = EKeyType.Down;

            if (Input.GetKeyDown(KeyCode.X))
            {
                ChangeState(EGameState.Init);
                _timer = 0;
            }

            _timer += Time.deltaTime;
            if (_timer > SpanTime) // ���ԂɒB������
            {
                switch (GameState.Value) // ���݂̃X�e�[�g
                {
                    case EGameState.None:
                        break;
                    case EGameState.Init:
                        ChangeState(EGameState.Wait);
                        break;
                    case EGameState.Wait:
                        _timer = 0;
                        ChangeState(EGameState.Check);
                        break;
                    case EGameState.Check:
                        break;
                    case EGameState.Clear:
                        break;
                    case EGameState.Miss:
                        break;
                }

            }
        }

        private void ChangeState(EGameState state)
        {
            // �X�e�[�g��؂�ւ���
            OldGameState = GameState.Value;
            GameState.Value = state;

            // �����`�F�b�N
        }

        private void CheckBattleScene()
        {
            foreach (var sc in BattleScenes)
            {
                bool check = true;

                check = check && sc.PlayerWin == _playerWin; // ������Ԃ̈�v
                if (sc.IsCheckOldgameState)
                    check = check && sc.OldGameState == OldGameState; //�O�̃X�e�[�g�̈�v
                if (sc.IsCheckSelectGameState)
                    check = check && sc.SelectGameState == GameState.Value; // �؂�ւ����X�e�[�g�̈�v
                if (sc.IsCheckCommandNum)
                    check = check && sc.CommandNum == CommandNum;

                if (check)
                {
                    LoadBattleScene(sc);
                    return;
                }

            }
        }

        private void LoadBattleScene(BattleScene battleScene)
        {
            // ���[�V�����Đ�
            if (battleScene.IsPlayerChangeState)
                PlayerAnimatorState.StatePlay(battleScene.ChangePlayerStateName, EStatePlayType.SinglePlay, true, battleScene.IsPlayerApplyChange);
            if (battleScene.IsEnemyChangeState)
                EnemyAnimatorState.StatePlay(battleScene.ChangeEnemyStateName, EStatePlayType.SinglePlay, true, battleScene.IsEnemyApplyChange);
            // ���W�Ɖ�]���㏑��
            if (battleScene.IsPlayerPos)
                PlayerAnimatorState.gameObject.transform.position = battleScene.TargetPlayerTransform.transform.position;
            if (battleScene.IsEnemyPos)
                EnemyAnimatorState.gameObject.transform.position = battleScene.TargetEnemyTransform.transform.position;

            if (battleScene.IsPlayerRot)
                PlayerAnimatorState.gameObject.transform.rotation = Quaternion.Euler(battleScene.TargetPlayerTransform.transform.rotation.eulerAngles);
            if (battleScene.IsEnemyRot)
                EnemyAnimatorState.gameObject.transform.rotation = Quaternion.Euler(battleScene.TargetEnemyTransform.transform.rotation.eulerAngles);

            // �J�����؂�ւ�
            if(battleScene.ChangeRandomVcams.Count > 0)
            {
                int ran = UnityEngine.Random.Range(0, battleScene.ChangeRandomVcams.Count);
                CameraManager.Instance.ChangeActiveCamera(battleScene.ChangeRandomVcams[ran]);
            }
            else if(battleScene.ChangeVcam != null)
                CameraManager.Instance.ChangeActiveCamera(battleScene.ChangeVcam);
        }
    }
}
