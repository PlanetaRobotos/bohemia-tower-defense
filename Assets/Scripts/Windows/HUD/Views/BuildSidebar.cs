using Windows.Global;
using Windows.TowerWidget.Views;
using Features.Levels.Implementation;
using Features.Towers;
using UnityEngine;

namespace Windows.HUD.Views
{
	/// <summary>
	/// UI component that displays towers that can be built on this level.
	/// </summary>
	public class BuildSidebar : MonoBehaviour
	{
		[Inject] private LevelManager LevelManager { get; }
		[Inject] private GameUI _gameUI;
		/// <summary>
		/// The prefab spawned for each button
		/// </summary>
		public TowerSpawnButton towerSpawnButton;

		/// <summary>
		/// Initialize the tower spawn buttons
		/// </summary>
		protected virtual void Start()
		{
			if (!LevelManager)
			{
				Debug.LogError("[UI] No level manager for tower list");
			}
			foreach (Tower tower in LevelManager.towerLibrary)
			{
				TowerSpawnButton button = Instantiate(towerSpawnButton, transform);
				button.InitializeButton(tower);
				button.buttonTapped += OnButtonTapped;
				button.draggedOff += OnButtonDraggedOff;
			}
		}

		/// <summary>
		/// Sets the GameUI to build mode with the <see cref="towerData"/>
		/// </summary>
		/// <param name="towerData"></param>
		void OnButtonTapped(Tower towerData)
		{
			if (_gameUI.isBuilding)
			{
				_gameUI.CancelGhostPlacement();
			}
			_gameUI.SetToBuildMode(towerData);
		}

		/// <summary>
		/// Sets the GameUI to build mode with the <see cref="towerData"/> 
		/// </summary>
		/// <param name="towerData"></param>
		void OnButtonDraggedOff(Tower towerData)
		{
			if (!_gameUI.isBuilding)
			{
				_gameUI.SetToDragMode(towerData);
			}
		}

		/// <summary>
		/// Unsubscribes from all the tower spawn buttons
		/// </summary>
		void OnDestroy()
		{
			TowerSpawnButton[] childButtons = GetComponentsInChildren<TowerSpawnButton>();

			foreach (TowerSpawnButton towerButton in childButtons)
			{
				towerButton.buttonTapped -= OnButtonTapped;
				towerButton.draggedOff -= OnButtonDraggedOff;
			}
		}

		/// <summary>
		/// Called by start wave button in scene
		/// </summary>
		public void StartWaveButtonPressed()
		{
			if (LevelManager)
			{
				LevelManager.BuildingCompleted();
			}
		}

		/// <summary>
		/// Debug button to add currency
		/// </summary>
		/// <param name="amount">How much to add</param>
		public void AddCurrency(int amount)
		{
			if (LevelManager)
			{
				LevelManager.currency.AddCurrency(amount);
			}
		}
	}
}