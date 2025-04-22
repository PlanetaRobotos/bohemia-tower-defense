using Features.Economy;
using Features.Levels.Implementation;
using UnityEngine;

namespace Windows.HUD.Views
{
	/// <summary>
	/// A class for controlling the displaying the currency
	/// </summary>
	public class CurrencyUI : MonoBehaviour
	{
		[Inject] private LevelManager LevelManager { get; }

		/// <summary>
		/// The text element to display information on
		/// </summary>
		public TMPro.TMP_Text display;

		/// <summary>
		/// The currency prefix to display next to the amount
		/// </summary>
		public string currencySymbol = "$";

		protected Currency m_Currency;

		/// <summary>
		/// Assign the correct currency value
		/// </summary>
		protected virtual void Start()
		{
			if (LevelManager)
			{
				m_Currency = LevelManager.currency;

				UpdateDisplay();
				m_Currency.currencyChanged += UpdateDisplay;
			}
			else
			{
				Debug.LogError("[UI] No level manager to get currency from");
			}
		}

		/// <summary>
		/// Unsubscribe from events
		/// </summary>
		protected virtual void OnDestroy()
		{
			if (m_Currency != null)
			{
				m_Currency.currencyChanged -= UpdateDisplay;
			}
		}

		/// <summary>
		/// A method for updating the display based on the current currency
		/// </summary>
		protected void UpdateDisplay()
		{
			int current = m_Currency.currentCurrency;
			display.text = current.ToString();
		}
	}
}