using UnityEngine;

namespace CreateWithVR.Assignment
{
    /// <summary>
    /// Plays looping ambient audio with optional random one-shot effects for room atmosphere.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class AmbientAudioZone : MonoBehaviour
    {
        [SerializeField]
        AudioClip m_AmbientLoop;

        [SerializeField]
        AudioClip[] m_RandomOneShots;

        [SerializeField]
        Vector2 m_OneShotIntervalRange = new Vector2(12f, 28f);

        [SerializeField]
        float m_Volume = 0.35f;

        AudioSource m_Source;
        float m_NextOneShotTime;

        void Awake()
        {
            m_Source = GetComponent<AudioSource>();
            m_Source.loop = true;
            m_Source.spatialBlend = 1f;
            m_Source.volume = m_Volume;
            m_Source.clip = m_AmbientLoop;
            ScheduleNextOneShot();
        }

        void OnEnable()
        {
            if (m_AmbientLoop != null)
                m_Source.Play();
        }

        void Update()
        {
            if (m_RandomOneShots == null || m_RandomOneShots.Length == 0)
                return;

            if (Time.time < m_NextOneShotTime)
                return;

            var clip = m_RandomOneShots[Random.Range(0, m_RandomOneShots.Length)];
            if (clip != null)
                m_Source.PlayOneShot(clip, m_Volume * 0.8f);

            ScheduleNextOneShot();
        }

        void ScheduleNextOneShot()
        {
            m_NextOneShotTime = Time.time + Random.Range(m_OneShotIntervalRange.x, m_OneShotIntervalRange.y);
        }
    }
}
