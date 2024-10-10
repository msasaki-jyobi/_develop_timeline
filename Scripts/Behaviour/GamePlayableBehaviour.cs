using develop_common;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

namespace develop_timeline
{

    public class GamePlayableBehaviour : PlayableBehaviour
    {
        // インスペクターで設定された値を取得用
        public GamePlayableAsset gamePlayableAsset;
        // 実行中のゲームオブジェクト
        public GameObject playablePlayer;
        // 必要な変数
        //public AudioManager audioManager;

        /// <summary>
        /// コンポーネントをまとめて取得
        /// </summary>
        private void GetComponents()
        {
            //if (audioManager == null)
            //{
            //    audioManager = AudioManager.Instance;
            //}
        }

        /// <summary>
        /// 開始時に呼び出される
        /// </summary>
        private void LineStart()
        {
            GetComponents();
            //GameManager.Instance.Player.GetComponent<Rigidbody>().isKinematic = true;
        }

        /// <summary>
        /// Playable呼び出し時
        /// </summary>
        private void LineUpdate()
        {
            // 効果音の再生
            var seClip = gamePlayableAsset.SEClip;
            if (seClip != null)
                if (seClip.Count != 0)
                    for (int i = 0; i < seClip.Count; i++)
                        AudioManager.Instance.PlayOneShotClipData(seClip[i]);

            var voiceClip = gamePlayableAsset.VoiceClip;
            if (voiceClip != null)
                if (voiceClip.Count != 0)
                    for (int i = 0; i < voiceClip.Count; i++)
                        AudioManager.Instance.PlayOneShotClipData(voiceClip[i]);

            var voiceID = gamePlayableAsset.PlayVoiceID;
            //if (voiceID != "")
            //    InstanceManager.Instance.PlayerVoice.PlayVoice(voiceID);


            var shapeData = gamePlayableAsset.SetShapeWordData;
            //if (shapeData != null)
            //    InstanceManager.Instance.UnitShape.SetShapeWard(shapeData);

            var message = gamePlayableAsset.Message;
            if (message != "")
                DirectorManager.Instance.OnSetTimelineMessage(message);

            // Managerの関数に文字列渡してイベントを呼ばせる



            //var effects = gamePlayableAsset.BodyEffects;
            //if(effects != null)
            //    if(effects.Count != 0)
            //        for (int i = 0; i < effects.Count; i++)
            //        {
            //            // 演者を取得
            //            var damagerA = TimelineManager.Instance.TargetMovieComponent.AnimatorA;
            //            var attackerB = TimelineManager.Instance.TargetMovieComponent.AnimatorB;
            //            var WeponC = TimelineManager.Instance.TargetMovieComponent.AnimatorC;
            //            // 対象を確定
            //            Bodys targetBody = null;
            //            switch(effects[i].Character)
            //            {
            //                case ECharacter.AnimatorA:
            //                    targetBody = damagerA.GetComponent<Bodys>();
            //                    break;
            //                case ECharacter.AnimatorB:
            //                    targetBody = attackerB.GetComponent<Bodys>();
            //                    break;
            //                case ECharacter.AnimatorC:
            //                    targetBody = WeponC.GetComponent<Bodys>();
            //                    break;
            //            }
            //            // 生成を頼む
            //            targetBody.CreateBodyEffect(effects[i]);
            //        }
        }

        /// <summary>
        /// Playable呼び出し（実行中のみ）
        /// </summary>
        private void LineGameUpdate()
        {
            // Stringイベントを実行する
            if (gamePlayableAsset.StringEvent != null)
            {
                foreach(var ev in gamePlayableAsset.StringEvent)
                    DirectorManager.Instance.UpdatePlayableEventInvoke(ev.EventName, ev.EventValue);
            }

            //var posID = gamePlayableAsset.PositionInstanceID;
            //var prefab = gamePlayableAsset.Prefab;
            //if (posID != "" && prefab != null)
            //{
            //    var posObj = InstanceManager.Instance.GetObjectInstance(posID);
            //    //var parentObject = gamePlayableAsset.IsParent ? posObj : null;
            //    var rot = gamePlayableAsset.SetRotation;
            //    //Functions.PlayEffect(posObj,prefab, parentObject:parentObject, rot:rot);
            //    InstanceManager.Instance.CreateObj(posID, prefab, gamePlayableAsset.IsParent, gamePlayableAsset.SetOffsetPos, rot);
            //}

        }

        /// <summary>
        /// TimeLine停止時
        /// </summary>
        private void LineStop()
        {
            Debug.Log("LineStop");
        }
        /// <summary>
        /// Playable停止時（実行中のみ）
        /// </summary>
        private void LineGameStop()
        {
            Debug.Log("LineGameStop");

            DirectorManager.Instance.FinishPlayable();
            // Movie終了処理を実行する
            //TimelineManager.Instance.OnEndDirectorAsync();
        }

        // メソッドに登録 //

        /// <summary>
        /// タイムライン開始時に呼び出される
        /// </summary>
        /// <param name="playable"></param>
        public override void OnGraphStart(Playable playable)
        {
            LineStart();
        }

        /// <summary>
        /// タイムライン停止時・終了時に呼び出される
        /// </summary>
        /// <param name="playable"></param>
        public override void OnGraphStop(Playable playable)
        {
            LineStop();
            // ゲーム実行中だけ動く処理
#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                LineGameStop();
            }
#else
        if (Application.isPlaying)
        {
              LineGameStop();
        }
#endif
        }

        /// <summary>
        /// スクリプトが呼び出された際に実行される
        /// </summary>
        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            LineUpdate();
            // ゲーム実行中だけ動く処理
#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                LineGameUpdate();
            }
#else
        if (Application.isPlaying)
        {
            LineGameUpdate();
        }
#endif
        }


        /// <summary>
        /// タイムラインが一時停止したら呼び出される
        /// </summary>
        /// <param name="playable"></param>
        /// <param name="info"></param>
        public override void OnBehaviourPause(Playable playable, FrameData info)
        {

        }

        /// <summary>
        /// タイムラインの各フレーム毎に呼び出される
        /// </summary>
        /// <param name="playable"></param>
        /// <param name="info"></param>
        public override void PrepareFrame(Playable playable, FrameData info)
        {

        }
    }
}
