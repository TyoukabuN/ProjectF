using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace UnityEngine.Rendering.PostProcessing
{
    [Serializable]
    [PostProcess(typeof(GrayscaleRender), PostProcessEvent.AfterStack, "Custom/Grayscale")]
    public sealed class GrayScale : PostProcessEffectSettings
    {
        [SerializeField]
        [Range(0f, 1f), Tooltip("Grayscale effect intensity")]
        public FloatParameter blend = new FloatParameter { value = 0.5f };

        public override bool IsEnabledAndSupported(PostProcessRenderContext context)
        {
            return enabled.value && blend.value > 0f;
        }
    }

    public sealed class GrayscaleRender : PostProcessEffectRenderer<GrayScale>
    {
        public override void Render(PostProcessRenderContext context)
        {
            var cmd = context.command;
            cmd.BeginSample("Grayscale");

            var sheet = context.propertySheets.Get(context.resources.shaders.grayScale);
            sheet.properties.SetFloat("_Blend", settings.blend);
            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);

            cmd.EndSample("Grayscale");
        }
    }
}
