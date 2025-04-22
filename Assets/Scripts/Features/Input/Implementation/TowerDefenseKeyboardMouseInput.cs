using Windows.Global;
using Features.Input.Models;
using Features.Levels.Implementation;
using UnityEngine;
using UnityInput = UnityEngine.Input;
using State = Windows.Global.GameUI.State;

namespace Features.Input.Implementation
{
    public class TowerDefenseKeyboardMouseInput : KeyboardMouseInput
    {
        [Inject] private GameUI _gameUI;
        [Inject] private InputController _inputController;
        [Inject] private LevelManager LevelManager { get; }

        /// <summary>
        ///     Handle camera panning behaviour
        /// </summary>
        protected override void OnUpdate(float _)
        {
            base.OnUpdate(_);

            // Escape handling
            if (UnityInput.GetKeyDown(KeyCode.Escape))
                switch (_gameUI.state)
                {
                    case State.Normal:
                        if (_gameUI.isTowerSelected)
                            _gameUI.DeselectTower();
                        else
                            _gameUI.Pause();
                        break;
                    case State.BuildingWithDrag:
                    case State.Building:
                        _gameUI.CancelGhostPlacement();
                        break;
                }

            // place towers with keyboard numbers
            var towerLibraryCount = LevelManager.towerLibrary.Count;

            // find the lowest value between 9 (keyboard numbers)
            // and the number of towers in the library
            var count = Mathf.Min(9, towerLibraryCount);

            // check each number key
            for (var i = 0; i < count; i++)
            {
                var key = KeyCode.Alpha1 + i;
                if (UnityInput.GetKeyDown(key))
                {
                    var controller = LevelManager.towerLibrary[key - KeyCode.Alpha1];
                    if (LevelManager.currency.CanAfford(controller.purchaseCost))
                    {
                        if (_gameUI.isBuilding) _gameUI.CancelGhostPlacement();
                        _gameUI.SetToBuildMode(controller);
                        _gameUI.TryMoveGhost(_inputController.basicMouseInfo);
                    }

                    break;
                }
            }

            // check for 0 key (10th tower)
            if (count < 10 && UnityInput.GetKeyDown(KeyCode.Alpha0))
            {
                var controller = LevelManager.towerLibrary[9];
                _gameUI.SetToBuildMode(controller);
                _gameUI.TryMoveGhost(_inputController.basicMouseInfo);
            }
        }

        /// <summary>
        ///     Register input events
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            _inputController.tapped += OnTap;
            _inputController.mouseMoved += OnMouseMoved;
        }

        /// <summary>
        ///     Deregister input events
        /// </summary>
        protected override void OnDisable()
        {
            _inputController.tapped -= OnTap;
            _inputController.mouseMoved -= OnMouseMoved;
        }

        /// <summary>
        ///     Ghost follows pointer
        /// </summary>
        private void OnMouseMoved(PointerInfo pointer)
        {
            // We only respond to mouse info
            var mouseInfo = pointer as MouseCursorInfo;

            if (mouseInfo != null && _gameUI.isBuilding) _gameUI.TryMoveGhost(pointer, false);
        }

        /// <summary>
        ///     Select towers or position ghosts
        /// </summary>
        private void OnTap(PointerActionInfo pointer)
        {
            // We only respond to mouse info
            var mouseInfo = pointer as MouseButtonInfo;

            if (mouseInfo != null && !mouseInfo.startedOverUI)
            {
                if (_gameUI.isBuilding)
                {
                    if (mouseInfo.mouseButtonId == 0) // LMB confirms
                        _gameUI.TryPlaceTower(pointer);
                    else // RMB cancels
                        _gameUI.CancelGhostPlacement();
                }
                else
                {
                    if (mouseInfo.mouseButtonId == 0)
                        // select towers
                        _gameUI.TrySelectTower(pointer);
                }
            }
        }
    }
}