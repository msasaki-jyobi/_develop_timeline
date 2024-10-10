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

        [Header("åªç›çƒê∂Ç™çsÇÌÇÍÇƒÇ¢ÇÈPlayableDirector")]
        public PlayableDirector PlayingDirector;

        public event Action<string, string> StartEvent;
        public event Action<string, string> UpdatePlayableEvent;
        public event Action<string, string> FinishEvent;

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
                Debug.LogError($"ä˘Ç…${PlayingDirector.playableAsset} Ç™é¿çsíÜÇ≈ÇµÇΩ");
                isCheck = false;
            }

            PlayingDirector = director;

            //Event Play
            if (PlayingDirector.gameObject.TryGetComponent<DirectorPlayer>(out var directorPlayer))
                foreach (var finishEvent in directorPlayer.FinishEventHandles)
                    StartEvent?.Invoke(finishEvent.EventName, finishEvent.EventValue);

            return isCheck;
        }

        public void OnSetTimelineMessage(string message)
        {
            TimelineTextGUI.text = message;
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
                    PlayingDirector = null;
                    TimelineTextGUI.text = "";

                    foreach (var finishEvent in directorPlayer.FinishEventHandles)
                        FinishEvent?.Invoke(finishEvent.EventName, finishEvent.EventValue);
                }
            }
            else
            {
                Debug.LogError("é¿çsíÜÇÃDirectorÇÕÇ†ÇËÇ‹ÇπÇÒ");
            }
        }

        public void OnCollStartEvent(string eventName, string value)
        {
            StartEvent?.Invoke(eventName, value);
        }
        public void OnCollFinishEvent(string eventName, string value)
        {
            FinishEvent?.Invoke(eventName, value);
        }

        public void UpdatePlayableEventInvoke(string eventName, string value)
        {
            UpdatePlayableEvent?.Invoke(eventName, value);
            Debug.Log($"Update!! name:{eventName}, value:{value}");
        }
    }
}
