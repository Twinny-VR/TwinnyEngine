#if FUSION2

using Fusion;
using System.Threading.Tasks;
using UnityEngine;

namespace Twinny.Helpers
{
    public static class AsyncOperationExtensions
    {
        // M�todo de extens�o para transformar AsyncOperation em Task
        public static Task ToTask(this AsyncOperation asyncOperation)
        {
            var tcs = new TaskCompletionSource<bool>();

            // Quando a opera��o for conclu�da, resolva a tarefa.
            asyncOperation.completed += (op) => tcs.SetResult(true);

            return tcs.Task;
        }


        // M�todo para aguardar a conclus�o do carregamento da cena
        public static async Task WaitForSceneLoadAsync(NetworkSceneAsyncOp asyncLoad)
        {
            // Enquanto a cena n�o estiver carregada, aguarde
            while (!asyncLoad.IsDone)
            {
                await Task.Yield(); // Aguarda at� o pr�ximo frame
            }
        }


        // M�todo para aguardar a conclus�o do carregamento da cena
        public static async Task WaitForSceneLoadAsync(AsyncOperation asyncLoad)
        {
            // Enquanto a cena n�o estiver carregada, aguarde
            while (!asyncLoad.isDone)
            {
                await Task.Yield(); // Aguarda at� o pr�ximo frame
            }

        }
    }
}

#endif