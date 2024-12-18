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

        [Header("再生時、Unitをprivate変数に上書き")]
        public bool UnitOverwrite_UnitComponents;
        [Header("再生時、Positionでラップする")]
        public bool IsPlayPositionParent; // 再生時に指定されたPositionの子オブジェクトにして、終了時に元に戻す
        [Header("再生終了時にPositionから解放し、元の位置に戻す")]
        public bool IsFinishResetPos; // 再生時 各オブジェクトの座標を記録して 終了時にPositionAから外して元に戻す
        [Header("Positionsを再生する時にUnitBに同期する")]
        public bool IsPlayPositionsUnitBSync;
        public Transform DefaultPositionsParent;
        private Vector3 _unitAPlayPosition;
        private Vector3 _unitBPlayPosition;


        [Header("＝＝＝＝＝＝")]
        [Header("プレハブを生成し、A,Bに自動割り当て")]
        public bool IsCreatePrefab;
        public GameObject UnitAPrefab;
        public GameObject UnitBPrefab;
        //[Header("B：自動割り当てしない場合は、手動でアタッチ")]
        //private Animator _unitA;
        //private Animator _unitB;
        [Header("参照必須（自動生成ならからでOK）")]
        public develop_common.UnitComponents UnitAComponents;
        public develop_common.UnitComponents UnitBComponents;


        [Header("＝＝＝＝＝＝")]
        [Header("事前にアタッチ、")]
        public GameObject Positions;
        public Animator PositionA;
        public Animator PositionB;

        [Header("現在再生が行われているPlayableDirector")]
        public PlayableDirector ActivePlayingDirector;

        public event Action<string, string> StartEvent;
        public event Action<EPlayableParamater, string, bool> UpdatePlayableEvent;
        public event Action<string, string> FinishNamedEvent;
        public event Action FinishEvent;

        public int ThirdCount; // ３回ループ用


        [Header("FadeControllerを利用したNormalPlay用")]
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

                ChangeUnitA(unitA); // UnitComponentsにプレハブをアタッチ
                ChangeUnitB(unitB); // UnitComponentsにプレハブをアタッチ
            }
        }

        public bool IsCheckPlaying()
        {
            return ActivePlayingDirector != null;
        }
        /// <summary>
        /// 設定されているUnitA,UnitBを元にモーションを実行
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

            ThirdCount = 0; // 3回カウンターリセット
            if (ActivePlayingDirector != null) // 現在再生中のは停止
            {
                ActivePlayingDirector.Pause();
                ActivePlayingDirector.Stop();
            }
            await UniTask.Delay(1);

            // 再生を実行
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
                Debug.LogError($"既に${ActivePlayingDirector.playableAsset} が実行中でした");
                isCheck = false;
            }

            ActivePlayingDirector = director;

            // DirectorPlayerがアタッチされていれば
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
                // 再生を実行したDirectorに「DirectorPlayer」が存在する場合
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


                // 終了処理
                ActivePlayingDirector = null;
                TimelineTextGUI.text = "";
                FinishNamedEvent?.Invoke("", "");
                FinishEvent?.Invoke();
            }
            else
            {
                Debug.LogError("実行中のDirectorはありません");
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
                    // offsetどうする？キャラ大きいとか小さいとか
                    if (IsPlayPositionParent)
                        unitA.transform.parent = positionA.transform;//親を設定
                    UnitAComponents.transform.localPosition = Vector3.zero; // 座標を親に合わせる
                    unitA.transform.rotation = Quaternion.Euler(Vector3.zero); // 向きを親に合わせる

                    // ローカル座標を調整する
                    //if (unitA.TryGetComponent<UnitDirectorOffset>(out var directorOffset))
                    //    unitA.transform.localPosition = directorOffset.GetBodyOffset(OriginBodyType); // 指定ボディを中心に調整
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
                    unitB.transform.localPosition = Vector3.zero; // 座標を親に合わせる
                    unitB.transform.rotation = Quaternion.Euler(Vector3.zero); // 向きを親に合わせる

                    //// ローカル座標を調整する
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
            // 存在すれば、指定されたディレクターにタイムラインをセット
            if (asset != null) director.playableAsset = asset;
            // directorにセットされてるアセットのtrackNameのトラックに引数のAnimatorをセット
            var binding = director.playableAsset.outputs.First(c => c.streamName == trackName);
            director.SetGenericBinding(binding.sourceObject, animator);
        }

        /// <summary>
        /// ユニットAを書き換える
        /// </summary>
        /// <param name="unit"></param>
        public void ChangeUnitA(GameObject unit)
        {
            UnitAComponents = unit.GetComponent<develop_common.UnitComponents>();

            // offsetどうする？キャラ大きいとか小さいとか
            UnitAComponents.transform.parent = PositionA.transform;//親を設定
            UnitAComponents.transform.localPosition = Vector3.zero; // 座標を親に合わせる
            UnitAComponents.transform.rotation = Quaternion.Euler(Vector3.zero); // 向きを親に合わせる
        }
        /// <summary>
        /// ユニットBを書き換える
        /// </summary>
        public void ChangeUnitB(GameObject unit)
        {
            UnitBComponents = unit.GetComponent<develop_common.UnitComponents>();

            // offsetどうする？キャラ大きいとか小さいとか
            UnitBComponents.transform.parent = PositionB.transform;//親を設定
            UnitBComponents.transform.localPosition = Vector3.zero; // 座標を親に合わせる
            UnitBComponents.transform.rotation = Quaternion.Euler(Vector3.zero); // 向きを親に合わせる
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
