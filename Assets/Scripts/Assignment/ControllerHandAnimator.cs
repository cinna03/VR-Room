using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;

namespace CreateWithVR.Assignment
{
    /// <summary>
    /// Replaces default controller visuals with a rigged hand model whose fingers curl
    /// in response to grip and trigger controller input.
    /// </summary>
    [AddComponentMenu("XR/Controller Hand Animator")]
    public class ControllerHandAnimator : MonoBehaviour
    {
        public enum HandSide
        {
            Left,
            Right
        }

        [Header("Hand")]
        [SerializeField]
        HandSide m_HandSide = HandSide.Right;

        [SerializeField]
        [Tooltip("Root transform of the imported hand model.")]
        Transform m_HandRoot;

        [Header("Input")]
        [SerializeField]
        XRInputValueReader<float> m_GripInput = new XRInputValueReader<float>("Grip");

        [SerializeField]
        XRInputValueReader<float> m_TriggerInput = new XRInputValueReader<float>("Trigger");

        [Header("Animation")]
        [SerializeField]
        Vector3 m_CurlAxis = Vector3.right;

        [SerializeField]
        float m_GripCurlAngle = 75f;

        [SerializeField]
        float m_TriggerCurlAngle = 65f;

        [SerializeField]
        float m_ThumbCurlAngle = 35f;

        [SerializeField]
        float m_AnimationSpeed = 14f;

        readonly Dictionary<Transform, Quaternion> m_RestLocalRotations = new Dictionary<Transform, Quaternion>();
        Transform[] m_IndexFingerJoints;
        Transform[] m_MiddleFingerJoints;
        Transform[] m_RingFingerJoints;
        Transform[] m_LittleFingerJoints;
        Transform[] m_ThumbJoints;
        string m_Prefix;
        float m_CurrentGrip;
        float m_CurrentTrigger;

        void Awake()
        {
            m_Prefix = m_HandSide == HandSide.Left ? "L_" : "R_";
            CacheFingerJoints();
        }

        void OnEnable()
        {
            m_GripInput?.EnableDirectActionIfModeUsed();
            m_TriggerInput?.EnableDirectActionIfModeUsed();
        }

        void OnDisable()
        {
            m_GripInput?.DisableDirectActionIfModeUsed();
            m_TriggerInput?.DisableDirectActionIfModeUsed();
        }

        void Update()
        {
            if (m_HandRoot == null)
                return;

            var targetGrip = m_GripInput != null ? m_GripInput.ReadValue() : 0f;
            var targetTrigger = m_TriggerInput != null ? m_TriggerInput.ReadValue() : 0f;

            m_CurrentGrip = Mathf.Lerp(m_CurrentGrip, targetGrip, Time.deltaTime * m_AnimationSpeed);
            m_CurrentTrigger = Mathf.Lerp(m_CurrentTrigger, targetTrigger, Time.deltaTime * m_AnimationSpeed);

            ApplyFingerCurl(m_IndexFingerJoints, m_CurrentGrip, m_CurrentTrigger, m_GripCurlAngle, m_TriggerCurlAngle);
            ApplyFingerCurl(m_MiddleFingerJoints, m_CurrentGrip, 0f, m_GripCurlAngle, 0f);
            ApplyFingerCurl(m_RingFingerJoints, m_CurrentGrip, 0f, m_GripCurlAngle, 0f);
            ApplyFingerCurl(m_LittleFingerJoints, m_CurrentGrip, 0f, m_GripCurlAngle, 0f);
            ApplyFingerCurl(m_ThumbJoints, m_CurrentGrip, 0f, m_ThumbCurlAngle, 0f);
        }

        void CacheFingerJoints()
        {
            var root = m_HandRoot != null ? m_HandRoot : transform;
            m_IndexFingerJoints = GetFingerChain(root, "Index");
            m_MiddleFingerJoints = GetFingerChain(root, "Middle");
            m_RingFingerJoints = GetFingerChain(root, "Ring");
            m_LittleFingerJoints = GetFingerChain(root, "Little");
            m_ThumbJoints = GetFingerChain(root, "Thumb");

            CacheRestRotations(m_IndexFingerJoints);
            CacheRestRotations(m_MiddleFingerJoints);
            CacheRestRotations(m_RingFingerJoints);
            CacheRestRotations(m_LittleFingerJoints);
            CacheRestRotations(m_ThumbJoints);
        }

        Transform[] GetFingerChain(Transform root, string fingerName)
        {
            var jointNames = new[]
            {
                $"{m_Prefix}{fingerName}Metacarpal",
                $"{m_Prefix}{fingerName}Proximal",
                $"{m_Prefix}{fingerName}Intermediate",
                $"{m_Prefix}{fingerName}Distal"
            };

            var joints = new List<Transform>();
            foreach (var jointName in jointNames)
            {
                var joint = FindDeepChild(root, jointName);
                if (joint != null)
                    joints.Add(joint);
            }

            return joints.ToArray();
        }

        static Transform FindDeepChild(Transform parent, string name)
        {
            if (parent.name == name)
                return parent;

            for (var i = 0; i < parent.childCount; i++)
            {
                var result = FindDeepChild(parent.GetChild(i), name);
                if (result != null)
                    return result;
            }

            return null;
        }

        void CacheRestRotations(IEnumerable<Transform> joints)
        {
            foreach (var joint in joints)
            {
                if (joint != null && !m_RestLocalRotations.ContainsKey(joint))
                    m_RestLocalRotations[joint] = joint.localRotation;
            }
        }

        void ApplyFingerCurl(IReadOnlyList<Transform> joints, float gripAmount, float triggerAmount, float gripAngle, float triggerAngle)
        {
            if (joints == null || joints.Count == 0)
                return;

            var totalAngle = gripAmount * gripAngle + triggerAmount * triggerAngle;
            if (totalAngle <= 0.001f)
            {
                ResetFinger(joints);
                return;
            }

            var segmentCount = joints.Count;
            for (var i = 0; i < segmentCount; i++)
            {
                var joint = joints[i];
                if (joint == null || !m_RestLocalRotations.TryGetValue(joint, out var restRotation))
                    continue;

                var weight = (i + 1) / (float)segmentCount;
                var curl = Quaternion.AngleAxis(-totalAngle * weight, m_CurlAxis.normalized);
                joint.localRotation = restRotation * curl;
            }
        }

        void ResetFinger(IReadOnlyList<Transform> joints)
        {
            foreach (var joint in joints)
            {
                if (joint != null && m_RestLocalRotations.TryGetValue(joint, out var restRotation))
                    joint.localRotation = restRotation;
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Recache Finger Joints")]
        void RecacheFingerJoints()
        {
            m_RestLocalRotations.Clear();
            CacheFingerJoints();
        }
#endif
    }
}
