using Cinemachine;
using develop_body;
using develop_common;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering.PostProcessing;

// PlayerにDirectorOffsetクラス（案）
//    Head : 0, 0.1f, 0
//    Spine: 0. 0.2f, 0
// DirectorPlayaerはどこを基準にするか選択する

namespace develop_timeline
{
    public class DirectorPlayer : SingletonMonoBehaviour<DirectorPlayer>
    {
        [Space(10)]
        public EBodyType OriginBodyType;
        public List<StringEventHandle> StartEventHandles = new List<StringEventHandle>();
        public List<StringEventHandle> FinishEventHandles = new List<StringEventHandle>();
        // 終了時に実行したいパラメーター

        [Space(10)]
        public PlayableDirector Director;
        public Animator PositionA;
        public Animator PositionB;
        public bool IsKinematicA;
        public bool IsKinematicB;
        public Animator InitUnitA;
        public Animator InitUnitB;

        [Space(10)]
        // 逆再生用
        //[Header("逆再生機能　ON/OFF")]
        //private bool IsReversing;
        public float Speed = 1.0f;
        public float ChangeTime = 0.05f;

        [Space(10)]
        // Debug
        public bool IsPlayableControl;

        private CinemachineBrain _brain;
        private GameObject _unitA;
        private GameObject _unitB;

        private void Start()
        {
            
        }


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
                Play();

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

        public void Play()
        {
            OnSetPlayDirector(unitA: InitUnitA, unitB: InitUnitB);
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
        /// ムービーを実行する
        /// </summary>
        /// <param name="Asset">実行するムービー</param>
        /// <param name="unitB">敵のAnimator</param>
        /// <param name="playerParent">プレイヤーの位置管理オブジェクト</param>
        /// <param name="enemyParent">敵の位置管理オブジェクト</param>
        public void OnSetPlayDirector(Animator unitA = null, Animator unitB = null)
        {
            var unitATrackName = "UnitA";
            var unitBTrackName = "UnitB";
            var posATrackName = "PosA";
            var posBTrackName = "PosB";

            DirectorManager.Instance.SetPlayDirector(Director);

            _brain = DirectorManager.Instance.Brain;

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
                    // offsetどうする？キャラ大きいとか小さいとか
                    unitA.transform.parent = PositionA.transform;//親を設定
                    unitA.transform.localPosition = Vector3.zero; // 座標を親に合わせる
                    unitA.transform.rotation = Quaternion.Euler(Vector3.zero); // 向きを親に合わせる

                    // ローカル座標を調整する
                    if (unitA.TryGetComponent<UnitDirectorOffset>(out var directorOffset))
                        unitA.transform.localPosition = directorOffset.GetBodyOffset(OriginBodyType);
                }
                // Bind UnitA
                BindAnimatorTrackDirector(Director, Director.playableAsset, unitATrackName, unitA);
                _unitA = unitA.gameObject;
                DirectorManager.Instance.UnitA = unitA.gameObject;

                if (IsKinematicA)
                    if (unitA.TryGetComponent<Rigidbody>(out var rigidBody))
                        rigidBody.isKinematic = true;
            }

            // UnitB 
            if (unitB != null)
            {
                if (PositionB != null)
                {
                    unitB.transform.parent = PositionB.gameObject.transform;
                    unitB.transform.localPosition = Vector3.zero; // 座標を親に合わせる
                    unitB.transform.rotation = Quaternion.Euler(Vector3.zero); // 向きを親に合わせる

                    // ローカル座標を調整する
                    if (unitB.TryGetComponent<UnitDirectorOffset>(out var directorOffset))
                    {
                        unitB.transform.localPosition = directorOffset.GetBodyOffset(OriginBodyType);
                        Debug.Log(unitB.transform.localPosition);
                    }
                }
                // Bind UnitB
                BindAnimatorTrackDirector(Director, Director.playableAsset, unitBTrackName, unitB);
                _unitB = unitB.gameObject;
                DirectorManager.Instance.UnitB = unitB.gameObject;

                if (IsKinematicB)
                    if (unitB.TryGetComponent<Rigidbody>(out var rigidBody))
                        rigidBody.isKinematic = true;
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
            // 存在すれば、指定されたディレクターにタイムラインをセット
            if (asset != null) director.playableAsset = asset;
            // directorにセットされてるアセットのtrackNameのトラックに引数のAnimatorをセット
            var binding = director.playableAsset.outputs.First(c => c.streamName == trackName);
            director.SetGenericBinding(binding.sourceObject, animator);
        }


    }
}
