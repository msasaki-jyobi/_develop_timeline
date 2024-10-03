using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace develop_timeline
{
    public class DirectorManager : SingletonMonoBehaviour<DirectorManager>
    {
        [Header("現在再生が行われているPlayableDirector")]
        public PlayableDirector PlayingDirector;

        public event Action<string, string> StartEvent;
        public event Action<string, string> FinishEvent;

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

            //Event Play
            if (PlayingDirector.gameObject.TryGetComponent<DirectorPlayer>(out var directorPlayer))
                foreach (var finishEvent in directorPlayer.FinishEventHandles)
                    StartEvent?.Invoke(finishEvent.EventName, finishEvent.EventValue);

            return isCheck;
        }

        public void FinishPlayable()
        {
            if (PlayingDirector != null)
            {
                if (PlayingDirector.gameObject.TryGetComponent<DirectorPlayer>(out var directorPlayer))
                {
                    directorPlayer.OnPlayFinish();
                    Destroy(PlayingDirector.gameObject);
                    PlayingDirector = null;

                    foreach (var finishEvent in directorPlayer.FinishEventHandles)
                        FinishEvent?.Invoke(finishEvent.EventName, finishEvent.EventValue);
                }
            }
            else
            {
                Debug.LogError("実行中のDirectorはありません");
            }
        }

        public void OnCollStartEvent(string eventName, string value)
        {
            StartEvent(eventName, value);
        }
        public void OnCollFinishEvent(string eventName, string value)
        {
            FinishEvent(eventName, value);
        }
    }
}
