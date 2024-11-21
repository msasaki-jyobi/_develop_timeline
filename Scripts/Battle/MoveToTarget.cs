using System.Collections;
using UnityEngine;

namespace develop_battle
{

    public class MoveAndAttach : MonoBehaviour
    {
        // Public variables
        public Transform TargetObject; // B�̃I�u�W�F�N�g
        public float MoveDuration = 1.0f; // A�b�����Ĉړ�
        public Vector3 FinalLocalPosition; // C�̃��[�J�����W
        public Vector3 FinalLocalRotationEuler; // D�̉�]�l�iEuluer�j

        // Start is called before the first frame update
        void Start()
        {
            if (TargetObject != null)
            {
                // �R���[�`�����J�n
                StartCoroutine(MoveToObjectAndAttach());
            }
            else
            {
                Debug.LogError("Target Object is not set.");
            }
        }

        private IEnumerator MoveToObjectAndAttach()
        {
            // 1. B�̕���������
            Vector3 directionToTarget = (TargetObject.position - transform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            transform.rotation = targetRotation;

            // 2. A�b������B�̏ꏊ�܂ňړ�
            Vector3 startPosition = transform.position;
            float elapsedTime = 0f;

            while (elapsedTime < MoveDuration)
            {
                transform.position = Vector3.Lerp(startPosition, TargetObject.position, elapsedTime / MoveDuration);
                elapsedTime += Time.deltaTime;
                yield return null; // ���̃t���[���܂őҋ@
            }

            // �Ō�̈ʒu����
            transform.position = TargetObject.position;

            // 3. B�̃I�u�W�F�N�g�̎q�I�u�W�F�N�g�ɂȂ�
            transform.SetParent(TargetObject);

            // 4. C�̃��[�J�����W�AD�̉�]�l�iEuluer�j�ɏ㏑��
            transform.localPosition = FinalLocalPosition;
            transform.localRotation = Quaternion.Euler(FinalLocalRotationEuler);

            Debug.Log("Movement and attachment complete.");
        }
    }
}
