using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace UnityEngine.Rendering.PostProcessing
{
    [Serializable]
    [PostProcess(typeof(RadialBlurRender), PostProcessEvent.AfterStack, "Custom/RadialBlur")]
    public sealed class RadialBlur : PostProcessEffectSettings
    {
        [SerializeField]
        [Range(0f, 1f), Tooltip("模糊程度，不能过高")]
        public FloatParameter blurFactor = new FloatParameter { value = 0.0109f, overrideState = true };

        [SerializeField]
        [Range(0.0f, 3.0f), Tooltip("清晰图像与原图插值")]
        public FloatParameter lerpFactor = new FloatParameter { value = 0f, overrideState = true };

        [SerializeField]
        [Range(0.0f, 3.0f), Tooltip("降低分辨率系数")]
        public IntParameter downSampleFactor = new IntParameter { value = 1, overrideState = true };

        [SerializeField]
        [Tooltip("模糊中心（0-1）屏幕空间，默认为中心点")]
        public Vector2Parameter blurCenter = new Vector2Parameter { value = new Vector2(0.5f, 0.5f), overrideState = true };

        [SerializeField]
        [Range(0f, 100f), Tooltip("(剩余)清晰图像与原图插值")]
        public FloatParameter leftLerpFactor = new FloatParameter { value = 0f, overrideState = true };
        [SerializeField]
        [Tooltip("调试")]
        public BoolParameter debug = new BoolParameter { value = false };

        [HideInInspector]
        public FloatParameter startTime = new FloatParameter { value = 0f, overrideState = true };

        public static readonly int BlurTex = Shader.PropertyToID("_BlurTex");
        public static readonly int AfterBlurTex = Shader.PropertyToID("_AfterBlurTex");

        public override bool IsEnabledAndSupported(PostProcessRenderContext context)
        {
            return enabled.value;
        }
    }

    public sealed class RadialBlurRender : PostProcessEffectRenderer<RadialBlur>
    {
        public override void Render(PostProcessRenderContext context)
        {
            if (context.resources.shaders.radiaBlur == null)
                return;

            var cmd = context.command;
            var lerpFactor = settings.debug ? settings.lerpFactor.value : settings.leftLerpFactor.value;

            cmd.BeginSample("RadialBlur");

            var sheet = context.propertySheets.Get(context.resources.shaders.radiaBlur);
            sheet.properties.SetFloat("_BlurFactor", settings.blurFactor);
            sheet.properties.SetVector("_BlurCenter", settings.blurCenter);
            sheet.properties.SetFloat("_LerpFactor", lerpFactor);

            int tw = Mathf.FloorToInt(context.screenWidth >> settings.downSampleFactor);
            int th = Mathf.FloorToInt(context.screenHeight >> settings.downSampleFactor);
            //blur
            context.GetScreenSpaceTemporaryRT(cmd, RadialBlur.AfterBlurTex, 0, context.sourceFormat, RenderTextureReadWrite.Default, FilterMode.Bilinear, tw, th);
            context.command.BlitFullscreenTriangle(context.source, RadialBlur.AfterBlurTex, sheet, 0);
            //blend with origin
            cmd.SetGlobalTexture(RadialBlur.BlurTex, RadialBlur.AfterBlurTex);
            //apply to dest
            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 1);

            cmd.ReleaseTemporaryRT(RadialBlur.AfterBlurTex);

            cmd.EndSample("RadialBlur");
        }
    }
}
