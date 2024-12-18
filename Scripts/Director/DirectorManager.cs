using Cinemachine;
using Cysharp.Threading.Tasks;
using develop_common;
using develop_easymovie;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UIElements;

namespace develop_timeline
{
    public class DirectorManager : SingletonMonoBehaviour<DirectorManager>
    {


        public TextMeshProUGUI TimelineTextGUI;
        public CinemachineBrain Brain;

        public TextFadeController TextFadeController;

        [Header("�Đ����AUnit��private�ϐ��ɏ㏑��")]
        public bool UnitOverwrite_UnitComponents;
        [Header("�Đ����APosition�Ń��b�v����")]
        public bool IsPlayPositionParent; // �Đ����Ɏw�肳�ꂽPosition�̎q�I�u�W�F�N�g�ɂ��āA�I�����Ɍ��ɖ߂�
        [Header("�Đ��I������Position���������A���̈ʒu�ɖ߂�")]
        public bool IsFinishResetPos; // �Đ��� �e�I�u�W�F�N�g�̍��W���L�^���� �I������PositionA����O���Č��ɖ߂�
        [Header("Positions���Đ����鎞��UnitB�ɓ�������")]
        public bool IsPlayPositionsUnitBSync;
        public Transform DefaultPositionsParent;
        private Vector3 _unitAPlayPosition;
        private Vector3 _unitBPlayPosition;


        [Header("������������")]
        [Header("�v���n�u�𐶐����AA,B�Ɏ������蓖��")]
        public bool IsCreatePrefab;
        public GameObject UnitAPrefab;
        public GameObject UnitBPrefab;
        //[Header("B�F�������蓖�Ă��Ȃ��ꍇ�́A�蓮�ŃA�^�b�`")]
        //private Animator _unitA;
        //private Animator _unitB;
        [Header("�Q�ƕK�{�i���������Ȃ炩���OK�j")]
        public develop_common.UnitComponents UnitAComponents;
        public develop_common.UnitComponents UnitBComponents;


        [Header("������������")]
        [Header("���O�ɃA�^�b�`�A")]
        public GameObject Positions;
        public Animator PositionA;
        public Animator PositionB;

        [Header("���ݍĐ����s���Ă���PlayableDirector")]
        public PlayableDirector ActivePlayingDirector;

        public event Action<string, string> StartEvent;
        public event Action<EPlayableParamater, string, bool> UpdatePlayableEvent;
        public event Action<string, string> FinishNamedEvent;
        public event Action FinishEvent;

        public int ThirdCount; // �R�񃋁[�v�p


        [Header("FadeController�𗘗p����NormalPlay�p")]
        public float NormalFadeTime = 0.5f;
        public Color NormalPlayFadeColor;



        private async void Start()
        {
            if (IsCreatePrefab)
            {
                var unitA = Instantiate(UnitAPrefab);
                var unitB = Instantiate(UnitBPrefab);
                unitA.gameObject.name = UnitAPrefab.name;
                unitB.gameObject.name = UnitBPrefab.name;

                ChangeUnitA(unitA); // UnitComponents�Ƀv���n�u���A�^�b�`
                ChangeUnitB(unitB); // UnitComponents�Ƀv���n�u���A�^�b�`
            }
        }

        public bool IsCheckPlaying()
        {
            return ActivePlayingDirector != null;
        }
        /// <summary>
        /// �ݒ肳��Ă���UnitA,UnitB�����Ƀ��[�V���������s
        /// </summary>
        /// <param name="director"></param>
        public async void NormalPlayDirector(PlayableDirector director)
        {
            await UniTask.WaitUntil(() => UnitAComponents != null);
            await UniTask.WaitUntil(() => UnitBComponents != null);



            if (IsPlayPositionsUnitBSync)
            {
                Positions.transform.position = UnitBComponents.transform.position;
            }

            if (UnitAComponents.TryGetComponent<Rigidbody>(out var rigid))
                rigid.isKinematic = true;
            if (UnitBComponents.TryGetComponent<Rigidbody>(out var rigid2))
                rigid2.isKinematic = true;

            ThirdCount = 0; // 3��J�E���^�[���Z�b�g
            if (ActivePlayingDirector != null) // ���ݍĐ����̂͒�~
            {
                ActivePlayingDirector.Pause();
                ActivePlayingDirector.Stop();
            }
            await UniTask.Delay(1);

            // �Đ������s
            ActivePlayingDirector = director;

            if (NormalFadeTime <= 0)
                SetDirectors(director, UnitAComponents, UnitBComponents, PositionA, PositionB, Brain, true);
            //director.Play();
            else
                FadeController.Instance.ActionPlayFadeIn(() =>
                {
                    SetDirectors(director, UnitAComponents, UnitBComponents, PositionA, PositionB, Brain, true);
                    //director.Play();
                }, NormalFadeTime, NormalFadeTime, NormalPlayFadeColor);
        }

