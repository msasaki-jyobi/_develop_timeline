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
    public enum EDirectorType
    {
        Normal, // �ʏ���s
        PlayPosReset, // �I����Ɏ��s�O�̍��W�ɖ߂�
    }


    public class DirectorManager : SingletonMonoBehaviour<DirectorManager>
    {
        public TextMeshProUGUI TimelineTextGUI;
        public CinemachineBrain Brain;

        public GameObject UnitAPrefab;
        public GameObject UnitBPrefab;
        public GameObject Positions;
        private Animator _unitA;
        private Animator _unitB;
        public Animator PositionA;
        public Animator PositionB;

        [Header("���ݍĐ����s���Ă���PlayableDirector")]
        public PlayableDirector ActivePlayingDirector;

        public event Action<string, string> StartEvent;
        public event Action<EPlayableParamater, string, bool> UpdatePlayableEvent;
        public event Action<string, string> FinishNamedEvent;
        public event Action FinishEvent;

        public int ThirdCount; // �R�񃋁[�v�p
        public develop_common.UnitComponents UnitAComponents;
        public develop_common.UnitComponents UnitBComponents;

        [Header("FadeController�𗘗p����NormalPlay�p")]
        public float NormalFadeTime = 0.5f;
        public Color NormalPlayFadeColor;

      

        private async void Start()
        {
            var unitA = Instantiate(UnitAPrefab);
            var unitB = Instantiate(UnitBPrefab);
            unitA.gameObject.name = UnitAPrefab.name;
            unitB.gameObject.name = UnitBPrefab.name;

            await UniTask.WaitUntil(() => unitA != null);
            await UniTask.WaitUntil(() => unitB != null);

            ChangeUnitA(unitA.GetComponent<develop_common.UnitComponents>().Animator);
            ChangeUnitB(unitB.GetComponent<develop_common.UnitComponents>().Animator);
        }

        public bool IsCheckPlaying()
        {
            return ActivePlayingDirector != null;
        }

        public async void NormalPlayDirector(PlayableDirector director)
        {
            ThirdCount = 0; // 3��J�E���^�[���Z�b�g
            if (ActivePlayingDirector != null) // ���ݍĐ����̂͒�~
            {
                ActivePlayingDirector.Pause();
                ActivePlayingDirector.Stop();
            }
            await UniTask.Delay(1);

            // UnitA��Position�����ւ�����ϕK�{���I
            // UnitA��Position�����ւ�����ϕK�{���I
            // UnitA��Position�����ւ�����ϕK�{���I
            // UnitA��Position�����ւ�����ϕK�{���I
            // UnitA��Position�����ւ�����ϕK�{���I
            // UnitA��Position�����ւ�����ϕK�{���I
            // UnitA, B��PositionA, B�����ւ�����ϕK�{���I



            // �Đ������s
            ActivePlayingDirector = director;

            if (NormalFadeTime <= 0)
                SetDirectors(director, _unitA, _unitB, PositionA, PositionB, Brain, true);
                //director.Play();
            else
                FadeController.Instance.ActionPlayFadeIn(() =>
                {
                    SetDirectors(director, _unitA, _unitB, PositionA, PositionB, Brain, true);
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
            if (TextFadeController.Instance != null)
                TextFadeController.Instance.UpdateMessageText(message);
            else if (TimelineTextGUI != null)
                TimelineTextGUI.text = message;
        }

        public void FinishPlayable()
        {
            return;
            if (ActivePlayingDirector != null)
            {
                if (ActivePlayingDirector.gameObject.TryGetComponent<DirectorPlayer>(out var directorPlayer))
                {
                    if (_unitA != null)
                    {
                        if (_unitA.TryGetComponent<Rigidbody>(out var rigidbody))
                            rigidbody.isKinematic = false;
                        _unitA.transform.parent = null;
                        _unitA = null;
                    }
                    if (_unitB != null)
                    {
                        if (_unitB.TryGetComponent<Rigidbody>(out var rigidbody))
                            rigidbody.isKinematic = false;
                        _unitB.transform.parent = null;
                        _unitB = null;
                    }
                    directorPlayer.OnPlayFinish();
                    Destroy(ActivePlayingDirector.gameObject);

                    foreach (var finishEvent in directorPlayer.FinishEventHandles)
                        FinishNamedEvent?.Invoke(finishEvent.EventName, finishEvent.EventValue);

                }
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

        public async void SetDirectors(PlayableDirector director, Animator unitA, Animator unitB, Animator positionA, Animator positionB, CinemachineBrain brain, bool isPlay = false)
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
                if (positionA != null)
                {
                    // offset�ǂ�����H�L�����傫���Ƃ��������Ƃ�
                    unitA.transform.parent = positionA.transform;//�e��ݒ�
                    _unitA.transform.localPosition = Vector3.zero; // ���W��e�ɍ��킹��
                    unitA.transform.rotation = Quaternion.Euler(Vector3.zero); // ������e�ɍ��킹��

                    // ���[�J�����W�𒲐�����
                    //if (unitA.TryGetComponent<UnitDirectorOffset>(out var directorOffset))
                    //    unitA.transform.localPosition = directorOffset.GetBodyOffset(OriginBodyType); // �w��{�f�B�𒆐S�ɒ���
                    if (UnitAComponents.UnitDirectorCharacterOffset != null)
                        UnitAComponents.UnitDirectorCharacterOffset.OnSetLocalPos(director.gameObject.name, true);
                }
                // Bind UnitA
                BindAnimatorTrackDirector(director, director.playableAsset, unitATrackName, unitA);
                //DirectorManager.Instance._unitA = unitA.gameObject;

                //if (IsKinematicA)
                //    if (unitA.TryGetComponent<Rigidbody>(out var rigidBody))
                //        rigidBody.isKinematic = true;
            }

            // UnitB 
            if (unitB != null)
            {
                if (positionB != null)
                {
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
                BindAnimatorTrackDirector(director, director.playableAsset, unitBTrackName, unitB);
                //DirectorManager.Instance._unitB = unitB.gameObject;

                //if (IsKinematicB)
                //    if (unitB.TryGetComponent<Rigidbody>(out var rigidBody))
                //        rigidBody.isKinematic = true;
            }

            // Cinemachine
            var brainTrack = director.playableAsset.outputs.First(c => c.streamName == "Brain");
            director.SetGenericBinding(brainTrack.sourceObject, brain);

            // DirectorPlay
            if(isPlay)
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
        public void ChangeUnitA(Animator unit)
        {
            _unitA = unit;
            UnitAComponents = unit.GetComponent<develop_common.UnitComponents>();

            // offset�ǂ�����H�L�����傫���Ƃ��������Ƃ�
            _unitA.transform.parent = PositionA.transform;//�e��ݒ�
            _unitA.transform.localPosition = Vector3.zero; // ���W��e�ɍ��킹��
            _unitA.transform.rotation = Quaternion.Euler(Vector3.zero); // ������e�ɍ��킹��
        }
        public void ChangeUnitB(Animator unit)
        {
            _unitB = unit;
            UnitBComponents = unit.GetComponent<develop_common.UnitComponents>();

            // offset�ǂ�����H�L�����傫���Ƃ��������Ƃ�
            _unitB.transform.parent = PositionB.transform;//�e��ݒ�
            _unitB.transform.localPosition = Vector3.zero; // ���W��e�ɍ��킹��
            _unitB.transform.rotation = Quaternion.Euler(Vector3.zero); // ������e�ɍ��킹��
        }

        public void SetPositionsParent(Transform parentObj)
        {
            if(Positions.transform.parent != parentObj)
            {
                Positions.transform.parent = parentObj;
                Positions.transform.localPosition = Vector3.zero;
                Positions.transform.rotation = Quaternion.Euler(Vector3.zero);
            }
        }

    }
}
