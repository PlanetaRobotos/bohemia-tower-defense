using Infrastructure.Services.ApplicationObservers.Runtime;
using UnityEngine;

namespace Features.Input.Declaration
{
	/// <summary>
	///     Base component that switches between active input schemes
	/// </summary>
	[DisallowMultipleComponent]
    public class InputSchemeSwitcher : MonoBehaviour
    {
	    [Inject] private IUpdater Updater { get; }
		
	    /// <summary>
	    ///     The current scheme activated
	    /// </summary>
	    protected InputScheme m_CurrentScheme;

	    /// <summary>
	    ///     The default scheme based on the platform
	    /// </summary>
	    protected InputScheme m_DefaultScheme;

	    /// <summary>
	    ///     The attached input schemes
	    /// </summary>
	    protected InputScheme[] m_InputSchemes;

	    /// <summary>
	    ///     Cache the schemes and activate the default
	    /// </summary>
	    protected virtual void Awake()
        {
            m_InputSchemes = GetComponents<InputScheme>();
            foreach (var scheme in m_InputSchemes)
            {
                scheme.Deactivate(null);
                if (m_CurrentScheme == null && scheme.isDefault) m_DefaultScheme = scheme;
            }

            if (m_DefaultScheme == null)
            {
                Debug.LogError("[InputSchemeSwitcher] Default scheme not set.");
                return;
            }

            m_DefaultScheme.Activate(null);
            m_CurrentScheme = m_DefaultScheme;
            
            Updater.Subscribe(OnUpdate, 0);
        }

	    /// <summary>
	    ///     Checks the different schemes and activates them if needed
	    /// </summary>
	    protected virtual void OnUpdate(float _)
        {
            foreach (var scheme in m_InputSchemes)
            {
                if (scheme.enabled || !scheme.shouldActivate) continue;
                if (m_CurrentScheme != null) m_CurrentScheme.Deactivate(scheme);
                scheme.Activate(m_CurrentScheme);
                m_CurrentScheme = scheme;
                break;
            }
        }

        protected virtual void OnDestroy()
        {
            Updater?.Unsubscribe(OnUpdate);
        }
    }
}