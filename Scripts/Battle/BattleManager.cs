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
        public int CommandNum { get; private set; } // 条件用：ランダムに選定、数値に応じた技
        public EGameState OldGameState { get; private set; }
        public ReactiveProperty<EGameState> GameState { get; private set; } = new ReactiveProperty<EGameState>();



        private float _timer;
        private bool _playerWin;

        private void Start()
        {
            GameState
                .Subscribe((x) =>
                {
                    // ステートを切り替えた時
                    switch (GameState.Value) // 現在のステート
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

                            _playerWin = SelectKey == (EKeyType)ran; // キーが一致するか？
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
            if (_timer > SpanTime) // 時間に達したら
            {
                switch (GameState.Value) // 現在のステート
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
            // ステートを切り替える
            OldGameState = GameState.Value;
            GameState.Value = state;

            // 条件チェック
        }

        private void CheckBattleScene()
        {
            foreach (var sc in BattleScenes)
            {
                bool check = true;

                check = check && sc.PlayerWin == _playerWin; // 勝利状態の一致
                if (sc.IsCheckOldgameState)
                    check = check && sc.OldGameState == OldGameState; //前のステートの一致
                if (sc.IsCheckSelectGameState)
                    check = check && sc.SelectGameState == GameState.Value; // 切り替えたステートの一致
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
            // モーション再生
            if (battleScene.IsPlayerChangeState)
                PlayerAnimatorState.StatePlay(battleScene.ChangePlayerStateName, EStatePlayType.SinglePlay, true, battleScene.IsPlayerApplyChange);
            if (battleScene.IsEnemyChangeState)
                EnemyAnimatorState.StatePlay(battleScene.ChangeEnemyStateName, EStatePlayType.SinglePlay, true, battleScene.IsEnemyApplyChange);
            // 座標と回転を上書き
            if (battleScene.IsPlayerPos)
                PlayerAnimatorState.gameObject.transform.position = battleScene.TargetPlayerTransform.transform.position;
            if (battleScene.IsEnemyPos)
                EnemyAnimatorState.gameObject.transform.position = battleScene.TargetEnemyTransform.transform.position;

            if (battleScene.IsPlayerRot)
                PlayerAnimatorState.gameObject.transform.rotation = Quaternion.Euler(battleScene.TargetPlayerTransform.transform.rotation.eulerAngles);
            if (battleScene.IsEnemyRot)
                EnemyAnimatorState.gameObject.transform.rotation = Quaternion.Euler(battleScene.TargetEnemyTransform.transform.rotation.eulerAngles);

            // カメラ切り替え
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
