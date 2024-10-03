using Cinemachine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering.PostProcessing;

namespace develop_timeline
{
    public class DirectorPlayer : SingletonMonoBehaviour<DirectorPlayer>
    {
        [Space(10)]
        public bool IsFinishSetParameterA;
        // �I�����Ɏ��s�������p�����[�^�[

        [Space(10)]
        public PlayableDirector Director;
        public Animator PositionA;
        public Animator PositionB;

        [Space(10)]
        // �t�Đ��p
        //[Header("�t�Đ��@�\�@ON/OFF")]
        //private bool IsReversing;
        public float Speed = 1.0f;
        public float ChangeTime = 0.05f;

        [Space(10)]
        // Debug
        public bool IsPlayableControl;

        private CinemachineBrain _brain;
        private GameObject _unitA;
        private GameObject _unitB;


        void Update()
        {
            // Debug
            //if (IsDebugPlay)
            //{
            //    if (Input.GetKeyDown(KeyCode.Alpha1))
            //        OnSetPlayDirector(DebugPlayable, unitA: DefaultUnitA, unitB: DebugUnitB, overrideDirector: null);
            //    if (Input.GetKeyDown(KeyCode.Alpha2))
            //        OnSetPlayDirector(DebugPlayable, unitA: DefaultUnitA, unitB: DebugUnitB, overrideDirector: null, playPosition: new Vector3(0, 10, 0), playRotation: new Vector3(0, 45, 0));
            //}


            if (Director == null) return;

            if (!IsPlayableControl) return;
            // Init Play
            if (Input.GetKeyDown(KeyCode.F))
            {
                Director.time = 0.01f;
                Director.Play();
            }
            // Back Play
            if (Input.GetKeyDown(KeyCode.A))
            {
                ChangePlaySpeed(-Speed, ChangeTime);
            }
            // Forward Play
            if (Input.GetKeyDown(KeyCode.D))
            {
                ChangePlaySpeed(Speed, ChangeTime);
            }
            //if (Input.GetKeyDown(KeyCode.E))
            //{
            //    IsReversing = !IsReversing;
            //}
            if (Director != null)
                if (Director.time < 0.01f)
                {
                    if (Director.state == PlayState.Playing)
                    {
                        Director.time = 0.01f;
                        Director.playableGraph.GetRootPlayable(0).SetSpeed(0);
                    }
                }
        }

        public void ChangePlaySpeed(float newSpeed, float duration)
        {
            float currentSpeed = (float)Director.playableGraph.GetRootPlayable(0).GetSpeed();
            DOTween.To(() => currentSpeed, (DG.Tweening.Core.DOSetter<float>)(x =>
            {
                PlayableExtensions.SetSpeed<Playable>(this.Director.playableGraph.GetRootPlayable(0), x);
                currentSpeed = x;
            }), newSpeed, duration);
        }

        /// <summary>
        /// ���[�r�[�����s����
        /// </summary>
        /// <param name="Asset">���s���郀�[�r�[</param>
        /// <param name="unitB">�G��Animator</param>
        /// <param name="playerParent">�v���C���[�̈ʒu�Ǘ��I�u�W�F�N�g</param>
        /// <param name="enemyParent">�G�̈ʒu�Ǘ��I�u�W�F�N�g</param>
        public void OnSetPlayDirector(Animator unitA = null, Animator unitB = null)
        {
            var unitATrackName = "UnitA";
            var unitBTrackName = "UnitB";
            var posATrackName = "PosA";
            var posBTrackName = "PosB";

            DirectorManager.Instance.SetPlayDirector(Director);

            _brain = Camera.main.GetComponent<CinemachineBrain>();

            // Bind Pos
            if (PositionA != null)
                BindAnimatorTrackDirector(Director, Director.playableAsset, posATrackName, PositionA);
            if (PositionB != null)
                BindAnimatorTrackDirector(Director, Director.playableAsset, posBTrackName, PositionB);

            // UnitA
            if (unitA != null)
            {
                if (PositionA != null)
                {
                    // offset�ǂ�����H�L�����傫���Ƃ��������Ƃ�
                    unitA.transform.parent = PositionA.transform;//�e��ݒ�
                    unitA.transform.localPosition = Vector3.zero; // ���W��e�ɍ��킹��
                    unitA.transform.rotation = Quaternion.Euler(Vector3.zero); // ������e�ɍ��킹��
                }
                // Bind UnitA
                BindAnimatorTrackDirector(Director, Director.playableAsset, unitATrackName, unitA);
                _unitA = unitA.gameObject;
            }

            // UnitB 
            if (unitB != null)
            {
                if (PositionB != null)
                {
                    unitB.transform.parent = PositionB.gameObject.transform;
                    unitB.transform.localPosition = Vector3.zero; // ���W��e�ɍ��킹��
                    unitB.transform.rotation = Quaternion.Euler(Vector3.zero); // ������e�ɍ��킹��
                }
                // Bind UnitB
                BindAnimatorTrackDirector(Director, Director.playableAsset, unitBTrackName, unitB);
                _unitB = unitB.gameObject;
            }

            // Cinemachine
            var brainTrack = Director.playableAsset.outputs.First(c => c.streamName == "Brain");
            Director.SetGenericBinding(brainTrack.sourceObject, _brain);

            // DirectorPlay
            Director.time = 0.01f;
            Director.Play();
            //DebugPlayable.Instance.ChangeSpeed(1, 0.05f);
        }

        public void OnPlayFinish()
        {
            if (_unitA != null)
            {
                _unitA.transform.parent = null;
                if (IsFinishSetParameterA)
                    Debug.Log("UnitAni Finish Parameter");

            }
            if (_unitB != null)
            {
                _unitB.transform.parent = null;

            }
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


    }
}