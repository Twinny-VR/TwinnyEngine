using System.Collections;
using TMPro;
using UnityEngine;

namespace Twinny.UI
{
    public class Loading : MonoBehaviour
    {

        #region Cached Components
        [SerializeField]
        private TMP_Text TMP_Loading;
        #endregion

        // Start is called before the first frame update
        void Start()
        {
            StartCoroutine(TextAnimate());        
        }

        IEnumerator TextAnimate()
        {
            string text = TMP_Loading.text;

            while (true) {
                TMP_Loading.text = text;
                yield return new WaitForSeconds(.5f);
                TMP_Loading.text += ".";
                yield return new WaitForSeconds(.5f);
                TMP_Loading.text += ".";
                yield return new WaitForSeconds(.5f);
                TMP_Loading.text += ".";
                yield return new WaitForSeconds(.5f);
            }

        }
    }
}
