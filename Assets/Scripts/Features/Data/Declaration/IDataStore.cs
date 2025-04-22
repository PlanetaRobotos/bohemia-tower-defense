namespace Features.Data.Declaration
{
	/// <summary>
	///     Interface for data store
	/// </summary>
	public interface IDataStore
    {
        void PreSave();

        void PostLoad();
    }
}