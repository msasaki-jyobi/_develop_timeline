using Cinemachine;
using Cysharp.Threading.Tasks;
using develop_common;
using develop_easymovie;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;

namespace develop_timeline
{
    public class DirectorManager : SingletonMonoBehaviour<DirectorManager>
    {
        public TextMeshProUGUI TimelineTextGUI;
        public CinemachineBrain Brain;
        public develop_common.UnitComponents PlayerComponents;

        [Header("現在再生が行われているPlayableDirector")]
        public PlayableDirector ActivePlayingDirector;

        public event Action<string, string> StartEvent;
        public event Action<EPlayableParamater, string> UpdatePlayableEvent;
        public event Action<string, string> FinishNamedEvent;
        public event Action FinishEvent;

        public GameObject UnitA;
        public GameObject UnitB;

        public int ThirdCount;

        [Header("FadeControllerを利用したNormalPlay用")]
        public float NormalFadeTime = 0.5f;
        public Color NormalPlayFadeColor;

        public bool IsCheckPlaying()
        {
            return ActivePlayingDirector != null;
        }

        public async void NormalPlayDirector(PlayableDirector director)
        {
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
                director.Play();
            else
                FadeController.Instance.ActionPlayFadeIn(() =>
                {
                    director.Play();
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
                    if (UnitA != null)
                    {
                        if (UnitA.TryGetComponent<Rigidbody>(out var rigidbody))
                            rigidbody.isKinematic = false;
                        UnitA.transform.parent = null;
                        UnitA = null;
                    }
                    if (UnitB != null)
                    {
                        if (UnitB.TryGetComponent<Rigidbody>(out var rigidbody))
                            rigidbody.isKinematic = false;
                        UnitB.transform.parent = null;
                        UnitB = null;
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
                Debug.LogError("実行中のDirectorはありません");
            }
        }
        /// <summary>
        /// Playble:Update Event
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="value"></param>
        public void UpdatePlayableEventInvoke(EPlayableParamater eventName, string value)
        {
            UpdatePlayableEvent?.Invoke(eventName, value);
            Debug.Log($"Update!! name:{eventName}, value:{value}");
        }


    }
}
