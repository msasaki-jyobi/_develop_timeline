using Cinemachine;
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
        public Animator Player;

        [Header("現在再生が行われているPlayableDirector")]
        public PlayableDirector PlayingDirector;

        public event Action<string, string> StartEvent;
        public event Action<string, string> UpdatePlayableEvent;
        public event Action<string, string> FinishNamedEvent;
        public event Action FinishEvent;

        public GameObject UnitA;
        public GameObject UnitB;

        public bool IsCheckPlaying()
        {
            return PlayingDirector != null;
        }

        public bool SetPlayDirector(PlayableDirector director)
        {
            bool isCheck = true;

            if (PlayingDirector != null)
            {
                Debug.LogError($"既に${PlayingDirector.playableAsset} が実行中でした");
                isCheck = false;
            }

            PlayingDirector = director;

            // DirectorPlayerがアタッチされていれば
            if (PlayingDirector.gameObject.TryGetComponent<DirectorPlayer>(out var directorPlayer))
                foreach (var finishEvent in directorPlayer.FinishEventHandles)
                    StartEvent?.Invoke(finishEvent.EventName, finishEvent.EventValue); // Event Play

            return isCheck;
        }

        public void OnSetTimelineMessage(string message)
        {
            TextFadeController.Instance.UpdateMessageText(message);
        }

        public void FinishPlayable()
        {
            if (PlayingDirector != null)
            {
                if (PlayingDirector.gameObject.TryGetComponent<DirectorPlayer>(out var directorPlayer))
                {
                    if(UnitA != null)
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
                    Destroy(PlayingDirector.gameObject);

                    foreach (var finishEvent in directorPlayer.FinishEventHandles)
                        FinishNamedEvent?.Invoke(finishEvent.EventName, finishEvent.EventValue);

                }
                PlayingDirector = null;
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
        public void UpdatePlayableEventInvoke(string eventName, string value)
        {
            UpdatePlayableEvent?.Invoke(eventName, value);
            Debug.Log($"Update!! name:{eventName}, value:{value}");
        }


    }
}
