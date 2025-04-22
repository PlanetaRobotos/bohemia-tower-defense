using Features.Economy.Models;
using UnityEngine;
using Infrastructure.Services.ApplicationObservers.Runtime;

namespace Features.Affectors
{
	/// <summary>
	///     A tower effect for generating currency
	/// </summary>
	public class CurrencyAffector : Affector
    {
	    /// <summary>
	    ///     The controller for currency gain
	    /// </summary>
	    public CurrencyGainer currencyGainer;

	    /// <summary>
	    ///     Format for displaying the the properties of this affector
	    /// </summary>
	    public string descriptionFormat = "<b>Produces</b> {1} at {2} units per second";

	    [Inject] private IUpdater Updater { get; }

	    /// <summary>
	    ///     Initialize the currency gain
	    /// </summary>
	    protected virtual void Start()
        {
            currencyGainer.Initialize(levelManager.currency);
            Updater.Subscribe(OnUpdate, 0);
        }

	    /// <summary>
	    ///     Update the currency gain
	    /// </summary>
	    protected virtual void OnUpdate(float _)
        {
            currencyGainer.Tick(Time.deltaTime);
        }

	    /// <summary>
	    ///     Subscribe to currency gain events
	    /// </summary>
	    protected virtual void OnEnable()
        {
            currencyGainer.currencyChanged += OnCurrencyChanged;
        }

	    /// <summary>
	    ///     Unsubscribe to currency gain event
	    /// </summary>
	    protected virtual void OnDisable()
        {
            currencyGainer.currencyChanged -= OnCurrencyChanged;
        }

	    /// <summary>
	    ///     Fires when currency changed in <see cref="currencyGainer" />
	    /// </summary>
	    /// <param name="info">
	    ///     The info for the currency gainer
	    /// </param>
	    protected void OnCurrencyChanged(CurrencyChangeInfo info)
        {
            Debug.Log($"Currency: {info}");
        }

	    protected virtual void OnDestroy()
        {
            Updater?.Unsubscribe(OnUpdate);
        }
    }
}