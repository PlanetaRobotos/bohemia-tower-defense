namespace Windows.MainMenuWidget.Abstract
{
	/// <summary>
	/// Base interface for menu pages
	/// </summary>
	public interface IMainMenuPage
	{
		/// <summary>
		/// Deactivates this page
		/// </summary>
		void Hide();

		/// <summary>
		/// Activates this page
		/// </summary>
		void Show();
	}
}