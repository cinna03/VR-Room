using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace CreateWithVR.Assignment
{
    /// <summary>
    /// Shows a world-space tooltip when the player hovers over an interactable object.
    /// </summary>
    [RequireComponent(typeof(XRBaseInteractable))]
    public class InteractableTooltip : MonoBehaviour
    {
        [SerializeField]
        TMP_Text m_TooltipText;

        [SerializeField]
        GameObject m_TooltipRoot;

        [SerializeField]
        [TextArea]
        string m_Message = "Pick me up";

        XRBaseInteractable m_Interactable;

        void Awake()
        {
            m_Interactable = GetComponent<XRBaseInteractable>();

            if (m_TooltipText != null)
                m_TooltipText.text = m_Message;

            SetTooltipVisible(false);
        }

        void OnEnable()
        {
            m_Interactable.hoverEntered.AddListener(OnHoverEntered);
            m_Interactable.hoverExited.AddListener(OnHoverExited);
        }

        void OnDisable()
        {
            m_Interactable.hoverEntered.RemoveListener(OnHoverEntered);
            m_Interactable.hoverExited.RemoveListener(OnHoverExited);
        }

        void OnHoverEntered(HoverEnterEventArgs args)
        {
            SetTooltipVisible(true);
        }

        void OnHoverExited(HoverExitEventArgs args)
        {
            SetTooltipVisible(false);
        }

        void SetTooltipVisible(bool visible)
        {
            if (m_TooltipRoot != null)
                m_TooltipRoot.SetActive(visible);
        }
    }
}
