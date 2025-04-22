using Constellation.SceneManagement;
using Constellation.SceneManagement.Manager;
using Features.Data.Implementation;
using Features.Levels.Data;
using GameConstants;
using GameStates;
using Infrastructure.DI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using WindowsSystem.Core.Managers;

namespace Windows.MainMenuWidget.Views
{
    /// <summary>
    /// The button for selecting a level
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class LevelSelectButton : MonoBehaviour, ISelectHandler
    {
        [Inject] private GameManager _gameManager;
        [Inject] private readonly ApplicationStateMachine _stateMachine;
        [Inject] private readonly IScenesManager _scenesManager;
        [Inject] private readonly WindowsController _windowsController;

        /// <summary>
        /// Reference to the required button component
        /// </summary>
        protected Button m_Button;

        /// <summary>
        /// The UI text element that displays the name of the level
        /// </summary>
        public TMPro.TMP_Text titleDisplay;

        public TMPro.TMP_Text description;

        public Sprite starAchieved;

        public Image[] stars;

        protected MouseScroll m_MouseScroll;

        /// <summary>
        /// The data concerning the level this button displays
        /// </summary>
        protected LevelItem m_Item;

        /// <summary>
        /// When the user clicks the button, change the scene
        /// </summary>
        public void ButtonClicked()
        {
            ChangeScenes();
        }

        /// <summary>
        /// A method for assigning the data from item to the button
        /// </summary>
        /// <param name="item">
        /// The data with the information concerning the level
        /// </param>
        public void Initialize(LevelItem item, MouseScroll mouseScroll)
        {
            LazyLoad();
            if (titleDisplay == null)
            {
                return;
            }

            m_Item = item;
            titleDisplay.text = item.name;
            description.text = item.description;
            HasPlayedState();
            m_MouseScroll = mouseScroll;
        }

        /// <summary>
        /// Configures the feedback concerning if the player has played
        /// </summary>
        protected void HasPlayedState()
        {
            if (!_gameManager)
            {
                return;
            }

            int starsForLevel = _gameManager.GetStarsForLevel(m_Item.id);
            for (int i = 0; i < starsForLevel; i++)
            {
                stars[i].sprite = starAchieved;
            }
        }

        /// <summary>
        /// Changes the scene to the scene name provided by m_Item
        /// </summary>
        protected void ChangeScenes()
        {
            // StartCoroutine(LoadSceneAsync(m_Item.sceneName));

            _windowsController.TryGetWindowById(WindowsConstants.MAIN_MENU_WINDOW, out var window);
            window.Close();
            
            _stateMachine.Enter<GameplayState, string>(m_Item.sceneName);
        }

        // private IEnumerator LoadSceneAsync(string sceneName)
        // {
        // 	AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        // 	asyncLoad.allowSceneActivation = false;
        // 	while (asyncLoad.progress < 0.9f) yield return null;
        // 	asyncLoad.allowSceneActivation = true;
        // 	while (!asyncLoad.isDone) yield return null;
        // 	
        // }

        /// <summary>
        /// Ensure <see cref="m_Button"/> is not null
        /// </summary>
        protected void LazyLoad()
        {
            if (m_Button == null)
            {
                m_Button = GetComponent<Button>();
            }
        }

        /// <summary>
        /// Remove all listeners on the button before destruction
        /// </summary>
        protected void OnDestroy()
        {
            if (m_Button != null)
            {
                m_Button.onClick.RemoveAllListeners();
            }
        }

        /// <summary>
        /// Implementation of ISelectHandler
        /// </summary>
        /// <param name="eventData">Select event data</param>
        public void OnSelect(BaseEventData eventData)
        {
            m_MouseScroll.SelectChild(this);
        }
    }
}