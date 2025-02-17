#if FUSION2

using Fusion;
using System.Threading.Tasks;
using UnityEngine;

namespace Twinny.Helpers
{
    public static class AsyncOperationExtensions
    {
        // Método de extensão para transformar AsyncOperation em Task
        public static Task ToTask(this AsyncOperation asyncOperation)
        {
            var tcs = new TaskCompletionSource<bool>();

            // Quando a operação for concluída, resolva a tarefa.
            asyncOperation.completed += (op) => tcs.SetResult(true);

            return tcs.Task;
        }


        // Método para aguardar a conclusão do carregamento da cena
        public static async Task WaitForSceneLoadAsync(NetworkSceneAsyncOp asyncLoad)
        {
            // Enquanto a cena não estiver carregada, aguarde
            while (!asyncLoad.IsDone)
            {
                await Task.Yield(); // Aguarda até o próximo frame
            }
        }


        // Método para aguardar a conclusão do carregamento da cena
        public static async Task WaitForSceneLoadAsync(AsyncOperation asyncLoad)
        {
            // Enquanto a cena não estiver carregada, aguarde
            while (!asyncLoad.isDone)
            {
                await Task.Yield(); // Aguarda até o próximo frame
            }

        }
    }
}

#endif