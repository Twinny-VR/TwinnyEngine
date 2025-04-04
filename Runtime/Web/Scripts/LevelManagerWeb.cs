using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Twinny.System
{
    public class LevelManagerWeb : LevelManager
    {
        public override async Task LoadAdditiveSceneAsync(object scene)
        {
            //            AsyncOperation asyncOperation = (scene is string name) ? SceneManager.LoadSceneAsync(name, LoadSceneMode.Additive) : SceneManager.LoadSceneAsync((int)scene, LoadSceneMode.Additive);
            if (scene is string name)
                SceneManager.LoadScene(name, LoadSceneMode.Additive);
            else
                SceneManager.LoadScene((int)scene, LoadSceneMode.Additive);
           /*
            asyncOperation.allowSceneActivation = false;
            while (!asyncOperation.isDone)
            {

                Debug.Log($"Carregando Cena {scene}");
                await Task.Delay(100);
                if (asyncOperation.progress >= .9f)
                {

                    asyncOperation.allowSceneActivation = true;
                    break;
                }
            }
           */
            Debug.Log("CARREGOU!");
            await Task.Yield();
        }
    }
}
