/**
 * @author Anh Pham (Zenga)
 * @email anhpt.csit@gmail.com, anhpt@zenga.com.vn
 * @date 2024/03/29
 */

using UnityEngine;

namespace SS.UI
{
    public class ScreenController : MonoBehaviour
    {
        public Component screen
        {
            get;
            set;
        }

        public string showAnimation
        {
            get;
            set;
        }

        public string hideAnimation
        {
            get;
            set;
        }

        public string animationObjectName
        {
            get;
            set;
        }

        public bool hasShield
        {
            get;
            set;
        }

        public ScreenManager screenManager
        {
            get;
            set;
        }

        private void OnDestroy()
        {
            screenManager.OnScreenDestroy(screen);
        }
    }
}