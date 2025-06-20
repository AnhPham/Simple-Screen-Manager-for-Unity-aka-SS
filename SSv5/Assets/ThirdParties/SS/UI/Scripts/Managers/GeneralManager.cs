/**
 * @author Anh Pham (Zenga)
 * @email anhpt.csit@gmail.com, anhpt@zenga.com.vn
 * @date 2024/03/29
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SS.UI
{
    public class GeneralManager : MonoBehaviour
    {
        #region Public Members
        public Camera backgroundCamera;
        public Canvas canvas;
        public UnscaledAnimation sceneShield;
        public RectTransform screenContainer;
        public RectTransform topContainer;
        public RectTransform screenLoadingContainer;
        public RectTransform topShieldContainer;
        public RectTransform sceneLoadingContainer;
        public float animationSpeed = 1;
        #endregion

        #region Unity Cycle
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
        #endregion

        #region Public Methods
        public void Setup(float animationSpeed)
        {
            this.animationSpeed = animationSpeed;
        }
        #endregion
    }
}
