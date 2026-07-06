using System;
using UnityEngine;

namespace CreateWithVR.Assignment
{
    /// <summary>
    /// Drives analog clock hands from the system clock in real time.
    /// Attach hour, minute, and second hand transforms and assign their rotation axes in the Inspector.
    /// </summary>
    public class AnalogWallClock : MonoBehaviour
    {
        [Header("Clock Hands")]
        [SerializeField] Transform m_HourHand;
        [SerializeField] Transform m_MinuteHand;
        [SerializeField] Transform m_SecondHand;

        [Header("Rotation")]
        [Tooltip("Local axis each hand rotates around (typically forward/Z for a wall-mounted clock).")]
        [SerializeField] Vector3 m_RotationAxis = Vector3.forward;

        [Tooltip("Offset applied to all hands so 12 o'clock aligns with your model.")]
        [SerializeField] float m_ZeroOffsetDegrees;

        [Header("Smoothing")]
        [SerializeField] bool m_SmoothSecondHand;
        [SerializeField] float m_SecondHandSmoothSpeed = 12f;

        float m_DisplayedSecondAngle;

        void Update()
        {
            var now = DateTime.Now;
            SetHandRotation(m_HourHand, GetHourAngle(now));
            SetHandRotation(m_MinuteHand, GetMinuteAngle(now));
            UpdateSecondHand(now);
        }

        static float GetHourAngle(DateTime time)
        {
            var hours = time.Hour % 12;
            return (hours + time.Minute / 60f + time.Second / 3600f) * 30f;
        }

        static float GetMinuteAngle(DateTime time)
        {
            return (time.Minute + time.Second / 60f + time.Millisecond / 60000f) * 6f;
        }

        static float GetSecondAngle(DateTime time)
        {
            return (time.Second + time.Millisecond / 1000f) * 6f;
        }

        void UpdateSecondHand(DateTime now)
        {
            if (m_SecondHand == null)
                return;

            var targetAngle = GetSecondAngle(now) + m_ZeroOffsetDegrees;

            if (m_SmoothSecondHand)
            {
                m_DisplayedSecondAngle = Mathf.LerpAngle(m_DisplayedSecondAngle, targetAngle, Time.deltaTime * m_SecondHandSmoothSpeed);
                ApplyRotation(m_SecondHand, m_DisplayedSecondAngle);
            }
            else
            {
                m_DisplayedSecondAngle = targetAngle;
                ApplyRotation(m_SecondHand, targetAngle);
            }
        }

        void SetHandRotation(Transform hand, float angle)
        {
            if (hand == null)
                return;

            ApplyRotation(hand, angle + m_ZeroOffsetDegrees);
        }

        void ApplyRotation(Transform hand, float angle)
        {
            hand.localRotation = Quaternion.AngleAxis(angle, m_RotationAxis.normalized);
        }

        void OnValidate()
        {
            m_RotationAxis = m_RotationAxis == Vector3.zero ? Vector3.forward : m_RotationAxis;
        }
    }
}
