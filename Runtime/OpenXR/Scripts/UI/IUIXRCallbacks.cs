using Fusion;
using UnityEngine;

namespace Twinny.UI
{
    public interface IUIXRCallbacks : IUICallBacks
    {
        void OnConnected(GameMode gameMode);
        void OnDisconnected();
        void OnPlayerList(int count);

    }
}
