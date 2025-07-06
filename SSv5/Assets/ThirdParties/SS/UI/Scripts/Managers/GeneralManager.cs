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
        #region Serialize Fields
        [SerializeField] protected Camera _backgroundCamera;
        [SerializeField] protected Canvas _mainCanvas;
        [SerializeField] protected UnscaledAnimation _sceneShield;
        [SerializeField] protected RectTransform _screenContainer;
        [SerializeField] protected RectTransform _topContainer;
        [SerializeField] protected RectTransform _screenLoadingContainer;
        [SerializeField] protected RectTransform _topShieldContainer;
        [SerializeField] protected RectTransform _sceneLoadingContainer;
        [SerializeField] protected float _animationSpeed = 1;
        #endregion

        #region Public Properties
        public Camera BackgroundCamera => _backgroundCamera;
        public Canvas MainCanvas => _mainCanvas;
        public UnscaledAnimation SceneShield => _sceneShield;
        public RectTransform ScreenContainer => _screenContainer;
        public RectTransform TopContainer => _topContainer;
        public RectTransform ScreenLoadingContainer => _screenLoadingContainer;
        public RectTransform TopShieldContainer => _topShieldContainer;
        public RectTransform SceneLoadingContainer => _sceneLoadingContainer;
        public float AnimationSpeed => _animationSpeed;
        #endregion

        #region Unity Cycle
        protected virtual void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
        #endregion

        #region Public Methods
        public virtual void Setup(float animationSpeed)
        {
            _animationSpeed = animationSpeed;
        }
        #endregion
    }
}
