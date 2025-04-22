using Windows.Global;
using Windows.TowerWidget.Views;
using UnityEngine;
using WindowsSystem.Core;

namespace Windows.HUD.Views
{
    public class HUDWindow : BaseWindow
    {
        [Inject] private GameUI GameUI { get; }

        [SerializeField] private MovingCanvas _confirmationButtons;
        [SerializeField] private MovingCanvas _invalidButtons;

        public MovingCanvas ConfirmationButtons => _confirmationButtons;

        public MovingCanvas InvalidButtons => _invalidButtons;

        public override void OnOpen()
        {
        }

        public void PauseGame()
        {
            GameUI.Pause();
            
        }

        public override void Close()
        {
            base.Close();
            
            
        }
    }
}