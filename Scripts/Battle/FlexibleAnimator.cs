using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

namespace develop_common
{
    public class FlexibleAnimator : MonoBehaviour
    {
        public GameObject Root; // このオブジェクトのRoot
        public Transform Target; // アニメーション対象のTransform
        public AnimatorStateController AnimatorStateController; // アニメーターのステートコントローラー
        public AnimationData AnimationData; // スクリプタブルオブジェクトを参照
        private Sequence _animationSequence; // 現在のアニメーションシーケンス
        private bool isAnimating = false; // アニメーション中かどうかを管理

        private void Start()
        {
            DOTween.Init(); // DoTweenの初期化
            if (AnimationData != null)
            {
                ResetToInitial();
            }
        }

        // 初期位置にリセット
        private void ResetToInitial()
        {
            Target.localPosition = AnimationData.InitialPosition;
            Target.localRotation = Quaternion.Euler(AnimationData.InitialRotation);
        }

        // アニメーション再生メソッド
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
                // アニメーション中なら現在のシーケンスを停止し、初期位置に戻る
                _animationSequence.Kill();
                SmoothReturnToInitial(() =>
                {
                    StartAnimating();
                });
            }
            else
            {
                // アニメーション中でない場合、新規に再生開始
                StartAnimating();
            }
        }

        // 初期位置にスムーズに戻る
        private void SmoothReturnToInitial(Action onComplete = null)
        {
            isAnimating = true; // アニメーション中フラグを立てる
            _animationSequence = DOTween.Sequence()
                .Append(Target.DOLocalMove(AnimationData.InitialPosition, AnimationData.ReturnDuration).SetEase(Ease.InOutCubic))
                .Join(Target.DOLocalRotate(AnimationData.InitialRotation, AnimationData.ReturnDuration).SetEase(Ease.InOutCubic))
                .OnComplete(() =>
                {
                    onComplete?.Invoke(); // 初期位置に戻った後、指定された処理を実行
                });
        }

        // アニメーションシーケンスの開始
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
