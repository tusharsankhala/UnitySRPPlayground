using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRP
{
    public partial class CameraRenderer
    {
        ScriptableRenderContext context;
        Camera camera;

        CullingResults cullingResults;

        static ShaderTagId unlitShaderTagID = new ShaderTagId("SRPDefaultUnlit");

        CommandBuffer buffer = new CommandBuffer
        {
            name = "Render Camera"
        };

        public void Render(ScriptableRenderContext context, Camera camera)
        {
            this.context = context;
            this.camera = camera;

            PrepareBuffer();
            PrepareForSceneWindow();
            if (!Cull())
                return;

            Setup();
            DrawVisibleGeometry();
            DrawUnsupportedShaders();
            DrawGizmos();
            Submit();
        }

        void Setup()
        {
            context.SetupCameraProperties(camera);
            CameraClearFlags flags = camera.clearFlags;

            buffer.ClearRenderTarget(flags <= CameraClearFlags.Depth,
                                     flags == CameraClearFlags.Color,
                                     flags == CameraClearFlags.Color ?
                                        camera.backgroundColor.linear : Color.clear);

            buffer.BeginSample(SampleName);
            ExecuteBuffer();
        }

        void DrawVisibleGeometry()
        {
            // Drawing Opaque Objects.           
            SortingSettings sortingSettings = new SortingSettings(camera)
            {
                criteria = SortingCriteria.CommonOpaque
            };

            DrawingSettings drawSettings = new DrawingSettings(unlitShaderTagID, sortingSettings);
            FilteringSettings filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
            context.DrawRenderers(cullingResults, ref drawSettings, ref filteringSettings);
            context.DrawSkybox(camera);

            // Drawing Transparent Objects.
            sortingSettings.criteria = SortingCriteria.CommonTransparent;
            drawSettings.sortingSettings = sortingSettings;
            filteringSettings.renderQueueRange = RenderQueueRange.transparent;
            context.DrawRenderers(cullingResults, ref drawSettings, ref filteringSettings);
        }

        void Submit()
        {
            buffer.EndSample(SampleName);
            ExecuteBuffer();
            context.Submit();
        }

        void ExecuteBuffer()
        {
            context.ExecuteCommandBuffer(buffer);
            buffer.Clear();
        }

        bool Cull()
        {
            if(camera.TryGetCullingParameters(out ScriptableCullingParameters scriptableCullingParam))
            {
                cullingResults = context.Cull(ref scriptableCullingParam);
                return true;
            }

            return false;
        }
    }
}