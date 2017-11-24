Shader "Custom/PageShader" {
	Properties {
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_TextTex ("Text (Multiply)", 2D) = "white" {}
		_ColorTex ("Text Noise", 2D) = "black" {}
		_TextColor ("Text Color", Color) = (1,1,1,1)
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0
		
		sampler2D _MainTex;
		sampler2D _TextTex;
		sampler2D _ColorTex;
		fixed4 _TextColor;

		struct Input {
			float2 uv_MainTex;
		};

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_CBUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_CBUFFER_END

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
			fixed4 b = tex2D (_ColorTex, IN.uv_MainTex);
			fixed4 t = tex2D(_TextTex, IN.uv_MainTex);
			o.Albedo = lerp(c.rgb, t.rgb * _TextColor, clamp(t.a - b.r, 0, 1)*_TextColor.a);
			// Metallic and smoothness come from slider variables
			o.Metallic = 0;
			o.Smoothness = 0;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
