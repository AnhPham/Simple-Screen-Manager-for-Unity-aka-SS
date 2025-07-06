/**
 * @author Anh Pham (Zenga)
 * @email anhpt.csit@gmail.com, anhpt@zenga.com.vn
 * @date 2024/03/29
 */

using UnityEngine;

namespace SS.UI
{
    public class ShieldController : MonoBehaviour
    {
        #region Serialize Fields
        [SerializeField] protected UnscaledAnimation _unscaledAnimation;
        #endregion

        #region Public Properties
        public UnscaledAnimation UnscaledAnimation => _unscaledAnimation;
        public bool BeingDestroyed { get; protected set; }
        #endregion

        #region Unity Cycle
        protected virtual void OnDestroy()
        {
            BeingDestroyed = true;
        }
        #endregion
    }
}
