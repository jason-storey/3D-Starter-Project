using UnityEngine;
using static UnityEngine.Object;
using static UnityEngine.Resources;

namespace LS
{
    
    public static class EntryPoint
    {
        static GameManager _manager;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void InitializeGameManager()
        {
            _manager = Instantiate(
                Load<GameManager>(GAME_MANAGER_PREFAB));
            _manager.name = GAME_MANAGER_PREFAB;
            DontDestroyOnLoad(_manager);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void StartTheGame()
        {
            _manager.StartTheGame();
        }

        
        
        
        const string GAME_MANAGER_PREFAB = "GameManager";
    }
    
    
    
}