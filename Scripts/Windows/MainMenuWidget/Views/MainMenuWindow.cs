using UnityEngine;

namespace Windows.MainMenuWidget.Views
{
    /// <summary>
    /// Main menu implementation for tower defense
    /// </summary>
    public class MainMenuWindow : MainMenu
    {
        /// <summary>
        /// Reference to title menu
        /// </summary>
        public SimpleMainMenuPage titleMenu;

        /// <summary>
        /// Reference to level select menu
        /// </summary>
        public LevelSelectScreen levelSelectMenu;

        public override void OnOpen()
        {
        }

        /// <summary>
        /// Bring up the options menu
        /// </summary>
        public void ShowLevelSelectMenu()
        {
            ChangePage(levelSelectMenu);
        }

        /// <summary>
        /// Returns to the title screen
        /// </summary>
        public void ShowTitleScreen()
        {
            Back(titleMenu);
        }

        /// <summary>
        /// Set initial page
        /// </summary>
        protected virtual void Awake()
        {
            ShowTitleScreen();
            // ShowLevelSelectMenu();
        }

        /// <summary>
        /// Escape key input
        /// </summary>
        protected virtual void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
            {
                if ((SimpleMainMenuPage)m_CurrentPage == titleMenu)
                {
                    Application.Quit();
                }
                else
                {
                    Back();
                }
            }
        }
    }
}