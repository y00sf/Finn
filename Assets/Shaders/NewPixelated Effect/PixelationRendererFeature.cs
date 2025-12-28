using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

/// <summary>
/// Depth-safe full screen pixelation effect for Unity 6 URP
/// Preserves camera depth texture for other shaders
/// </summary>
public class PixelationRendererFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class PixelationSettings
    {
        [Range(8, 512)]
        public int pixelSize = 64;
        
        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        
        public Material pixelationMaterial;
    }

    public PixelationSettings settings = new PixelationSettings();
    private PixelationRenderPass renderPass;

    public override void Create()
    {
        if (settings.pixelationMaterial == null)
        {
            Debug.LogError("Pixelation material is not assigned!");
            return;
        }

        renderPass = new PixelationRenderPass(settings);
        renderPass.renderPassEvent = settings.renderPassEvent;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (renderPass == null || settings.pixelationMaterial == null)
            return;

        if (renderingData.cameraData.cameraType == CameraType.Game || 
            renderingData.cameraData.cameraType == CameraType.SceneView)
        {
            renderPass.Setup(renderer);
            renderer.EnqueuePass(renderPass);
        }
    }

    protected override void Dispose(bool disposing)
    {
        renderPass?.Dispose();
    }

    private class PixelationRenderPass : ScriptableRenderPass
    {
        private PixelationSettings settings;
        private RTHandle tempColorTarget;
        private ScriptableRenderer renderer;
        
        private static readonly int PixelSizeID = Shader.PropertyToID("_PixelSize");
        private static readonly int BlitTextureID = Shader.PropertyToID("_BlitTexture");

        public PixelationRenderPass(PixelationSettings pixelSettings)
        {
            settings = pixelSettings;
            profilingSampler = new ProfilingSampler("PixelationEffect");
        }

        public void Setup(ScriptableRenderer renderer)
        {
            this.renderer = renderer;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            var desc = renderingData.cameraData.cameraTargetDescriptor;
            desc.depthBufferBits = 0;
            
            RenderingUtils.ReAllocateIfNeeded(ref tempColorTarget, desc, 
                FilterMode.Point, TextureWrapMode.Clamp, name: "_PixelationTemp");
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (settings.pixelationMaterial == null)
            {
                Debug.LogError("Pixelation material is null!");
                return;
            }

            CommandBuffer cmd = CommandBufferPool.Get("Pixelation Effect");
            
            var cameraData = renderingData.cameraData;
            var source = renderer.cameraColorTargetHandle;
            
            // Calculate pixel size - now just pass the setting directly as a float
            settings.pixelationMaterial.SetFloat(PixelSizeID, settings.pixelSize);
            
            // Apply pixelation
            Blitter.BlitCameraTexture(cmd, source, tempColorTarget, settings.pixelationMaterial, 0);
            Blitter.BlitCameraTexture(cmd, tempColorTarget, source);
            
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            // Cleanup
        }

        // RenderGraph Path
        private class PassData
        {
            public Material material;
            public Vector4 pixelSize;
            public TextureHandle source;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (settings.pixelationMaterial == null)
            {
                Debug.LogError("Pixelation material is null in RenderGraph!");
                return;
            }

            var resourceData = frameData.Get<UniversalResourceData>();
            var cameraData = frameData.Get<UniversalCameraData>();
            
            var source = resourceData.activeColorTexture;
            
            if (!source.IsValid())
            {
                Debug.LogError("Invalid source texture in RenderGraph!");
                return;
            }

            // Create temp texture
            var desc = renderGraph.GetTextureDesc(source);
            desc.name = "PixelationTemp";
            desc.depthBufferBits = DepthBits.None;
            desc.clearBuffer = false;
            
            var tempTarget = renderGraph.CreateTexture(desc);
            
            // Calculate pixel size - pass directly as float
            settings.pixelationMaterial.SetFloat(PixelSizeID, settings.pixelSize);
            
            // Apply pixelation
            using (var builder = renderGraph.AddRasterRenderPass<PassData>("Pixelation", out var passData, profilingSampler))
            {
                passData.material = settings.pixelationMaterial;
                passData.pixelSize = new Vector4(settings.pixelSize, settings.pixelSize, 0, 0);
                passData.source = source;
                
                builder.UseTexture(source, AccessFlags.Read);
                builder.SetRenderAttachment(tempTarget, 0, AccessFlags.Write);
                
                builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                {
                    data.material.SetFloat(PixelSizeID, data.pixelSize.x);
                    data.material.SetTexture(BlitTextureID, data.source);
                    Blitter.BlitTexture(context.cmd, data.source, new Vector4(1, 1, 0, 0), data.material, 0);
                });
            }
            
            // Copy back
            using (var builder = renderGraph.AddRasterRenderPass<PassData>("Pixelation Copy", out var passData, profilingSampler))
            {
                passData.source = tempTarget;
                
                builder.UseTexture(tempTarget, AccessFlags.Read);
                builder.SetRenderAttachment(source, 0, AccessFlags.Write);
                
                builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                {
                    Blitter.BlitTexture(context.cmd, data.source, new Vector4(1, 1, 0, 0), 0, false);
                });
            }
        }

        public void Dispose()
        {
            tempColorTarget?.Release();
        }
    }
}