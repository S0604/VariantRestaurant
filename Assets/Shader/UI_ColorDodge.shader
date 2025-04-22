Shader "UI/ColorDodge"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}     // 這是圖片本身
        _Color ("Tint", Color) = (1,1,1,1)             // Tint 顏色控制
    }
    SubShader
    {
        Tags { 
            "Queue"="Transparent"                      // 在 UI 上正常顯示
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Pass
        {
            // 🎯 這裡是重點！使用類似 Color Dodge 的混合方式：
            Blend OneMinusDstColor One

            Cull Off           // 不剔除面
            Lighting Off       // 關閉打光
            ZWrite Off         // 不寫入深度
            ZTest Always       // 不做深度測試（讓 UI 不被擋住）
            Fog { Mode Off }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                half2 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;
            fixed4 _Color;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 texCol = tex2D(_MainTex, IN.texcoord) * IN.color;
                return texCol;
            }
            ENDCG
        }
    }
}
