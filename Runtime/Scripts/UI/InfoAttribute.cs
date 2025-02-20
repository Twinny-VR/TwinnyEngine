using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Twinny.UI
{

    public class InfoAttribute : PropertyAttribute
    {
        public string message;

        public InfoAttribute(string message)
        {
            this.message = message;
        }
    }
}
