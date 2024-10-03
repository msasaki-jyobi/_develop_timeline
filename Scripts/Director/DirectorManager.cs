using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace develop_timeline
{
    public class DirectorManager : SingletonMonoBehaviour<DirectorManager>
    {
        [Header("åªç›çƒê∂Ç™çsÇÌÇÍÇƒÇ¢ÇÈPlayableDirector")]
        public PlayableDirector PlayingDirector;

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
            return isCheck;
        }

        public void FinishPlayable()
        {
            if(PlayingDirector != null)
            {
                if(PlayingDirector.gameObject.TryGetComponent<DirectorPlayer>(out var directorPlayer))
                {
                    directorPlayer.OnPlayFinish();
                    Destroy(PlayingDirector.gameObject);
                    PlayingDirector = null;
                }
            }
            else
            {
                Debug.LogError("é¿çsíÜÇÃDirectorÇÕÇ†ÇËÇ‹ÇπÇÒ");
            }

      
        }
    }
}
