using System.Collections;
using UnityEngine;

namespace develop_battle
{

    public class MoveAndAttach : MonoBehaviour
    {
        // Public variables
        public Transform TargetObject; // Bのオブジェクト
        public float MoveDuration = 1.0f; // A秒かけて移動
        public Vector3 FinalLocalPosition; // Cのローカル座標
        public Vector3 FinalLocalRotationEuler; // Dの回転値（Euluer）

        // Start is called before the first frame update
        void Start()
        {
            if (TargetObject != null)
            {
                // コルーチンを開始
                StartCoroutine(MoveToObjectAndAttach());
            }
            else
            {
                Debug.LogError("Target Object is not set.");
            }
        }

        private IEnumerator MoveToObjectAndAttach()
        {
            // 1. Bの方向を向く
            Vector3 directionToTarget = (TargetObject.position - transform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            transform.rotation = targetRotation;

            // 2. A秒かけてBの場所まで移動
            Vector3 startPosition = transform.position;
            float elapsedTime = 0f;

            while (elapsedTime < MoveDuration)
            {
                transform.position = Vector3.Lerp(startPosition, TargetObject.position, elapsedTime / MoveDuration);
                elapsedTime += Time.deltaTime;
                yield return null; // 次のフレームまで待機
            }

            // 最後の位置調整
            transform.position = TargetObject.position;

            // 3. Bのオブジェクトの子オブジェクトになる
            transform.SetParent(TargetObject);

            // 4. Cのローカル座標、Dの回転値（Euluer）に上書き
            transform.localPosition = FinalLocalPosition;
            transform.localRotation = Quaternion.Euler(FinalLocalRotationEuler);

            Debug.Log("Movement and attachment complete.");
        }
    }
}
