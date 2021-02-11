using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRP
{
    public class CustomRenderPipeline : RenderPipeline
    {
        CameraRenderer renderer = new CameraRenderer();

        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            foreach(Camera camera in cameras)
            {
                renderer.Render(context, camera);
            }
        }
    }
}