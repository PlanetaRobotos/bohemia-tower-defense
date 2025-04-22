using Windows.Global;
using Windows.HUD.Views;
using Windows.TowerWidget.Views;
using Features.Input.Declaration;
using Features.Input.Models;
using UnityEngine;
using UnityInput = UnityEngine.Input;
using State = Windows.Global.GameUI.State;

namespace Features.Input.Implementation
{
    public class TowerDefenseTouchInput : TouchInput
    {
	    /// <summary>
	    ///     A percentage of the screen where panning occurs while dragging
	    /// </summary>
	    [Range(0, 0.5f)] public float panAreaScreenPercentage = 0.2f;

	    /// <summary>
	    ///     The object that holds the confirmation buttons
	    /// </summary>
	    private MovingCanvas _confirmationButtons;

	    /// <summary>
	    ///     The attached Game UI object
	    /// </summary>
	    [Inject] private GameUI _gameUI;

	    /// <summary>
	    ///     The object that holds the invalid selection
	    /// </summary>
	    private MovingCanvas _invalidButtons;

	    /// <summary>
	    ///     The pointer at the edge of the screen
	    /// </summary>
	    private TouchInfo m_DragPointer;

	    /// <summary>
	    ///     Keeps track of whether or not the ghost tower is selected
	    /// </summary>
	    private bool m_IsGhostSelected;

	    /// <summary>
	    ///     Hide UI
	    /// </summary>
	    protected virtual void Awake()
        {
            if (_confirmationButtons != null) _confirmationButtons.canvasEnabled = false;
            if (_invalidButtons != null) _invalidButtons.canvasEnabled = false;
        }

	    /// <summary>
	    ///     Decay flick
	    /// </summary>
	    protected override void Update()
        {
            base.Update();

            // Edge pan
            if (m_DragPointer != null) EdgePan();

            if (UnityInput.GetKeyDown(KeyCode.Escape))
                switch (_gameUI.state)
                {
                    case State.Normal:
                        if (_gameUI.isTowerSelected)
                            _gameUI.DeselectTower();
                        else
                            _gameUI.Pause();
                        break;
                    case State.Building:
                        _gameUI.CancelGhostPlacement();
                        break;
                }
        }

	    /// <summary>
	    ///     Register input events
	    /// </summary>
	    protected override void OnEnable()
        {
            base.OnEnable();

            var hud = FindObjectOfType<HUDWindow>();
            if (hud)
            {
                _confirmationButtons = hud.ConfirmationButtons;
                _invalidButtons = hud.InvalidButtons;
            }

            _gameUI.stateChanged += OnStateChanged;
            _gameUI.ghostBecameValid += OnGhostBecameValid;

            // Register tap event
            inputController.tapped += OnTap;
            inputController.startedDrag += OnStartDrag;

            // disable pop ups
            _confirmationButtons.canvasEnabled = false;
            _invalidButtons.canvasEnabled = false;
        }

	    /// <summary>
	    ///     Deregister input events
	    /// </summary>
	    protected override void OnDisable()
        {
            base.OnDisable();

            if (_confirmationButtons != null) _confirmationButtons.canvasEnabled = false;
            if (_invalidButtons != null) _invalidButtons.canvasEnabled = false;
            inputController.tapped -= OnTap;
            inputController.startedDrag -= OnStartDrag;
            if (_gameUI != null)
            {
                _gameUI.stateChanged -= OnStateChanged;
                _gameUI.ghostBecameValid -= OnGhostBecameValid;
            }
        }

	    /// <summary>
	    ///     Called on input press
	    /// </summary>
	    protected override void OnPress(PointerActionInfo pointer)
        {
            base.OnPress(pointer);
            var touchInfo = pointer as TouchInfo;
            // Press starts on a ghost? Then we can pick it up
            if (touchInfo != null)
                if (_gameUI.state == State.Building)
                {
                    m_IsGhostSelected = _gameUI.IsPointerOverGhost(pointer);
                    if (m_IsGhostSelected) m_DragPointer = touchInfo;
                }
        }

	    /// <summary>
	    ///     Called on input release, for flicks
	    /// </summary>
	    protected override void OnRelease(PointerActionInfo pointer)
        {
            // Override normal behaviour. We only want to do flicks if there's no ghost selected
            // For this reason, we intentionally do not call base
            var touchInfo = pointer as TouchInfo;

            if (touchInfo != null)
            {
                // Show UI on release
                if (_gameUI.isBuilding)
                {
                    Vector2 screenPoint = cameraRig.cachedCamera.WorldToScreenPoint(_gameUI.GetGhostPosition());
                    if (_gameUI.IsGhostAtValidPosition() && _gameUI.IsValidPurchase())
                    {
                        _confirmationButtons.canvasEnabled = true;
                        _invalidButtons.canvasEnabled = false;
                        _confirmationButtons.TryMove(screenPoint);
                    }
                    else
                    {
                        _invalidButtons.canvasEnabled = true;
                        _confirmationButtons.canvasEnabled = false;
                        _confirmationButtons.TryMove(screenPoint);
                    }

                    if (m_IsGhostSelected) _gameUI.ReturnToBuildMode();
                }

                if (!m_IsGhostSelected && cameraRig != null)
                    // Do normal base behaviour here
                    DoReleaseFlick(pointer);

                m_IsGhostSelected = false;

                // Reset m_DragPointer if released
                if (m_DragPointer != null && m_DragPointer.touchId == touchInfo.touchId) m_DragPointer = null;
            }
        }

