 using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;
using UnityEngine.Rendering;

namespace XGUI
{
    #region XTextMeshProHrefEvent
    public class XTextMeshProHrefEvent : MonoBehaviour, IPointerClickHandler
    {
        public class HrefClickEvent : UnityEvent<string, string> { }
        //点击事件监听
        public HrefClickEvent onHrefClick = new HrefClickEvent();

        public XTextMeshProUGUI xtext;

        //没有
        public bool notlinkIdCallEvent = false;

        public void OnPointerClick(PointerEventData eventData)
        {
            int linkIndex = TMP_TextUtilities.FindIntersectingLink(xtext, Input.mousePosition, eventData.enterEventCamera);
            string linkId = "-1";
            string linkText = string.Empty;

            if (linkIndex != -1)
            {
                TMP_LinkInfo linkInfo = xtext.textInfo.linkInfo[linkIndex];
                linkId = linkInfo.GetLinkID();
                linkText = linkInfo.GetLinkText();
            }

            try
            {
                if (!notlinkIdCallEvent && linkId == "-1") return;

                //把解析后的linkId，通过事件发出去
                onHrefClick.Invoke(linkId, linkText);
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }

        void OnDestroy()
        {
            if (onHrefClick != null)
            {
                onHrefClick.RemoveAllListeners();
                onHrefClick = null;
            }
        }
    }
    #endregion


    public class XTextMeshProUGUI : TextMeshProUGUI
    {
        private static Color grayColor = new Color(58 / 255.0f, 69 / 255.0f, 92 / 255.0f, 1);
        private static Color grayOutlineColor = new Color(122 / 255.0f, 122 / 255.0f, 122 / 255.0f, 1);
        private static Color VoidColor = new Color(0, 0, 0, 0);

        private static Color _garyTopColor = new Color(225 / 255.0f, 225 / 255.0f, 225 / 255.0f);
        private static Color _garyBottomColor = new Color(242 / 255.0f, 242 / 255.0f, 242 / 255.0f);

        private Material m_CacheMaterial = null;

        private static Material s_GrayMaterial;

        /// <summary>
        /// 是否静态字体
        /// </summary>
        /// 

        //[SerializeField]
        //public bool isStatic = false;
        //[SerializeField]
        //public int languageId = 0;


        #region 超链接
        private XTextMeshProHrefEvent hrefEvent;
        public XTextMeshProHrefEvent.HrefClickEvent OnHrefClick
        {
            get
            {
                if (hrefEvent == null)
                {
                    hrefEvent = gameObject.AddComponent<XTextMeshProHrefEvent>();
                    hrefEvent.notlinkIdCallEvent = this.notlinkIdCallEvent;
                    hrefEvent.xtext = this;
                }
                return hrefEvent.onHrefClick;
            }
        }

        #endregion

        //渐变色
        private VertexGradient m_CachGradient = new VertexGradient();
        //bool
        private bool m_isHasGradient = false;
        private bool m_isGrey = false;

        private Color m_CacheColor;
        public override Color color
        {
            get { return m_fontColor; }
            set { if (m_fontColor == value) return; this.m_CacheColor = value; m_havePropertiesChanged = true; m_fontColor = value; SetVerticesDirty(); }
        }

        public override string text
        {
            get { return m_text; }
            set
            {
                if (m_text == value)
                    return;

                base.text = value;
            }
        }

        protected override void Awake()
        {
            base.Awake();

            if (m_IsOutlineGray && m_sharedMaterial != null)
            {
                this.m_CacheMaterial = new Material(this.fontMaterial);
            }

            if (!s_GrayMaterial)
            {
                s_GrayMaterial = new Material(this.fontMaterial);
                s_GrayMaterial.EnableKeyword(ShaderUtilities.Keyword_Outline);
                s_GrayMaterial.SetFloat("_IsGray", 1);
            }

            //如果是静态则替换为zh_cn文件对应字符串
            //如果是动态则删除
            if (this.enableVertexGradient)
                m_isHasGradient = true;//this.m_CachGradient = m_fontColorGradient;//

            if (Application.isPlaying && isStatic && languageId != 0)
            {
                //m_text = CSharpLuaInterface.GetLanguage(languageId);
            }
            m_CacheColor = this.color;
        }


