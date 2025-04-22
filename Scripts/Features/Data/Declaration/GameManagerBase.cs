using System;
using Features.Data.Implementation.Savers;
using UnityEngine;

namespace Features.Data.Declaration
{
	/// <summary>
	///     Base game manager
	/// </summary>
	public abstract class GameManagerBase<TGameManager, TDataStore> : MonoBehaviour
        where TDataStore : GameDataStoreBase, new()
        where TGameManager : GameManagerBase<TGameManager, TDataStore>
    {
	    /// <summary>
	    ///     File name of saved game
	    /// </summary>
	    private const string k_SavedGameFile = "save";

	    /// <summary>
	    ///     The serialization implementation for persistence
	    /// </summary>
	    protected JsonSaver<TDataStore> m_DataSaver;

	    /// <summary>
	    ///     The object used for persistence
	    /// </summary>
	    protected TDataStore m_DataStore;

	    /// <summary>
	    ///     Load data
	    /// </summary>
	    protected virtual void Awake()
        {
            LoadData();
        }

	    /// <summary>
	    ///     Set up persistence
	    /// </summary>
	    protected void LoadData()
        {
            // If it is in Unity Editor use the standard JSON (human readable for debugging) otherwise encrypt it for deployed version
#if UNITY_EDITOR
            m_DataSaver = new JsonSaver<TDataStore>(k_SavedGameFile);
#else
			m_DataSaver = new EncryptedJsonSaver<TDataStore>(k_SavedGameFile);
#endif

            try
            {
                if (!m_DataSaver.Load(out m_DataStore))
                {
                    m_DataStore = new TDataStore();
                    SaveData();
                }
            }
            catch (Exception)
            {
                Debug.Log("Failed to load data, resetting");
                m_DataStore = new TDataStore();
                SaveData();
            }
        }

	    /// <summary>
	    ///     Saves the gamme
	    /// </summary>
	    protected virtual void SaveData()
        {
            m_DataSaver.Save(m_DataStore);
        }
    }
}