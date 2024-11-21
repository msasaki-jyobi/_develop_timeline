using Cinemachine;
using Cysharp.Threading.Tasks;
using develop_battle;
using develop_common;
using develop_easymovie;
using JetBrains.Annotations;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using developUnitComponents = develop_common.UnitComponents;


namespace develop_battle
{
    public enum EGameState
    {
        None,
        Init,
        Wait,
        Check,
        Battle,
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
        public TextMeshProUGUI MessageTextUGUI;
        public developUnitComponents PlayerUnitComponents;
        public developUnitComponents EnemyUnitComponents;

        public Slider PlayerHealthSlider;
        public Slider EnemyHealthSlider;

        public ReactiveProperty<int> PlayerHealth = new ReactiveProperty<int>();
        public ReactiveProperty<int> EnemyHealth = new ReactiveProperty<int>();

        public Slider TimeSlider;
        public float DefaultSpanTime = 3f;
        private float _spanTime;


        public List<BattleScene> BattleScenes = new List<BattleScene>();

        private bool _loadBattleEnemysData;
        private BattleScene ActiveBattleScene;
        private developUnitComponents AttakerUnitComponents;
        private developUnitComponents DamagerUnitComponents;
        private List<BattleActionData> BattleActionDatas = new List<BattleActionData>();


        public EKeyType SelectKey { get; private set; }
        public int CommandNum { get; private set; } // 条件用：ランダムに選定、数値に応じた技
        public EGameState OldGameState { get; private set; }
        public ReactiveProperty<EGameState> GameState { get; private set; } = new ReactiveProperty<EGameState>();



        private ReactiveProperty<float> _timer = new ReactiveProperty<float>();
        private bool _playerWin;
        private bool _damagerWin;

