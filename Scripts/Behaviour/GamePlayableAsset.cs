using Cinemachine;
using develop_common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace develop_timeline
{

    [System.Serializable]
    public class GamePlayableAsset : PlayableAsset
    {
        public List<StringEventHandle> StringEvent = new List<StringEventHandle>();

        // タイムライン中に実行したい内容を変数として登録
        [Header("音声の再生")]
        public List<develop_common.ClipData> SEClip;
        public List<develop_common.ClipData> VoiceClip;
        public string PlayVoiceID;

        [Header("Player：Shape")]
        public ShapeWordData SetShapeWordData;

        [Header("実行するイベント名")]
        public string EventName = "";
        public string EventValue = "";

        [Header("InstanceManager：IDを指定してオブジェクト生成")]
        public string PositionInstanceID;
        public GameObject Prefab;
        public bool IsParent;
        public Vector3 SetOffsetPos;
        public Vector3 SetRotation;

        [Header("TextManager"), Multiline]
        public string Message;

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