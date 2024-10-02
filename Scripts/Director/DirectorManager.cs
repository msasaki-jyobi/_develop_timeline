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
    public class DirectorManager : SingletonMonoBehaviour<DirectorManager>
    {
        // Inspector attach 
        public PlayableDirector DefaultDirector;
        public CinemachineBrain Brain;
        public Animator PosA;
        public Animator PosB;
        public Animator DefaultUnitA;

        // �t�Đ��p
        //[Header("�t�Đ��@�\�@ON/OFF")]
        //private bool IsReversing;
        public float Speed = 1.0f;
        public float ChangeTime = 0.05f;

        // Debug
        public PlayableAsset DebugPlayable;
        public Animator DebugUnitB;

        private PlayableDirector _playingDirector;

        void Update()
        {
            // Debug
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                OnSetPlayDirector(DebugPlayable, unitA: DefaultUnitA, unitB: DebugUnitB, overrideDirector: null);
            }

            if (_playingDirector == null)
                return;

            // Init Play
            if (Input.GetKeyDown(KeyCode.F))
            {
                _playingDirector.time = 0.01f;
                _playingDirector.Play();
            }
            // Back Play
            if (Input.GetKeyDown(KeyCode.A))
            {
                ChangeSpeed(-Speed, ChangeTime);
            }
            // Forward Play
            if (Input.GetKeyDown(KeyCode.D))
            {
                ChangeSpeed(Speed, ChangeTime);
            }
            //if (Input.GetKeyDown(KeyCode.E))
            //{
            //    IsReversing = !IsReversing;
            //}
            if (_playingDirector != null)
                if (_playingDirector.time < 0.01f)
                {
                    if (_playingDirector.state == PlayState.Playing)
                    {
                        _playingDirector.time = 0.01f;
                        _playingDirector.playableGraph.GetRootPlayable(0).SetSpeed(0);
                    }
                }
        }

        public void ChangeSpeed(float newSpeed, float duration)
        {
            float currentSpeed = (float)_playingDirector.playableGraph.GetRootPlayable(0).GetSpeed();
            DOTween.To(() => currentSpeed, x =>
            {
                _playingDirector.playableGraph.GetRootPlayable(0).SetSpeed(x);
                currentSpeed = x;
            }, newSpeed, duration);
        }

        /// <summary>
        /// ���[�r�[�����s����
        /// </summary>
        /// <param name="asset">���s���郀�[�r�[</param>
        /// <param name="unitB">�G��Animator</param>
        /// <param name="playerParent">�v���C���[�̈ʒu�Ǘ��I�u�W�F�N�g</param>
        /// <param name="enemyParent">�G�̈ʒu�Ǘ��I�u�W�F�N�g</param>
        public void OnSetPlayDirector(PlayableAsset asset, Animator unitA = null, Animator unitB = null, PlayableDirector overrideDirector = null)
        {
            var unitATrackName = "UnitA";
            var unitBTrackName = "UnitB";
            var posATrackName = "PosA";
            var posBTrackName = "PosB";

            // Direcotr Override
            if (overrideDirector != null)
                _playingDirector = overrideDirector;
            else if (_playingDirector == null)
                _playingDirector = DefaultDirector;

            // PlayerAnimator
            if (unitA == null)
                unitA = DefaultUnitA;

            // Bind Pos
            if(PosA != null)
                BindAnimatorTrackDirector(_playingDirector, asset, posATrackName, PosA);
            if(PosB != null)
                BindAnimatorTrackDirector(_playingDirector, asset, posBTrackName, PosB);

            // UnitA
            if (unitA != null)
            {
                if (PosA != null)
                {
                    unitA.transform.parent = PosA.transform;//�e��ݒ�
                    unitA.transform.localPosition = Vector3.zero; // ���W��e�ɍ��킹��
                    unitA.transform.rotation = Quaternion.Euler(Vector3.zero); // ������e�ɍ��킹��
                }
                // Bind UnitA
                BindAnimatorTrackDirector(_playingDirector, asset, unitATrackName, unitA);
            }

            // UnitB 
            if (unitB != null)
            {
                if (PosB != null)
                {
                    unitB.transform.parent = PosB.gameObject.transform;
                    unitB.transform.localPosition = Vector3.zero; // ���W��e�ɍ��킹��
                    unitB.transform.rotation = Quaternion.Euler(Vector3.zero); // ������e�ɍ��킹��
                }
                // Bind UnitB
                BindAnimatorTrackDirector(_playingDirector, asset, unitBTrackName, unitB);
            }



            // Cinemachine
            var brainTrack = _playingDirector.playableAsset.outputs.First(c => c.streamName == "Brain");
            _playingDirector.SetGenericBinding(brainTrack.sourceObject, Brain);

            // DirectorPlay
            _playingDirector.Play();
            //DebugPlayable.Instance.ChangeSpeed(1, 0.05f);
        }

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
