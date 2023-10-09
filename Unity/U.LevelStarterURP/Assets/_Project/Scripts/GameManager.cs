using UnityEngine;

namespace LS
{
    public class GameManager : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField]
        Shade _shade;

        [SerializeField]
        bool _fadeInOnStart = true;
        
        void Awake()
        {
            print("Game Manager Ready!");
        }

        public void StartTheGame()
        {
            if (_fadeInOnStart)
                Invoke(nameof(FadeInAndStart), 0.5f);
        }

        void FadeInAndStart()
        {
            _shade.UnCoverScreen();
        }
    }
}