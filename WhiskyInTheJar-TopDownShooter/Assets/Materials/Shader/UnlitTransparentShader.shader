Shader "Custom/UnlitTransparentShader" {
     Properties{
         _Color("Main Color", Color) = (1,1,1,1)
         _MainTex("Base (RGB) Trans (A)", 2D) = "w$$anonymous$$te" {}
     }
 
         SubShader{
             Tags {"Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"}
 
             ZWrite Off
             Lighting Off
             Fog { Mode Off }
 
             Blend SrcAlpha OneMinusSrcAlpha
             Cull Off
 
             Pass {
                 Color[_Color]
                 SetTexture[_MainTex] { combine texture * primary }
             }
     }
 }