        private void Start()
        {
            PlayerHealth.Value = 100;
            EnemyHealth.Value = 100;
            _spanTime = DefaultSpanTime;

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
                        case EGameState.Battle:
                            break;
                    }
                });

            // タイマー変動中
            _timer
                .Subscribe((x) =>
                {
                    // タイムスライダー更新
                    TimeSlider.value = _timer.Value / _spanTime;

                    switch (GameState.Value) // 現在のステート
                    {
                        case EGameState.None:
                            break;
                        case EGameState.Init:
                            if (_timer.Value > _spanTime)
                                ChangeState(EGameState.Wait);
                            break;
                        case EGameState.Wait:
                            if (_timer.Value > _spanTime)
                            {
                                _timer.Value = 0;
                                ChangeState(EGameState.Check);
                            }
                            break;
                        case EGameState.Check:
                            break;
                        case EGameState.Battle:
                            BattleUpdate();
                            //_timer.Value = 0;
                            //PlayerHealth.Value -= 5;

                            //if (PlayerHealth.Value <= 80)
                            //{
                            //    ChangeState(EGameState.Init);
                            //    _timer.Value = 0;
                            //}
                            break;
                            //_timer.Value = 0;
                            //EnemyHealth.Value -= 5;
                            //if (EnemyHealth.Value <= 80)
                            //{
                            //    ChangeState(EGameState.Init);
                            //    _timer.Value = 0;
                            //}
                            break;
                    }
                });

            // Playerの体力が変更
            PlayerHealth
                .Subscribe((x) =>
                {
                    PlayerHealthSlider.value = (float)PlayerHealth.Value / 100;
                });

            // 敵の体力が変更
            EnemyHealth
                .Subscribe((x) =>
                {
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

                if (sc.IsCheckPlayerWin)
                    check = check && sc.PlayerWin == _playerWin; // 勝利状態の一致
                if(sc.IsCheckDamagerWin)
                    check = check && sc.DamagerWin == _damagerWin; // 勝利状態の一致
                if (sc.IsCheckOldgameState)
                    check = check && sc.OldGameState == OldGameState; //前のステートの一致
                if (sc.IsCheckSelectGameState)
                    check = check && sc.SelectGameState == GameState.Value; // 切り替えたステートの一致
                if (sc.IsCheckCommandNum)
                    check = check && sc.CommandNum == CommandNum;

                if (check)
                {
                    GameState.Value = sc.IsSetGameState ? sc.SetGameState : EGameState.Battle;
                    PlayBattleScene(sc);

                    Time.timeScale = sc.SetTimeScale;
                    await UniTask.Delay((int)(sc.DelayTime * 1000), ignoreTimeScale: true);
                    Time.timeScale = 1;
                    _spanTime = sc.LoopTime;
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
            ActiveBattleScene = battleScene;
            AttakerUnitComponents = _playerWin ? PlayerUnitComponents : EnemyUnitComponents;
            DamagerUnitComponents = _playerWin ? EnemyUnitComponents : PlayerUnitComponents;

            // モーション再生
            if (battleScene.IsDamagerChangeState)
                DamagerUnitComponents.AnimatorStateController.StatePlay(battleScene.ChangeDamagerStateName, EStatePlayType.SinglePlay, true, battleScene.IsDamagerApplyChange);
            if (battleScene.IsAttakerChangeState)
                AttakerUnitComponents.UnitActionLoader.LoadAction(battleScene.AttakerAction, ignoreRequirement: true);

            // 座標と回転を上書き
            if (battleScene.IsDamagerPos)
                DamagerUnitComponents.gameObject.transform.position = battleScene.TargetDamagerTransform.transform.position;
            if (battleScene.IsAttackerPos)
                AttakerUnitComponents.gameObject.transform.position = battleScene.TargetAttakerTransform.transform.position;

            if (battleScene.IsDamagerRot)
                DamagerUnitComponents.gameObject.transform.rotation = Quaternion.Euler(battleScene.TargetDamagerTransform.transform.rotation.eulerAngles);
            if (battleScene.IsAttackerRot)
                AttakerUnitComponents.gameObject.transform.rotation = Quaternion.Euler(battleScene.TargetAttakerTransform.transform.rotation.eulerAngles);

            // カメラ切り替え
            var lookTarget = battleScene.LookDamager ? DamagerUnitComponents.transform : null;
            if (battleScene.ChangeVcam != null)
                develop_easymovie.CameraManager.Instance.ChangeActiveCamera(battleScene.ChangeVcam, battleScene.BrandTime, lookTarget: lookTarget);
            else if (battleScene.ChangeRandomVcams.Count > 0)
            {
                int ran = UnityEngine.Random.Range(0, battleScene.ChangeRandomVcams.Count);
                develop_easymovie.CameraManager.Instance.ChangeActiveCamera(battleScene.ChangeRandomVcams[ran], battleScene.BrandTime, lookTarget: lookTarget);
            }
        }
        /// <summary>
        /// Clear or Miss の間呼ばれ続ける
        /// </summary>
        public void BattleUpdate()
        {
            if (ActiveBattleScene == null) return;
            if (DamagerUnitComponents == null) return;

            // ループ中のエネミー情報をセット
            if (!_loadBattleEnemysData)
            {
                _loadBattleEnemysData = true;
                SetBattleEnemys(ActiveBattleScene.EnemyDatas);
            }

            // エネミー処理
            if (BattleActionDatas.Count > 0)
            {
                foreach (var bAction in BattleActionDatas)
                {
                    if (_timer.Value > bAction.PlayTime) // 
                    {
                        if (!bAction.IsPlay)
                        {
                            bAction.IsPlay = true;
                            bAction.IsInitPlay = true; // 現状機能しない

                            // アニメーション
                            if(bAction.IsFlexibleAnimator)
                                bAction.FlexibleAnimator.Play();

                            // プレハブ生成 エフェクトじゃないから、手とかだから…//
                            // それか、単純にオブジェクト生成して、2秒かけて近づいて、2秒たったらモーション再生とか
                            // プレハブ側にやらせるパターン
                            GameObject enemyPrefab = null;
                            if (bAction.SummonPrefab != null)
                                enemyPrefab = Instantiate(bAction.SummonPrefab);
                            if (enemyPrefab != null)
                            {
                                // 狙うBody設定

                            }

                            // ダメージ発生 //
                            if (!_playerWin)
                                PlayerHealth.Value -= bAction.DamageAmount;
                            else
                                EnemyHealth.Value -= bAction.DamageAmount;

                            // エフェクト
                            var damageBody = DamagerUnitComponents.UnitInstance.SearchObject(bAction.DamageBodyName);
                            if (damageBody != null)
                            {
                                UtilityFunction.PlayEffect(damageBody, bAction.DamageEffectPrefab);
                            }
                            // 効果音
                            AudioManager.Instance.PlayOneShotClipData(bAction.DamageSE);
                            // ボイス
                            if (DamagerUnitComponents.UnitVoice != null)
                                DamagerUnitComponents.UnitVoice.PlayVoice(bAction.DamageVoice);
                            // 字幕
                            if (bAction.DamageMessage != "")
                            {
                                MessageTextUGUI.text = bAction.DamageMessage;
                            }
                            // Additiveモーション 再生
                            DamagerUnitComponents.AnimatorStateController.AnimatorLayerPlay(1, "Additive1", 0);

                            // カメラ切り替え
                            var lookTarget = bAction.LookDamager ? DamagerUnitComponents.transform : null;
                            if (bAction.ChangeVcam != null)
                                develop_easymovie.CameraManager.Instance.ChangeActiveCamera(bAction.ChangeVcam, bAction.BrandTime, lookTarget: lookTarget);
                            else if (bAction.ChangeRandomVcams.Count > 0)
                            {
                                int ran = UnityEngine.Random.Range(0, bAction.ChangeRandomVcams.Count);
                                develop_easymovie.CameraManager.Instance.ChangeActiveCamera(bAction.ChangeRandomVcams[ran], bAction.BrandTime, lookTarget: lookTarget);
                            }

                        }
                    }

                }
            }


            if (_timer.Value > _spanTime) // バトルゲージマックス時
            {
                // winチェック（脱出）
                _damagerWin = 3 == (int)SelectKey;

                _loadBattleEnemysData = false;
                _timer.Value = 0;
                MessageTextUGUI.text = "";

                LoadCheckBattleScenes();
            }
        }
        /// <summary>
        /// バトル状態をリセットする
        /// </summary>
        public void ClearBattleScene()
        {
            BattleActionDatas.Clear();
            MessageTextUGUI.text = "";
        }
        /// <summary>
        /// 読み込みリストに複製する
        /// </summary>
        /// <param name="enemyDatas"></param>
        public void SetBattleEnemys(List<BattleActionData> enemyDatas)
        {
            BattleActionDatas.Clear();
            foreach (var data in enemyDatas)
            {
                var list = new BattleActionData(data);
                BattleActionDatas.Add(list);
            }
        }
    }
}
