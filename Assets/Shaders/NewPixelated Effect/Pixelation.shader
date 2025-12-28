Shader "Hidden/PixelationEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _PixelSize ("Pixel Size", Range(8, 512)) = 64
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque" 
            "RenderPipeline" = "UniversalPipeline"
        }
        
        LOD 100
        ZTest Always
        ZWrite Off
        Cull Off

        Pass
        {
            Name "PixelationPass"
            
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            
            // _BlitTexture and _BlitTexture_TexelSize are already defined in Blit.hlsl
            // Pixel size property - represents how many pixels across the screen
            float _PixelSize;
            
            // Fragment shader - applies pixelation
            half4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.texcoord;
                
                // _PixelSize = number of pixels we want across the screen
                // Lower value = fewer pixels = more pixelated
                float2 pixelGrid = float2(_PixelSize, _PixelSize);
                
                // Calculate pixelated UV coordinates
                float2 pixelatedUV;
                pixelatedUV.x = floor(uv.x * pixelGrid.x) / pixelGrid.x;
                pixelatedUV.y = floor(uv.y * pixelGrid.y) / pixelGrid.y;
                
                // Add half pixel offset to sample from center of pixelated block
                pixelatedUV += 0.5 / pixelGrid;
                
                // Sample the texture at pixelated coordinates
                half4 color = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, pixelatedUV);
                
                return color;
            }
            ENDHLSL
        }
    }
}