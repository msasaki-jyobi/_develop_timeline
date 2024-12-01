using Cinemachine;
using develop_common;
using develop_timeline;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace develop_timeline
{
    public enum EPlayableParamater
    {
        None,
        PlayDirector,
        AnimatorStateMotion,
        AdditiveMotion,
        Shape,
        CameraChange,
        Damage,
        Dead,
        ChangeInstanceShape,
        ChangeInstanceObject,
        AddShape,
        ThirdPlayDirector,
        SelectWaveTalk,
    }

    [System.Serializable]
    public class GamePlayableAsset : PlayableAsset
    {
        // タイムライン中に実行したい内容を変数として登録
        [Header("音声の再生")]
        public List<develop_common.ClipData> SEClip;
        public List<develop_common.ClipData> VoiceClip;
        public string PlayVoiceID;

        [Header("Shape")]
        public List<TimelineSetShape> TimelineSetShape = new List<TimelineSetShape>();

        [Header("実行するイベント")]
        public List<TimelineSetEvent> TimelineSetEvents = new List<TimelineSetEvent>();

        [Header("InstanceManager：IDを指定してオブジェクト生成")]
        public string InstanceKeyName;
        public GameObject Prefab;
        public bool IsParent;
        public Vector3 SetOffsetPos;
        public Vector3 SetRotation;

        [Header("TextManager"), Multiline]
        public string Message;
        [Header("FadeLoadScene")]
        public string FadeLoadSceneName = "";

        public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
        {
            //今回作成するもう1つのクラス
            GamePlayableBehaviour behaviour = new GamePlayableBehaviour();

            //cubeObjectというプロパティの準備、CreatePlayableの引数で渡されるオブジェクトの設定
            behaviour.gamePlayableAsset = this;
            behaviour.playablePlayer = go;

            //「ScriptPlayable」クラスの「Create」メソッドの呼び出し
            //引数に「CreatePlayable」メソッドの引数で渡された「PlayablGraph」と、「CubePlayableBehaviour」を指定
            ScriptPlayable<GamePlayableBehaviour> playable = ScriptPlayable<GamePlayableBehaviour>.Create(graph, behaviour);

            return playable;
        }
    }
}

[System.Serializable]
public class TimelineSetShape
{
    public string ShapeUnitName;
    public develop_common.ShapeWordData SetShapeWordData;
}
[System.Serializable]
public class TimelineSetEvent
{
    public EPlayableParamater EventName;
    public string EventValue = "";
}