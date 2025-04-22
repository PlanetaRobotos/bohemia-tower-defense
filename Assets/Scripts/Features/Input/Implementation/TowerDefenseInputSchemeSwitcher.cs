using Windows.Global;
using Features.Input.Declaration;
using State = Windows.Global.GameUI.State;

namespace Features.Input.Implementation
{
	/// <summary>
	///     TD Specific input switcher that also disables controls when the game is paused
	/// </summary>
	public class TowerDefenseInputSchemeSwitcher : InputSchemeSwitcher
    {
        [Inject] private GameUI m_GameUI;

        /// <summary>
        ///     Gets whether the game is in a paused state
        /// </summary>
        public bool isPaused => m_GameUI.state == State.Paused;

        /// <summary>
        ///     Register GameUI's stateChanged event
        /// </summary>
        protected virtual void Start()
        {
            m_GameUI.stateChanged += OnUIStateChanged;
        }

        /// <summary>
        ///     Do nothing when game is paused
        /// </summary>
        protected override void Update()
        {
            if (isPaused) return;

            base.Update();
        }

        /// <summary>
        ///     Unregister from GameUI's stateChanged event
        /// </summary>
        protected virtual void OnDestroy()
        {
            m_GameUI.stateChanged -= OnUIStateChanged;
        }

        /// <summary>
        ///     Activate or deactivate the current input scheme when the game pauses/unpauses
        /// </summary>
        private void OnUIStateChanged(State oldState, State newState)
        {
            if (m_CurrentScheme == null) return;
            if (newState == State.Paused)
                m_CurrentScheme.Deactivate(null);
            else
                m_CurrentScheme.Activate(null);
        }
    }
}