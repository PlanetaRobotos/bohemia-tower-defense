using System;
using Infrastructure.Services.ApplicationObservers.Runtime;
using UnityEngine;

namespace Features.Health.Core
{
	/// <summary>
	///     Class to visualizer the health of a damageable
	/// </summary>
	public class HealthVisualizer : MonoBehaviour
    {
	    [Inject] private IUpdater Updater { get; }

	    /// <summary>
	    ///     The DamageableBehaviour that will be used to assign the damageable
	    /// </summary>
	    [Tooltip("This field does not need to be populated here, it can be set up in code using AssignDamageable")]
        public DamageableBehaviour damageableBehaviour;

	    /// <summary>
	    ///     The object whose X-scale we change to decrease the health bar. Should have a default uniform scale
	    /// </summary>
	    public Transform healthBar;

	    /// <summary>
	    ///     The object whose X-scale we change to increase the health bar background. Should have a default uniform scale
	    /// </summary>
	    public Transform backgroundBar;

	    /// <summary>
	    ///     Whether to show this health bar even when it is full
	    /// </summary>
	    public bool showWhenFull;

	    /// <summary>
	    ///     Camera to face the visualization at
	    /// </summary>
	    protected Transform m_CameraToFace;

	    /// <summary>
	    ///     Damageable whose health is visualized
	    /// </summary>
	    protected Damageable m_Damageable;

	    /// <summary>
	    ///     Assigns a damageable if damageableBehaviour is populated
	    /// </summary>
	    protected virtual void Awake()
        {
            if (damageableBehaviour != null) AssignDamageable(damageableBehaviour.configuration);
            
            Updater.Subscribe(OnUpdate, 0);
        }

	    /// <summary>
	    ///     Caches the main camera
	    /// </summary>
	    protected virtual void Start()
        {
            m_CameraToFace = UnityEngine.Camera.main.transform;
        }

	    /// <summary>
	    ///     Turns us to face the camera
	    /// </summary>
	    protected virtual void OnUpdate(float _)
        {
            var direction = m_CameraToFace.transform.forward;
            transform.forward = -direction;
        }

	    /// <summary>
	    ///     Updates the visualization of the health
	    /// </summary>
	    /// <param name="normalizedHealth">Normalized health value</param>
	    public void UpdateHealth(float normalizedHealth)
        {
            var scale = Vector3.one;

            if (healthBar != null)
            {
                scale.x = normalizedHealth;
                healthBar.transform.localScale = scale;
            }

            if (backgroundBar != null)
            {
                scale.x = 1 - normalizedHealth;
                backgroundBar.transform.localScale = scale;
            }

            SetVisible(showWhenFull || normalizedHealth < 1.0f);
        }

	    /// <summary>
	    ///     Sets the visibility status of this visualiser
	    /// </summary>
	    public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

	    /// <summary>
	    ///     Assigns the damageable, subscribing to the damaged event
	    /// </summary>
	    /// <param name="damageable">Damageable to assign</param>
	    public void AssignDamageable(Damageable damageable)
        {
            if (m_Damageable != null) m_Damageable.healthChanged -= OnHealthChanged;
            m_Damageable = damageable;
            m_Damageable.healthChanged += OnHealthChanged;
        }

        private void OnHealthChanged(HealthChangeInfo healthChangeInfo)
        {
            UpdateHealth(m_Damageable.normalisedHealth);
        }

        private void OnDestroy()
        {
	        Updater?.Unsubscribe(OnUpdate);
        }
    }
}