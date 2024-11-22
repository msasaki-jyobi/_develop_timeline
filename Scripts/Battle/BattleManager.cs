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
        [Header("�Q�Ɛ�")]
        public TextMeshProUGUI MessageTextUGUI; // ���b�Z�[�W�p
        public developUnitComponents PlayerUnitComponents; // Player
        public developUnitComponents EnemyUnitComponents; // Enemy
        public Slider PlayerHealthSlider;
        public Slider EnemyHealthSlider;
        public Slider TimeSlider;
        // Health
        public ReactiveProperty<int> PlayerHealth = new ReactiveProperty<int>();
        public ReactiveProperty<int> EnemyHealth = new ReactiveProperty<int>();
        // Timer
        public float DefaultSpanTime = 3f;

        // �ǂݍ��ސ퓬�V�[��
        public List<BattleScene> BattleScenes = new List<BattleScene>();

        private bool _loadBattleEnemysData; // �o�g���f�[�^�ǂݍ��݊����t���O
        private BattleScene ActiveBattleScene; // ���ݍĐ����ɐ퓬�V�[��
        private developUnitComponents AttakerUnitComponents; // ���ݍU�����s���Ă��郆�j�b�g
        private developUnitComponents DamagerUnitComponents; // ���ݍU�����󂯂Ă��郆�j�b�g
        private List<BattleActionData> BattleActionDatas = new List<BattleActionData>(); // �ǂݍ��ރt���[���f�[�^

        public EKeyType SelectKey { get; private set; } // ���͂��ꂽ�L�[
        public int CommandNum { get; private set; } // �����p�F�����_���ɑI��A���l�ɉ������Z
        public EGameState OldGameState { get; private set; } // �O��̃X�e�[�g
        public ReactiveProperty<EGameState> GameState { get; private set; }
            = new ReactiveProperty<EGameState>(); // ����̃X�e�[t

        private ReactiveProperty<float> _timer = new ReactiveProperty<float>(); // �^�C�}�[
        private float _spanTime;

        private bool _isPlayerInputKeyContains; // Player Key Match Flg
        private bool _isDamagerKeyContains; // Damager Key Match

        [SerializeField] private bool IsDebugV; // Debug Test
        private int _currentDebugIndex = -1;

        private void Start()
        {
            // ����������
            ChangeState(EGameState.Init);
            _timer.Value = 0;
            PlayerHealth.Value = 100;
            EnemyHealth.Value = 100;
            _spanTime = DefaultSpanTime;

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

            // �X�e�[�g�ύX�Ɋւ��鏈��
            //StateControl();
        }

        private void Update()
        {
            _timer.Value += Time.deltaTime;

            BattleUpdate();

            // �L�[����Ɋւ��鏈��
            KeyControl();
        }

        private void StateControl()
        {
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
                            _isPlayerInputKeyContains = SelectKey == (EKeyType)ran;
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
                            //BattleUpdate();
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
        }

        private void KeyControl()
        {
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

            if (Input.GetKeyDown(KeyCode.V))
                if (IsDebugV)
                {
                    _currentDebugIndex++;
                    if (_currentDebugIndex == BattleScenes.Count)
                        _currentDebugIndex = 0;

                    LoadBattleScene(BattleScenes[_currentDebugIndex]);
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
                    check = check && sc.IsPlayerInputKeyContains == _isPlayerInputKeyContains; // ������Ԃ̈�v
                if (sc.IsCheckDamagerWin)
                    check = check && sc.IsDamgerKeyContains == _isDamagerKeyContains; // ������Ԃ̈�v
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

        private async void LoadBattleScene(BattleScene battle)
        {
            // State Change
            GameState.Value = battle.IsSetGameState ? battle.SetGameState : EGameState.Battle;
            PlayBattleScene(battle);

            // Delay
            Time.timeScale = battle.SetTimeScale;
            await UniTask.Delay((int)(battle.DelayTime * 1000), ignoreTimeScale: true);
            Time.timeScale = 1;
            _spanTime = battle.LoopTime;
        }

        /// <summary>
        /// �o�g���V�[�����Đ�����
        /// </summary>
        /// <param name="battleScene"></param>
        private void PlayBattleScene(BattleScene battleScene)
        {
            ActiveBattleScene = battleScene;
            AttakerUnitComponents = _isPlayerInputKeyContains ? PlayerUnitComponents : EnemyUnitComponents;
            DamagerUnitComponents = _isPlayerInputKeyContains ? EnemyUnitComponents : PlayerUnitComponents;

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
                            if (bAction.IsFlexibleAnimator)
                            {
                                if (bAction.IsFlexibleSetTarget)
                                {
                                    var target = DamagerUnitComponents.UnitInstance.SearchObject(
                                            bAction.FlexibleTargetBodyName
                                        ).transform;
                                    //bAction.FlexibleAnimator.Target = target;
                                    bAction.FlexibleAnimator.Root.transform.parent = target;
                                    bAction.FlexibleAnimator.Root.transform.localPosition = Vector3.zero;
                                }

                                bAction.FlexibleAnimator.Play();
                            }

                            // �v���n�u���� �G�t�F�N�g����Ȃ�����A��Ƃ�������c//
                            // ���ꂩ�A�P���ɃI�u�W�F�N�g�������āA2�b�����ċ߂Â��āA2�b�������烂�[�V�����Đ��Ƃ�
                            // �v���n�u���ɂ�点��p�^�[��
                            //GameObject enemyPrefab = null;
                            //if (bAction.SummonPrefab != null)
                            //    enemyPrefab = Instantiate(bAction.SummonPrefab);
                            //if (enemyPrefab != null)
                            //{
                            //    // �_��Body�ݒ�

                            //}

                            // �_���[�W���� //
                            if (!_isPlayerInputKeyContains)
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
                            if (bAction.IsAdditive)
                            {
                                // Additive���[�V���� �Đ�
                                DamagerUnitComponents.AnimatorStateController.AnimatorLayerPlay(1, bAction.AdditiveStateName, 0);

                            }

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
                _isDamagerKeyContains = 3 == (int)SelectKey;

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
