using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

namespace develop_common
{
    public class FlexibleAnimator : MonoBehaviour
    {
        public GameObject Root; // ���̃I�u�W�F�N�g��Root
        public Transform Target; // �A�j���[�V�����Ώۂ�Transform
        public AnimatorStateController AnimatorStateController; // �A�j���[�^�[�̃X�e�[�g�R���g���[���[
        public AnimationData AnimationData; // �X�N���v�^�u���I�u�W�F�N�g���Q��
        private Sequence _animationSequence; // ���݂̃A�j���[�V�����V�[�P���X
        private bool isAnimating = false; // �A�j���[�V���������ǂ������Ǘ�

        private void Start()
        {
            DOTween.Init(); // DoTween�̏�����
            if (AnimationData != null)
            {
                ResetToInitial();
            }
        }

        // �����ʒu�Ƀ��Z�b�g
        private void ResetToInitial()
        {
            Target.localPosition = AnimationData.InitialPosition;
            Target.localRotation = Quaternion.Euler(AnimationData.InitialRotation);
        }

        // �A�j���[�V�����Đ����\�b�h
        public void Play()
        {
            if (AnimationData == null)
            {
                Debug.LogWarning("AnimationData is not assigned!");
                return;
            }

            ResetToInitial();

            if (isAnimating)
            {
                // �A�j���[�V�������Ȃ猻�݂̃V�[�P���X���~���A�����ʒu�ɖ߂�
                _animationSequence.Kill();
                SmoothReturnToInitial(() =>
                {
                    StartAnimating();
                });
            }
            else
            {
                // �A�j���[�V�������łȂ��ꍇ�A�V�K�ɍĐ��J�n
                StartAnimating();
            }
        }

        // �����ʒu�ɃX���[�Y�ɖ߂�
        private void SmoothReturnToInitial(Action onComplete = null)
        {
            isAnimating = true; // �A�j���[�V�������t���O�𗧂Ă�
            _animationSequence = DOTween.Sequence()
                .Append(Target.DOLocalMove(AnimationData.InitialPosition, AnimationData.ReturnDuration).SetEase(Ease.InOutCubic))
                .Join(Target.DOLocalRotate(AnimationData.InitialRotation, AnimationData.ReturnDuration).SetEase(Ease.InOutCubic))
                .OnComplete(() =>
                {
                    onComplete?.Invoke(); // �����ʒu�ɖ߂�����A�w�肳�ꂽ���������s
                });
        }

        // �A�j���[�V�����V�[�P���X�̊J�n
        private void StartAnimating()
        {
            if (AnimationData == null || AnimationData.AnimationSteps.Count == 0)
            {
                Debug.LogWarning("No animation steps found or AnimationData is null!");
                return;
            }

            Debug.Log("Starting animation with " + AnimationData.AnimationSteps.Count + " steps");

            _animationSequence?.Kill();

            isAnimating = true;
            _animationSequence = DOTween.Sequence();

            foreach (var step in AnimationData.AnimationSteps)
            {
                Debug.Log($"Adding Step - LocalPosition: {step.LocalPosition}, Duration: {step.Duration}");

                if (!string.IsNullOrEmpty(step.StateName))
                {
                    _animationSequence.AppendCallback(() =>
                    {
                        AnimatorStateController.StatePlay(step.StateName, step.StatePlayType, step.StateReset);
                        Debug.Log($"Playing State: {step.StateName}");
                    });
                }

                _animationSequence.Append(Target.DOLocalMove(step.LocalPosition, step.Duration).SetEase(Ease.OutCubic)
                    .OnStart(() => Debug.Log($"DOLocalMove Started: {Target.localPosition} -> {step.LocalPosition}"))
                    .OnComplete(() => Debug.Log($"DOLocalMove Completed: {Target.localPosition}")));

                _animationSequence.Join(Target.DOLocalRotate(step.LocalRotation, step.Duration).SetEase(Ease.OutCubic));
                _animationSequence.AppendInterval(step.TransitionDuration);
            }

            _animationSequence.OnComplete(() =>
            {
                Debug.Log("Animation sequence completed.");
                if (AnimationData.IsLooping)
                {
                    StartAnimating();
                }
                else
                {
                    isAnimating = false;
                    SmoothReturnToInitial();
                }
            });

            _animationSequence.Play();
        }

    }
}