	    /// <summary>
	    ///     Called on tap,
	    ///     calls confirmation of tower placement
	    /// </summary>
	    protected virtual void OnTap(PointerActionInfo pointer)
        {
            var touchInfo = pointer as TouchInfo;
            if (touchInfo != null)
            {
                if (_gameUI.state == State.Normal && !touchInfo.startedOverUI)
                {
                    _gameUI.TrySelectTower(touchInfo);
                }
                else if (_gameUI.state == State.Building && !touchInfo.startedOverUI)
                {
                    _gameUI.TryMoveGhost(touchInfo, false);
                    if (_gameUI.IsGhostAtValidPosition() && _gameUI.IsValidPurchase())
                    {
                        _confirmationButtons.canvasEnabled = true;
                        _invalidButtons.canvasEnabled = false;
                        _confirmationButtons.TryMove(touchInfo.currentPosition);
                    }
                    else
                    {
                        _invalidButtons.canvasEnabled = true;
                        _invalidButtons.TryMove(touchInfo.currentPosition);
                        _confirmationButtons.canvasEnabled = false;
                    }
                }
            }
        }

	    /// <summary>
	    ///     Called on input drag start
	    /// </summary>
	    protected virtual void OnStartDrag(PointerActionInfo pointer)
        {
            var touchInfo = pointer as TouchInfo;
            if (touchInfo != null)
                if (m_IsGhostSelected)
                {
                    _gameUI.ChangeToDragMode();
                    m_DragPointer = touchInfo;
                }
        }


	    /// <summary>
	    ///     Called when we drag
	    /// </summary>
	    protected override void OnDrag(PointerActionInfo pointer)
        {
            // Override normal behaviour. We only want to pan if there's no ghost selected
            // For this reason, we intentionally do not call base
            var touchInfo = pointer as TouchInfo;
            if (touchInfo != null)
            {
                // Try to pick up the tower if it was dragged off
                if (m_IsGhostSelected) _gameUI.TryMoveGhost(pointer, false);

                if (_gameUI.state == State.BuildingWithDrag)
                {
                    DragGhost(touchInfo);
                }
                else
                {
                    // Do normal base behaviour only if no ghost selected
                    if (cameraRig != null)
                    {
                        DoDragPan(pointer);

                        if (_invalidButtons.canvasEnabled)
                            _invalidButtons.TryMove(
                                cameraRig.cachedCamera.WorldToScreenPoint(_gameUI.GetGhostPosition()));
                        if (_confirmationButtons.canvasEnabled)
                            _confirmationButtons.TryMove(
                                cameraRig.cachedCamera.WorldToScreenPoint(_gameUI.GetGhostPosition()));
                    }
                }
            }
        }

	    /// <summary>
	    ///     Drags the ghost
	    /// </summary>
	    private void DragGhost(TouchInfo touchInfo)
        {
            if (touchInfo.touchId == m_DragPointer.touchId)
            {
                _gameUI.TryMoveGhost(touchInfo, false);

                if (_invalidButtons.canvasEnabled) _invalidButtons.canvasEnabled = false;
                if (_confirmationButtons.canvasEnabled) _confirmationButtons.canvasEnabled = false;
            }
        }

	    /// <summary>
	    ///     pans at the edge of the screen
	    /// </summary>
	    private void EdgePan()
        {
            var edgeWidth = panAreaScreenPercentage * Screen.width;
            PanWithScreenCoordinates(m_DragPointer.currentPosition, edgeWidth, panSpeed);
        }


	    /// <summary>
	    ///     If the new state is <see cref="GameUI.State.Building" /> then move the ghost to the center of the screen
	    /// </summary>
	    /// <param name="previousState">
	    ///     The previous the GameUI was is in
	    /// </param>
	    /// <param name="currentState">
	    ///     The new state the GameUI is in
	    /// </param>
	    private void OnStateChanged(State previousState, State currentState)
        {
            // Early return for two reasons
            // 1. We are not moving into Build Mode
            // 2. We are not actually touching
            if (UnityInput.touchCount == 0) return;
            if (currentState == State.Building && previousState != State.BuildingWithDrag)
            {
                _gameUI.MoveGhostToCenter();
                _confirmationButtons.canvasEnabled = false;
                _invalidButtons.canvasEnabled = false;
            }

            if (currentState == State.BuildingWithDrag) m_IsGhostSelected = true;
        }

	    /// <summary>
	    ///     Displays the correct confirmation buttons when the tower has become valid
	    /// </summary>
	    private void OnGhostBecameValid()
        {
            // this only needs to be done if the invalid buttons are already on screen
            if (!_invalidButtons.canvasEnabled) return;
            Vector2 screenPoint = cameraRig.cachedCamera.WorldToScreenPoint(_gameUI.GetGhostPosition());
            if (!_confirmationButtons.canvasEnabled)
            {
                _confirmationButtons.canvasEnabled = true;
                _invalidButtons.canvasEnabled = false;
                _confirmationButtons.TryMove(screenPoint);
            }
        }
    }
}