using Cinemachine;
using Cysharp.Threading.Tasks;
using develop_battle;
using develop_common;
using develop_easymovie;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

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

        public Slider PlayerHealthSlider;
        public Slider EnemyHealthSlider;

        public ReactiveProperty<int> PlayerHealth = new ReactiveProperty<int>()  ;
        public ReactiveProperty<int> EnemyHealth = new ReactiveProperty<int>()  ;

        public Slider TimeSlider;
        public float SpanTime = 3f;


        public List<BattleScene> BattleScenes = new List<BattleScene>();

        public EKeyType SelectKey { get; private set; }
        public int CommandNum { get; private set; } // 条件用：ランダムに選定、数値に応じた技
        public EGameState OldGameState { get; private set; }
        public ReactiveProperty<EGameState> GameState { get; private set; } = new ReactiveProperty<EGameState>();



        private ReactiveProperty<float> _timer = new ReactiveProperty<float>();
        private bool _playerWin;

        private void Start()
        {
            PlayerHealth.Value = 100;
            EnemyHealth.Value = 100;

            // ステートを切り替えた時
            GameState
                .Subscribe((x) =>
                {
                    switch (GameState.Value) // 現在のステート
                    {
                        case EGameState.None:
                            break;
                        case EGameState.Init:
                            break;
                        case EGameState.Wait:

                            break;
                        case EGameState.Check:
                            // Enemyの攻撃キーを確定する
                            int ran = UnityEngine.Random.Range(0, 4);
                            ran = 0;
                            // 同じキーを押せたか？
                            _playerWin = SelectKey == (EKeyType)ran;
                            // バトルシーンを再生する
                            LoadCheckBattleScenes();
                            break;
                        case EGameState.Clear:
                            break;
                        case EGameState.Miss:
                            break;
                    }
                });

            // タイマー変動中
            _timer
                .Subscribe((x) => { 

                    TimeSlider.value = _timer.Value / SpanTime;

                    if (_timer.Value > SpanTime * 0.7f)
                        Debug.Log("右だ！");

                    if (_timer.Value > SpanTime) // 時間に達したら
                    {
                        switch (GameState.Value) // 現在のステート
                        {
                            case EGameState.None:
                                break;
                            case EGameState.Init:
                                ChangeState(EGameState.Wait);
                                break;
                            case EGameState.Wait:
                                _timer.Value = 0;
                                ChangeState(EGameState.Check);
                                break;
                            case EGameState.Check:
                                break;
                            case EGameState.Clear:
                                _timer.Value = 0;
                                PlayerHealth.Value -= 5;

                                if (PlayerHealth.Value <= 80)
                                {
                                    ChangeState(EGameState.Init);
                                    _timer.Value = 0;
                                }
                                break;
                            case EGameState.Miss:
                                _timer.Value = 0;
                                EnemyHealth.Value -= 5;
                                if (EnemyHealth.Value <= 80)
                                {
                                    ChangeState(EGameState.Init);
                                    _timer.Value = 0;
                                }
                                break;
                        }

                    }
                });

            // Playerの体力が変更
            PlayerHealth
                .Subscribe((x) => {
                    PlayerHealthSlider.value = (float)PlayerHealth.Value / 100;
                });

            // 敵の体力が変更
            EnemyHealth
                .Subscribe((x) => {
                    EnemyHealthSlider.value = (float)EnemyHealth.Value / 100;
                });

            // 初期化処理
            ChangeState(EGameState.Init);
            _timer.Value = 0;
        }

        private void Update()
        {
            _timer.Value += Time.deltaTime;

            if (Input.GetKeyDown(KeyCode.W))
                SelectKey = EKeyType.Up;
            if (Input.GetKeyDown(KeyCode.A))
                SelectKey = EKeyType.Left;
            if (Input.GetKeyDown(KeyCode.S))
                SelectKey = EKeyType.Down;
            if (Input.GetKeyDown(KeyCode.D))
                SelectKey = EKeyType.Right;

            if (Input.GetKeyDown(KeyCode.X))
            {
                ChangeState(EGameState.Init);
                _timer.Value = 0;
            }
        }

        /// <summary>
        /// ステートを変更する
        /// </summary>
        /// <param name="state"></param>
        private void ChangeState(EGameState state)
        {
            // ステートを切り替える
            OldGameState = GameState.Value;
            GameState.Value = state;
        }
        /// <summary>
        /// 切り替えられるバトルシーンがあるかチェックする
        /// </summary>
        private async void LoadCheckBattleScenes()
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
                    GameState.Value = _playerWin ? EGameState.Clear : EGameState.Miss;
                    PlayBattleScene(sc);

                    Time.timeScale = sc.SetTimeScale;
                    await UniTask.Delay((int)(sc.DelayTime * 1000), ignoreTimeScale: true);
                    Time.timeScale = 1;
                    return;
                }

            }
        }
        /// <summary>
        /// バトルシーンを再生する
        /// </summary>
        /// <param name="battleScene"></param>
        private void PlayBattleScene(BattleScene battleScene)
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
            if (battleScene.ChangeVcam != null)
                develop_easymovie.CameraManager.Instance.ChangeActiveCamera(battleScene.ChangeVcam, battleScene.BrandTime);
            else if (battleScene.ChangeRandomVcams.Count > 0)
            {
                int ran = UnityEngine.Random.Range(0, battleScene.ChangeRandomVcams.Count);
                develop_easymovie.CameraManager.Instance.ChangeActiveCamera(battleScene.ChangeRandomVcams[ran], battleScene.BrandTime);
            }
        }
    }
}
