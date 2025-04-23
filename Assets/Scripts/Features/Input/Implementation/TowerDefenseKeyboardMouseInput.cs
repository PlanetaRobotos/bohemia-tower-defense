using Windows.Global;
using Features.Input.Models;
using Features.Levels.Implementation;
using UnityEngine;
using UnityInput = UnityEngine.Input;
using State = Windows.Global.GameUI.State;
using Infrastructure.Services.ApplicationObservers.Runtime;

namespace Features.Input.Implementation
{
    public class TowerDefenseKeyboardMouseInput : KeyboardMouseInput
    {
        [Inject] private GameUI _gameUI;
        [Inject] private InputController _inputController;
        [Inject] private LevelManager LevelManager { get; }

        /// <summary>
        ///     Main update loop: handle camera, ghost movement, escape, and build hotkeys
        /// </summary>
        protected override void Update()
        {
            base.Update();

            // Always drive ghost movement per-frame when in build mode
            if (_gameUI.isBuilding)
            {
                _gameUI.TryMoveGhost(_inputController.basicMouseInfo, false);
            }

            // Escape key handling
            if (UnityInput.GetKeyDown(KeyCode.Escape))
            {
                switch (_gameUI.state)
                {
                    case State.Normal:
                        if (_gameUI.isTowerSelected)
                            _gameUI.DeselectTower();
                        else
                            _gameUI.Pause();
                        break;
                    case State.Building:
                    case State.BuildingWithDrag:
                        _gameUI.CancelGhostPlacement();
                        break;
                }
            }

            // Tower build hotkeys (1-9, 0 as 10)
            int maxKeys = Mathf.Min(9, LevelManager.towerLibrary.Count);
            for (int i = 0; i < maxKeys; i++)
            {
                if (UnityInput.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    var towerPrefab = LevelManager.towerLibrary[i];
                    if (LevelManager.currency.CanAfford(towerPrefab.purchaseCost))
                    {
                        if (_gameUI.isBuilding)
                        {
                            _gameUI.CancelGhostPlacement();
                        }
                        _gameUI.SetToBuildMode(towerPrefab);
                    }
                    break;
                }
            }
            // 0 key as 10th
            if (LevelManager.towerLibrary.Count >= 10 && UnityInput.GetKeyDown(KeyCode.Alpha0))
            {
                var towerPrefab = LevelManager.towerLibrary[9];
                _gameUI.SetToBuildMode(towerPrefab);
            }
        }

        /// <summary>
        ///     Register tap events
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();
            _inputController.tapped += OnTap;
        }

        /// <summary>
        ///     Deregister tap events
        /// </summary>
        protected override void OnDisable()
        {
            _inputController.tapped -= OnTap;
            base.OnDisable();
        }

        /// <summary>
        ///     Handle click/tap for tower placement and selection
        /// </summary>
        private void OnTap(PointerActionInfo pointer)
        {
            // ignore UI hits
            var mb = pointer as MouseButtonInfo;
            if (mb == null || mb.startedOverUI) return;

            if (_gameUI.isBuilding)
            {
                // LMB to confirm, RMB to cancel
                if (mb.mouseButtonId == 0)
                {
                    _gameUI.TryPlaceTower(pointer);
                }
                else if (mb.mouseButtonId == 1)
                {
                    _gameUI.CancelGhostPlacement();
                }
            }
            else // Normal state: select tower on LMB
            {
                if (mb.mouseButtonId == 0)
                {
                    _gameUI.TrySelectTower(pointer);
                }
            }
        }
    }
}
