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
        public int CommandNum { get; private set; } // �����p�F�����_���ɑI��A���l�ɉ������Z
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

            // �X�e�[�g��؂�ւ�����
            GameState
                .Subscribe((x) =>
                {
                    switch (GameState.Value) // ���݂̃X�e�[�g
                    {
                        case EGameState.None:
                            break;
                        case EGameState.Init:
                            break;
                        case EGameState.Wait:

                            break;
                        case EGameState.Check:
                            // Enemy�̍U���L�[���m�肷��
                            int ran = UnityEngine.Random.Range(0, 4);
                            ran = 0;
                            // �����L�[�����������H
                            _playerWin = SelectKey == (EKeyType)ran;
                            // �o�g���V�[�����Đ�����
                            LoadCheckBattleScenes();
                            break;
                        case EGameState.Battle:
                            break;
                    }
                });

            // �^�C�}�[�ϓ���
            _timer
                .Subscribe((x) =>
                {
                    // �^�C���X���C�_�[�X�V
                    TimeSlider.value = _timer.Value / _spanTime;

                    switch (GameState.Value) // ���݂̃X�e�[�g
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

            // Player�̗̑͂��ύX
            PlayerHealth
                .Subscribe((x) =>
                {
                    PlayerHealthSlider.value = (float)PlayerHealth.Value / 100;
                });

            // �G�̗̑͂��ύX
            EnemyHealth
                .Subscribe((x) =>
                {
                    EnemyHealthSlider.value = (float)EnemyHealth.Value / 100;
                });

            // ����������
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
        /// �X�e�[�g��ύX����
        /// </summary>
        /// <param name="state"></param>
        private void ChangeState(EGameState state)
        {
            // �X�e�[�g��؂�ւ���
            OldGameState = GameState.Value;
            GameState.Value = state;
        }
        /// <summary>
        /// �؂�ւ�����o�g���V�[�������邩�`�F�b�N����
        /// </summary>
        private async void LoadCheckBattleScenes()
        {
            foreach (var sc in BattleScenes)
            {
                bool check = true;

                if (sc.IsCheckPlayerWin)
                    check = check && sc.PlayerWin == _playerWin; // ������Ԃ̈�v
                if(sc.IsCheckDamagerWin)
                    check = check && sc.DamagerWin == _damagerWin; // ������Ԃ̈�v
                if (sc.IsCheckOldgameState)
                    check = check && sc.OldGameState == OldGameState; //�O�̃X�e�[�g�̈�v
                if (sc.IsCheckSelectGameState)
                    check = check && sc.SelectGameState == GameState.Value; // �؂�ւ����X�e�[�g�̈�v
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
        /// �o�g���V�[�����Đ�����
        /// </summary>
        /// <param name="battleScene"></param>
        private void PlayBattleScene(BattleScene battleScene)
        {
            ActiveBattleScene = battleScene;
            AttakerUnitComponents = _playerWin ? PlayerUnitComponents : EnemyUnitComponents;
            DamagerUnitComponents = _playerWin ? EnemyUnitComponents : PlayerUnitComponents;

            // ���[�V�����Đ�
            if (battleScene.IsDamagerChangeState)
                DamagerUnitComponents.AnimatorStateController.StatePlay(battleScene.ChangeDamagerStateName, EStatePlayType.SinglePlay, true, battleScene.IsDamagerApplyChange);
            if (battleScene.IsAttakerChangeState)
                AttakerUnitComponents.UnitActionLoader.LoadAction(battleScene.AttakerAction, ignoreRequirement: true);

            // ���W�Ɖ�]���㏑��
            if (battleScene.IsDamagerPos)
                DamagerUnitComponents.gameObject.transform.position = battleScene.TargetDamagerTransform.transform.position;
            if (battleScene.IsAttackerPos)
                AttakerUnitComponents.gameObject.transform.position = battleScene.TargetAttakerTransform.transform.position;

            if (battleScene.IsDamagerRot)
                DamagerUnitComponents.gameObject.transform.rotation = Quaternion.Euler(battleScene.TargetDamagerTransform.transform.rotation.eulerAngles);
            if (battleScene.IsAttackerRot)
                AttakerUnitComponents.gameObject.transform.rotation = Quaternion.Euler(battleScene.TargetAttakerTransform.transform.rotation.eulerAngles);

            // �J�����؂�ւ�
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
        /// Clear or Miss �̊ԌĂ΂ꑱ����
        /// </summary>
        public void BattleUpdate()
        {
            if (ActiveBattleScene == null) return;
            if (DamagerUnitComponents == null) return;

            // ���[�v���̃G�l�~�[�����Z�b�g
            if (!_loadBattleEnemysData)
            {
                _loadBattleEnemysData = true;
                SetBattleEnemys(ActiveBattleScene.EnemyDatas);
            }

            // �G�l�~�[����
            if (BattleActionDatas.Count > 0)
            {
                foreach (var bAction in BattleActionDatas)
                {
                    if (_timer.Value > bAction.PlayTime) // 
                    {
                        if (!bAction.IsPlay)
                        {
                            bAction.IsPlay = true;
                            bAction.IsInitPlay = true; // ����@�\���Ȃ�

                            // �A�j���[�V����
                            if(bAction.IsFlexibleAnimator)
                                bAction.FlexibleAnimator.Play();

                            // �v���n�u���� �G�t�F�N�g����Ȃ�����A��Ƃ�������c//
                            // ���ꂩ�A�P���ɃI�u�W�F�N�g�������āA2�b�����ċ߂Â��āA2�b�������烂�[�V�����Đ��Ƃ�
                            // �v���n�u���ɂ�点��p�^�[��
                            GameObject enemyPrefab = null;
                            if (bAction.SummonPrefab != null)
                                enemyPrefab = Instantiate(bAction.SummonPrefab);
                            if (enemyPrefab != null)
                            {
                                // �_��Body�ݒ�

                            }

                            // �_���[�W���� //
                            if (!_playerWin)
                                PlayerHealth.Value -= bAction.DamageAmount;
                            else
                                EnemyHealth.Value -= bAction.DamageAmount;

                            // �G�t�F�N�g
                            var damageBody = DamagerUnitComponents.UnitInstance.SearchObject(bAction.DamageBodyName);
                            if (damageBody != null)
                            {
                                UtilityFunction.PlayEffect(damageBody, bAction.DamageEffectPrefab);
                            }
                            // ���ʉ�
                            AudioManager.Instance.PlayOneShotClipData(bAction.DamageSE);
                            // �{�C�X
                            if (DamagerUnitComponents.UnitVoice != null)
                                DamagerUnitComponents.UnitVoice.PlayVoice(bAction.DamageVoice);
                            // ����
                            if (bAction.DamageMessage != "")
                            {
                                MessageTextUGUI.text = bAction.DamageMessage;
                            }
                            // Additive���[�V���� �Đ�
                            DamagerUnitComponents.AnimatorStateController.AnimatorLayerPlay(1, "Additive1", 0);

                            // �J�����؂�ւ�
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


            if (_timer.Value > _spanTime) // �o�g���Q�[�W�}�b�N�X��
            {
                // win�`�F�b�N�i�E�o�j
                _damagerWin = 3 == (int)SelectKey;

                _loadBattleEnemysData = false;
                _timer.Value = 0;
                MessageTextUGUI.text = "";

                LoadCheckBattleScenes();
            }
        }
        /// <summary>
        /// �o�g����Ԃ����Z�b�g����
        /// </summary>
        public void ClearBattleScene()
        {
            BattleActionDatas.Clear();
            MessageTextUGUI.text = "";
        }
        /// <summary>
        /// �ǂݍ��݃��X�g�ɕ�������
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
