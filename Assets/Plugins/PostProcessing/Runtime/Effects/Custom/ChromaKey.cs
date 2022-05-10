using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace UnityEngine.Rendering.PostProcessing
{
    [Serializable]
    [PostProcess(typeof(ChromaKeyRender), PostProcessEvent.BeforeStack, "Custom/ChromaKey")]
    public sealed class ChromaKey : PostProcessEffectSettings
    {
        [SerializeField]
        [Range(0f, 1f), Tooltip("绿幕的颜色")]
        public ColorParameter keyColor = new ColorParameter { value = Color.green, overrideState = true };

        [SerializeField]
        [Range(0.0f, 1.0f)]
        public FloatParameter colorCutoff = new FloatParameter { value = 0.2f, overrideState = true };

        [SerializeField]
        [Range(0.0f, 1.0f)]
        public FloatParameter colorFeathering = new FloatParameter { value = 0.333f, overrideState = true };

        [SerializeField]
        [Range(0.0f, 1.0f)]
        public FloatParameter maskFeathering = new FloatParameter { value = 1.0f, overrideState = true };

        [SerializeField]
        [Range(0.0f, 1.0f)]
        public FloatParameter sharpening = new FloatParameter { value = 0.5f, overrideState = true };

        [SerializeField]
        [Range(0.0f, 1.0f)]
        public FloatParameter despillStrength = new FloatParameter { value = 0f, overrideState = false };

        [SerializeField]
        [Range(0.0f, 1.0f)]
        public FloatParameter despillLuminanceAdd = new FloatParameter { value = 0f, overrideState = false };

        public override bool IsEnabledAndSupported(PostProcessRenderContext context)
        {
            return enabled.value;
        }
    }

    public sealed class ChromaKeyRender : PostProcessEffectRenderer<ChromaKey>
    {
        public override void Render(PostProcessRenderContext context)
        {
            if (context.resources.shaders.radiaBlur == null)
                return;

            var cmd = context.command;

            cmd.BeginSample("ChromaKey");

            var sheet = context.propertySheets.Get(context.resources.shaders.chromaKey);
            sheet.properties.SetColor("_KeyColor", settings.keyColor);
            sheet.properties.SetFloat("_ColorCutoff", settings.colorCutoff);
            sheet.properties.SetFloat("_ColorFeathering", settings.colorFeathering);
            sheet.properties.SetFloat("_MaskFeathering", settings.maskFeathering);
            sheet.properties.SetFloat("_Sharpening", settings.sharpening);
            sheet.properties.SetFloat("_Despill", settings.despillStrength);
            sheet.properties.SetFloat("_DespillLuminanceAdd", settings.despillLuminanceAdd);
            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);

            cmd.EndSample("ChromaKey");
        }
    }
}
