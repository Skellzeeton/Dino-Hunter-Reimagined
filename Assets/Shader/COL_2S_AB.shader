Shader "Triniti/Character/COL_2S_AB" {
Properties {
 _Color ("Main Color", Color) = (1,1,1,1)
 _MainTex ("MainTex(RGB)", 2D) = "" {}
}
SubShader {
 Tags { "QUEUE"="Transparent" }
 Pass {
  Tags { "QUEUE"="Transparent" }
  Cull Off
  Blend SrcAlpha OneMinusSrcAlpha
  ZWrite On
  CGPROGRAM
  #pragma vertex vert
  #pragma fragment frag
  #pragma multi_compile_instancing
  #pragma multi_compile_fog
  #include "UnityCG.cginc"
  sampler2D _MainTex;
  float4 _MainTex_ST;
  UNITY_INSTANCING_BUFFER_START(Props)
      UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
  UNITY_INSTANCING_BUFFER_END(Props)
  struct appdata {
      float4 vertex : POSITION;
      float2 uv     : TEXCOORD0;
      fixed4 color  : COLOR;
      UNITY_VERTEX_INPUT_INSTANCE_ID
  };
  struct v2f {
      float4 pos   : SV_POSITION;
      float2 uv    : TEXCOORD0;
      fixed4 color : COLOR;
      UNITY_FOG_COORDS(1)
      UNITY_VERTEX_INPUT_INSTANCE_ID
  };
  v2f vert (appdata v) {
      v2f o;
      UNITY_SETUP_INSTANCE_ID(v);
      UNITY_TRANSFER_INSTANCE_ID(v, o);
      o.pos   = UnityObjectToClipPos(v.vertex);
      o.uv    = TRANSFORM_TEX(v.uv, _MainTex);
      o.color = v.color * UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
      UNITY_TRANSFER_FOG(o, o.pos);
      return o;
  }
  fixed4 frag (v2f i) : SV_Target {
      UNITY_SETUP_INSTANCE_ID(i);
      fixed4 c = tex2D(_MainTex, i.uv);
      c.rgb *= i.color.rgb;
      c.a   *= i.color.a;
      UNITY_APPLY_FOG(i.fogCoord, c);
      return c;
  }
  ENDCG
 }
}
}