        public void SetGray(bool gray)
        {
            if (gray)
            {
                if (!m_IsOutlineGray)
                {
                    if (m_isGrey) return;

                    this.m_CacheColor = this.m_fontColor;
                    this.m_fontColor = grayColor;
                    m_isGrey = true;
                    SetLayoutDirty();
                    //SetVerticesDirty();
                    SetMaterialDirty();
                }
                else
                {
                    
                    this.fontMaterial = s_GrayMaterial;
                }
            }
            else
            {
                m_isGrey = false;
                if (!m_IsOutlineGray)
                {
                    this.m_fontColor = this.m_CacheColor;                    
                    SetLayoutDirty();
                    //SetVerticesDirty();
                    SetMaterialDirty();
                }
                else if (this.m_CacheMaterial)
                {
                    this.fontMaterial = this.m_CacheMaterial;
                    this.fontMaterial.SetFloat("_IsGray", 0);
                }                
            }

            if (m_spriteAsset != null && m_subTextObjects != null && m_subTextObjects.Length > 0)
            {
                for (int i = 0; i < m_subTextObjects.Length; i++)
                {
                    if (m_subTextObjects[i] != null)
                    {
                        m_subTextObjects[i].SetGray(gray);
                    }
                }
            }
        }



        public void SetGradientColor(Color oneColor, Color twoColor)
        {
            if (this.enableVertexGradient)
            {
                VertexGradient gradient = new VertexGradient();
                if (this.m_colorMode == ColorMode.VerticalGradient)
                {
                    gradient.topLeft = oneColor;
                    gradient.bottomLeft = twoColor;
                    gradient.topRight = oneColor;
                    gradient.bottomRight = twoColor;
                }
                else
                { 
                    gradient.topLeft = oneColor;
                    gradient.bottomLeft = oneColor;
                    gradient.topRight = twoColor;
                    gradient.bottomRight = twoColor;
                }
                m_fontColorGradient = gradient;
            }
        }

        public void SetGradientColor(Color oneColor, Color twoColor, int colorMode = 2)
        {
            if (this.m_colorMode != (ColorMode)colorMode)
                this.m_colorMode = (ColorMode)colorMode;                
            m_havePropertiesChanged = true;
            SetGradientColor(oneColor, twoColor);
            SetVerticesDirty();
        }

        private static Color ToColor(string colorName)
        {

            Color hColor;
            string tColor = "#" + colorName;
            ColorUtility.TryParseHtmlString(tColor, out hColor);

            return hColor;
        }

        [SerializeField]
        private bool m_IgnoreAttachingCanvas = false;

        private RectMask2D m_ParentMask;
        public override void RecalculateClipping()
        {
            //UpdateClipParent();
        }
        private void UpdateCull(bool cull)
        {
            var cullingChanged = canvasRenderer.cull != cull;
            canvasRenderer.cull = cull;

            if (cullingChanged)
            {
                onCullStateChanged.Invoke(cull);
                SetVerticesDirty();
            }
        }
        //private void UpdateClipParent()
        //{
        //    var newParent = (maskable && IsActive()) ? XMaskUtilities.GetRectMaskForClippable(this, m_IgnoreAttachingCanvas) : null;

        //    // if the new parent is different OR is now inactive
        //    if (m_ParentMask != null && (newParent != m_ParentMask || !newParent.IsActive()))
        //    {
        //        m_ParentMask.RemoveClippable(this);
        //        UpdateCull(false);
        //    }

        //    // don't re-add it if the newparent is inactive
        //    if (newParent != null && newParent.IsActive())
        //        newParent.AddClippable(this);

        //    m_ParentMask = newParent;
        //}

        public void onLinkSelection()
        {

        }

        
    }

}
