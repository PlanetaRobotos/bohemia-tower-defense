using System.Globalization;
using Features.Health.Core;
using Features.Levels.Implementation;
using UnityEngine;

namespace Windows.HUD.Views
{
	/// <summary>
	/// A simple implementation of UI for player base health
	/// </summary>
	public class PlayerBaseHealth : MonoBehaviour
	{
		[Inject] private LevelManager LevelManager { get; }
		
		/// <summary>
		/// The text element to display information on
		/// </summary>
		public TMPro.TMP_Text display;

		/// <summary>
		/// The highest health that the base can go to
		/// </summary>
		protected float m_MaxHealth;

		/// <summary>
		/// Get the max health of the player base
		/// </summary>
		protected virtual void Start()
		{
			if (LevelManager == null)
			{
				return;
			}
			if (LevelManager.numberOfHomeBases > 0)
			{
				Damageable baseConfig = LevelManager.playerHomeBases[0].configuration;
				baseConfig.damaged += OnBaseDamaged;
				float currentHealth = baseConfig.currentHealth;
				float noramlisedHealth = baseConfig.normalisedHealth;
				m_MaxHealth = currentHealth / noramlisedHealth;
			}
			UpdateDisplay();
		}

		/// <summary>
		/// Subscribes to the player base health died event
		/// </summary>
		/// <param name="info">
		/// The associated health change information
		/// </param>
		protected virtual void OnBaseDamaged(HealthChangeInfo info)
		{
			UpdateDisplay();
		}

		/// <summary>
		/// Get the current health of the home base and display it on m_Display
		/// </summary>
		protected void UpdateDisplay()
		{
			if (LevelManager == null)
			{
				return;
			}
			float currentHealth = LevelManager.GetAllHomeBasesHealth();
			display.text = currentHealth.ToString(CultureInfo.InvariantCulture);
		}
	}
}