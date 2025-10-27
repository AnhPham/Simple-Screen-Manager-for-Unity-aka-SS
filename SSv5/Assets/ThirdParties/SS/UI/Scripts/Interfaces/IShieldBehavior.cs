/**
 * @author Anh Pham (Zenga)
 * @email anhpt.csit@gmail.com, anhpt@zenga.com.vn
 * @date 2024/03/29
 */

namespace SS.UI
{
    public interface IShieldBehavior
    {
        public void OnShieldTap();
        public void OnShieldHold();
        public void OnShieldRelease();
    }
}