        public bool SetPlayDirector(PlayableDirector director)
        {
            bool isCheck = true;

            if (ActivePlayingDirector != null)
            {
                Debug.LogError($"����${ActivePlayingDirector.playableAsset} �����s���ł���");
                isCheck = false;
            }

            ActivePlayingDirector = director;

            // DirectorPlayer���A�^�b�`����Ă����
            if (ActivePlayingDirector.gameObject.TryGetComponent<DirectorPlayer>(out var directorPlayer))
                foreach (var finishEvent in directorPlayer.FinishEventHandles)
                    StartEvent?.Invoke(finishEvent.EventName, finishEvent.EventValue); // Event Play

            return isCheck;
        }

        public void OnSetTimelineMessage(string message)
        {
            if (TextFadeController != null)
                TextFadeController.UpdateMessageText(message);
            else if (TimelineTextGUI != null)
                TimelineTextGUI.text = message;
        }

        public void FinishPlayable()
        {
            if (ActivePlayingDirector != null)
            {
                // �Đ������s����Director�ɁuDirectorPlayer�v�����݂���ꍇ
                if (ActivePlayingDirector.gameObject.TryGetComponent<DirectorPlayer>(out var directorPlayer))
                {
                    if (UnitAComponents != null)
                    {
                        if (UnitAComponents.TryGetComponent<Rigidbody>(out var rigidbody))
                            rigidbody.isKinematic = false;
                        UnitAComponents.transform.parent = null;
                        UnitAComponents = null;
                    }
                    if (UnitBComponents != null)
                    {
                        if (UnitBComponents.TryGetComponent<Rigidbody>(out var rigidbody))
                            rigidbody.isKinematic = false;
                        UnitBComponents.transform.parent = null;
                        UnitBComponents = null;
                    }
                    directorPlayer.OnPlayFinish();
                    Destroy(ActivePlayingDirector.gameObject);

                    foreach (var finishEvent in directorPlayer.FinishEventHandles)
                        FinishNamedEvent?.Invoke(finishEvent.EventName, finishEvent.EventValue);

                }
                if (IsFinishResetPos)
                {
                    UnitAComponents.transform.parent = null;
                    UnitAComponents.transform.position = _unitAPlayPosition;
                    UnitBComponents.transform.parent = null;
                    UnitBComponents.transform.position = _unitBPlayPosition;
                }

                if (UnitAComponents.TryGetComponent<Rigidbody>(out var rigid))
                    rigid.isKinematic = false;

                if (UnitBComponents.TryGetComponent<Rigidbody>(out var rigid2))
                    rigid2.isKinematic = false;

                if (IsPlayPositionsUnitBSync)
                {
                    Positions.transform.localPosition = Vector3.zero;
                }


                // �I������
                ActivePlayingDirector = null;
                TimelineTextGUI.text = "";
                FinishNamedEvent?.Invoke("", "");
                FinishEvent?.Invoke();
            }
            else
            {
                Debug.LogError("���s����Director�͂���܂���");
            }
        }
        /// <summary>
        /// Playble:Update Event
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="value"></param>
        public void UpdatePlayableEventInvoke(EPlayableParamater eventName, string value, bool unitA = true)
        {
            UpdatePlayableEvent?.Invoke(eventName, value, unitA);
            Debug.Log($"Update!! name:{eventName}, value:{value}");
        }

