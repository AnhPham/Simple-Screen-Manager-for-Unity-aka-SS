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
        [SerializeField] UnscaledAnimation _unscaledAnimation;
        #endregion

        #region Public Properties
        public UnscaledAnimation unscaledAnimation => _unscaledAnimation;
        #endregion
    }
}
