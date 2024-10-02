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

        [Space(10)]
        // 逆再生用
        //[Header("逆再生機能　ON/OFF")]
        //private bool IsReversing;
        public float Speed = 1.0f;
        public float ChangeTime = 0.05f;

        [Space(10)]
        // Debug
        public PlayableAsset DebugPlayable;
        public Animator DebugUnitB;
        public bool IsDebugPlay;
        public bool IsPlayerControl;

        private PlayableDirector _playingDirector;

        void Update()
        {
            // Debug
            if (IsDebugPlay)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1))
                    OnSetPlayDirector(DebugPlayable, unitA: DefaultUnitA, unitB: DebugUnitB, overrideDirector: null);
                if (Input.GetKeyDown(KeyCode.Alpha2))
                    OnSetPlayDirector(DebugPlayable, unitA: DefaultUnitA, unitB: DebugUnitB, overrideDirector: null, playPosition: new Vector3(0, 10, 0), playRotation: new Vector3(0, 45, 0));
            }


            if (_playingDirector == null) return;

            if (!IsPlayerControl) return;
            // Init Play
            if (Input.GetKeyDown(KeyCode.F))
            {
                _playingDirector.time = 0.01f;
                _playingDirector.Play();
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

        public void ChangePlaySpeed(float newSpeed, float duration)
        {
            float currentSpeed = (float)_playingDirector.playableGraph.GetRootPlayable(0).GetSpeed();
            DOTween.To(() => currentSpeed, x =>
            {
                _playingDirector.playableGraph.GetRootPlayable(0).SetSpeed(x);
                currentSpeed = x;
            }, newSpeed, duration);
        }

        /// <summary>
        /// ムービーを実行する
        /// </summary>
        /// <param name="asset">実行するムービー</param>
        /// <param name="unitB">敵のAnimator</param>
        /// <param name="playerParent">プレイヤーの位置管理オブジェクト</param>
        /// <param name="enemyParent">敵の位置管理オブジェクト</param>
        public void OnSetPlayDirector(PlayableAsset asset, Animator unitA = null, Animator unitB = null, PlayableDirector overrideDirector = null, Vector3 playPosition = default, Vector3 playRotation = default)
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

            // Play Pos Rot
            if (playPosition != default)
                transform.position = playPosition;
            if (playRotation != default)
                transform.rotation = Quaternion.Euler(playRotation);

            // PlayerAnimator
            if (unitA == null)
                unitA = DefaultUnitA;

            // Bind Pos
            if (PosA != null)
                BindAnimatorTrackDirector(_playingDirector, asset, posATrackName, PosA);
            if (PosB != null)
                BindAnimatorTrackDirector(_playingDirector, asset, posBTrackName, PosB);

            // UnitA
            if (unitA != null)
            {
                if (PosA != null)
                {
                    // offsetどうする？キャラ大きいとか小さいとか
                    unitA.transform.parent = PosA.transform;//親を設定
                    unitA.transform.localPosition = Vector3.zero; // 座標を親に合わせる
                    unitA.transform.rotation = Quaternion.Euler(Vector3.zero); // 向きを親に合わせる
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
                    unitB.transform.localPosition = Vector3.zero; // 座標を親に合わせる
                    unitB.transform.rotation = Quaternion.Euler(Vector3.zero); // 向きを親に合わせる
                }
                // Bind UnitB
                BindAnimatorTrackDirector(_playingDirector, asset, unitBTrackName, unitB);
            }



            // Cinemachine
            var brainTrack = _playingDirector.playableAsset.outputs.First(c => c.streamName == "Brain");
            _playingDirector.SetGenericBinding(brainTrack.sourceObject, Brain);

            // DirectorPlay
            _playingDirector.time = 0.01f;
            _playingDirector.Play();
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
            // 存在すれば、指定されたディレクターにタイムラインをセット
            if (asset != null) director.playableAsset = asset;
            // directorにセットされてるアセットのtrackNameのトラックに引数のAnimatorをセット
            var binding = director.playableAsset.outputs.First(c => c.streamName == trackName);
            director.SetGenericBinding(binding.sourceObject, animator);
        }

    }
}