        public async void SetDirectors(PlayableDirector director, develop_common.UnitComponents unitA, develop_common.UnitComponents unitB, Animator positionA, Animator positionB, CinemachineBrain brain, bool isPlay = false)
        {
            var unitATrackName = "UnitA";
            var unitBTrackName = "UnitB";
            var posATrackName = "PosA";
            var posBTrackName = "PosB";


            // Bind Pos
            if (positionA != null)
                BindAnimatorTrackDirector(director, director.playableAsset, posATrackName, positionA);
            if (positionB != null)
                BindAnimatorTrackDirector(director, director.playableAsset, posBTrackName, positionB);

            // UnitA
            if (unitA != null)
            {
                _unitAPlayPosition = unitA.transform.position;



                if (positionA != null)
                {
                    // offset�ǂ�����H�L�����傫���Ƃ��������Ƃ�
                    if (IsPlayPositionParent)
                        unitA.transform.parent = positionA.transform;//�e��ݒ�
                    UnitAComponents.transform.localPosition = Vector3.zero; // ���W��e�ɍ��킹��
                    unitA.transform.rotation = Quaternion.Euler(Vector3.zero); // ������e�ɍ��킹��

                    // ���[�J�����W�𒲐�����
                    //if (unitA.TryGetComponent<UnitDirectorOffset>(out var directorOffset))
                    //    unitA.transform.localPosition = directorOffset.GetBodyOffset(OriginBodyType); // �w��{�f�B�𒆐S�ɒ���
                    if (UnitAComponents.UnitDirectorCharacterOffset != null)
                        UnitAComponents.UnitDirectorCharacterOffset.OnSetLocalPos(director.gameObject.name, true);
                }
                // Bind UnitA
                BindAnimatorTrackDirector(director, director.playableAsset, unitATrackName, unitA.Animator);
                //DirectorManager.Instance._unitA = unitA.gameObject;

                //if (IsKinematicA)
                //    if (unitA.TryGetComponent<Rigidbody>(out var rigidBody))
                //        rigidBody.isKinematic = true;
            }

            // UnitB 
            if (unitB != null)
            {
                _unitBPlayPosition = unitB.transform.position;

                if (positionB != null)
                {
                    if (IsPlayPositionParent)
                        unitB.transform.parent = positionB.gameObject.transform;
                    unitB.transform.localPosition = Vector3.zero; // ���W��e�ɍ��킹��
                    unitB.transform.rotation = Quaternion.Euler(Vector3.zero); // ������e�ɍ��킹��

                    //// ���[�J�����W�𒲐�����
                    //if (unitB.TryGetComponent<UnitDirectorOffset>(out var directorOffset))
                    //{
                    //    unitB.transform.localPosition = directorOffset.GetBodyOffset(OriginBodyType);
                    //    Debug.Log(unitB.transform.localPosition);
                    //}
                    if (UnitBComponents.UnitDirectorCharacterOffset != null)
                        UnitBComponents.UnitDirectorCharacterOffset.OnSetLocalPos(director.gameObject.name, false);
                }
                // Bind UnitB
                BindAnimatorTrackDirector(director, director.playableAsset, unitBTrackName, unitB.Animator);
                //DirectorManager.Instance._unitB = unitB.gameObject;

                //if (IsKinematicB)
                //    if (unitB.TryGetComponent<Rigidbody>(out var rigidBody))
                //        rigidBody.isKinematic = true;
            }

            // Cinemachine
            var brainTrack = director.playableAsset.outputs.First(c => c.streamName == "Brain");
            director.SetGenericBinding(brainTrack.sourceObject, brain);

            // DirectorPlay
            if (isPlay)
            {
                await UniTask.Delay(1);
                director.time = 0.01f;
                director.Play();
            }

            //DebugPlayable.Instance.ChangeSpeed(1, 0.05f);
        }
        /// <summary>
        /// TrackBind
        /// </summary>
        /// <param name="director"></param>
        /// <param name="asset"></param>
        /// <param name="trackName"></param>
        /// <param name="animator"></param>
        private void BindAnimatorTrackDirector(PlayableDirector director, PlayableAsset asset, string trackName, Animator animator)
        {
            // ���݂���΁A�w�肳�ꂽ�f�B���N�^�[�Ƀ^�C�����C�����Z�b�g
            if (asset != null) director.playableAsset = asset;
            // director�ɃZ�b�g����Ă�A�Z�b�g��trackName�̃g���b�N�Ɉ�����Animator���Z�b�g
            var binding = director.playableAsset.outputs.First(c => c.streamName == trackName);
            director.SetGenericBinding(binding.sourceObject, animator);
        }

        /// <summary>
        /// ���j�b�gA������������
        /// </summary>
        /// <param name="unit"></param>
        public void ChangeUnitA(GameObject unit)
        {
            UnitAComponents = unit.GetComponent<develop_common.UnitComponents>();

            // offset�ǂ�����H�L�����傫���Ƃ��������Ƃ�
            UnitAComponents.transform.parent = PositionA.transform;//�e��ݒ�
            UnitAComponents.transform.localPosition = Vector3.zero; // ���W��e�ɍ��킹��
            UnitAComponents.transform.rotation = Quaternion.Euler(Vector3.zero); // ������e�ɍ��킹��
        }
        /// <summary>
        /// ���j�b�gB������������
        /// </summary>
        public void ChangeUnitB(GameObject unit)
        {
            UnitBComponents = unit.GetComponent<develop_common.UnitComponents>();

            // offset�ǂ�����H�L�����傫���Ƃ��������Ƃ�
            UnitBComponents.transform.parent = PositionB.transform;//�e��ݒ�
            UnitBComponents.transform.localPosition = Vector3.zero; // ���W��e�ɍ��킹��
            UnitBComponents.transform.rotation = Quaternion.Euler(Vector3.zero); // ������e�ɍ��킹��
        }

        public void SetPositionsParent(Transform parentObj)
        {
            if (Positions.transform.parent != parentObj)
            {
                Positions.transform.parent = parentObj;
                Positions.transform.localPosition = Vector3.zero;
                Positions.transform.rotation = Quaternion.Euler(Vector3.zero);
            }
        }

    }
}
