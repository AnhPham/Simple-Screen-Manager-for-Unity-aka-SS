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
        #region Public Properties
        public Component Screen { get; set; }
        public string ShowAnimation { get; set; }
        public string HideAnimation { get; set; }
        public string AnimationObjectName { get; set; }
        public bool HasShield { get; set; }
        public ScreenManager Manager { get; set; }
        public bool BeingDestroyed { get; protected set; }
        public float ShieldAlpha { get; set; }
        #endregion

        #region Unity Cycle
        protected virtual void OnDestroy()
        {
            BeingDestroyed = true;

            if (Manager != null)
            {
                // Normally, this method will be called only once right after Core.Close or Core.Destroy.
                // But this is for the case where the screen is destroyed using Object.Destroy
                Manager.OnScreenClosed(Screen);
            }
        }
        #endregion
    }
}