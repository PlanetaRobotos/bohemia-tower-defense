using System.Collections.Generic;
using Features.Levels.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Windows.MainMenuWidget.Views
{
	/// <summary>
	/// A manager for the level select user interface
	/// </summary>
	public class LevelSelectScreen : SimpleMainMenuPage
	{
		/// <summary>
		/// The button to instantiate that 
		/// represents the level select buttons
		/// </summary>
		public LevelSelectButton selectionPrefab;

		/// <summary>
		/// The layout group to instantiate the buttons in
		/// </summary>
		public LayoutGroup layout;

		/// <summary>
		/// A buffer for the levels panel
		/// </summary>
		public Transform rightBuffer;

		public Button backButton;

		public MouseScroll mouseScroll;
		
		/// <summary>
		/// The reference to the list of levels to display
		/// </summary>
		protected LevelList m_LevelList;
		
		protected List<Button> m_Buttons = new List<Button>();

		/// <summary>
		/// Instantiate the buttons
		/// </summary>
		protected virtual void Start()
		{
			if (!_gameManager)
			{
				return;
			}

			m_LevelList = _gameManager.levelList;
			if (layout == null || selectionPrefab == null || m_LevelList == null)
			{
				return;
			}

			int amount = m_LevelList.Count;
			for (int i = 0; i < amount; i++)
			{
				LevelSelectButton button = CreateButton(m_LevelList[i]);
				button.transform.SetParent(layout.transform);
				button.transform.localScale = Vector3.one;
				m_Buttons.Add(button.GetComponent<Button>());
			}
			
			if (rightBuffer != null)
			{
				rightBuffer.SetAsLastSibling();
			}

			for (int index = 1; index < m_Buttons.Count - 1; index++)
			{
				Button button = m_Buttons[index];
			}
			
			mouseScroll.SetHasRightBuffer(rightBuffer != null);
			
			var rt = layout.GetComponent<RectTransform>();
			LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
			canvas.enabled = false;
		}

		/// <summary>
		/// Create and Initialise a Level select button based on item
		/// </summary>
		/// <param name="item">
		/// The level data
		/// </param>
		/// <returns>
		/// The initialised button
		/// </returns>
		protected LevelSelectButton CreateButton(LevelItem item)
		{
			LevelSelectButton button = Instantiate(selectionPrefab);
			button.Initialize(item, mouseScroll);
			return button;
		}

		/// <summary>
		/// Sets up the navigation for a selectable
		/// </summary>
		/// <param name="selectable">Selectable to set up</param>
		/// <param name="left">Select on left</param>
		/// <param name="right">Select on right</param>
		void SetUpNavigation(Selectable selectable, Selectable left, Selectable right)
		{
			Navigation navigation = selectable.navigation;
			navigation.selectOnLeft = left;
			navigation.selectOnRight = right;
			selectable.navigation = navigation;
		}
		
	}
}