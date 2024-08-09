
HEADER
{
	Description = "";
}

FEATURES
{
	#include "common/features.hlsl"
}

MODES
{
	VrForward();
	Depth(); 
	ToolsVis( S_MODE_TOOLS_VIS );
	ToolsWireframe( "vr_tools_wireframe.shader" );
	ToolsShadingComplexity( "tools_shading_complexity.shader" );
}

COMMON
{
	#ifndef S_ALPHA_TEST
	#define S_ALPHA_TEST 0
	#endif
	#ifndef S_TRANSLUCENT
	#define S_TRANSLUCENT 0
	#endif
	
	#include "common/shared.hlsl"
	#include "procedural.hlsl"

	#define S_UV2 1
	#define CUSTOM_MATERIAL_INPUTS
}

struct VertexInput
{
	#include "common/vertexinput.hlsl"
	float4 vColor : COLOR0 < Semantic( Color ); >;
};

struct PixelInput
{
	#include "common/pixelinput.hlsl"
	float3 vPositionOs : TEXCOORD14;
	float3 vNormalOs : TEXCOORD15;
	float4 vTangentUOs_flTangentVSign : TANGENT	< Semantic( TangentU_SignV ); >;
	float4 vColor : COLOR0;
	float4 vTintColor : COLOR1;
};

VS
{
	#include "common/vertex.hlsl"

	PixelInput MainVs( VertexInput v )
	{
		PixelInput i = ProcessVertex( v );
		i.vPositionOs = v.vPositionOs.xyz;
		i.vColor = v.vColor;

		ExtraShaderData_t extraShaderData = GetExtraPerInstanceShaderData( v );
		i.vTintColor = extraShaderData.vTint;

		VS_DecodeObjectSpaceNormalAndTangent( v, i.vNormalOs, i.vTangentUOs_flTangentVSign );

		return FinalizeVertex( i );
	}
}

PS
{
	#include "common/pixel.hlsl"
	
	SamplerState g_sSampler0 < Filter( ANISO ); AddressU( WRAP ); AddressV( WRAP ); >;
	CreateInputTexture2D( Medium, Srgb, 8, "None", "_color", ",0/,0/0", Default4( 0.45, 0.45, 0.45, 1.00 ) );
	CreateInputTexture2D( Bad, Srgb, 8, "None", "_color", ",0/,0/0", Default4( 1.00, 0.00, 0.00, 1.00 ) );
	CreateInputTexture2D( Nice, Srgb, 8, "None", "_color", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	Texture2D g_tMedium < Channel( RGBA, Box( Medium ), Srgb ); OutputFormat( DXT5 ); SrgbRead( True ); >;
	Texture2D g_tBad < Channel( RGBA, Box( Bad ), Srgb ); OutputFormat( DXT5 ); SrgbRead( True ); >;
	Texture2D g_tNice < Channel( RGBA, Box( Nice ), Srgb ); OutputFormat( DXT5 ); SrgbRead( True ); >;
	float g_flCondition < UiType( Slider ); UiGroup( ",0/,0/0" ); Default1( 0 ); Range1( 0, 1 ); >;
	
	float4 MainPs( PixelInput i ) : SV_Target0
	{
		Material m = Material::Init();
		m.Albedo = float3( 1, 1, 1 );
		m.Normal = float3( 0, 0, 1 );
		m.Roughness = 1;
		m.Metalness = 0;
		m.AmbientOcclusion = 1;
		m.TintMask = 1;
		m.Opacity = 1;
		m.Emission = float3( 0, 0, 0 );
		m.Transmission = 0;
		
		float4 l_0 = Tex2DS( g_tMedium, g_sSampler0, i.vTextureCoords.xy );
		float4 l_1 = Tex2DS( g_tBad, g_sSampler0, i.vTextureCoords.xy );
		float l_2 = g_flCondition;
		float l_3 = l_2 * 2;
		float l_4 = l_3 - 1;
		float4 l_5 = lerp( l_0, l_1, l_4 );
		float4 l_6 = Tex2DS( g_tNice, g_sSampler0, i.vTextureCoords.xy );
		float4 l_7 = lerp( l_6, l_0, l_3 );
		float4 l_8 = l_2 >= 0.5 ? l_5 : l_7;
		
		m.Albedo = l_8.xyz;
		m.Opacity = 1;
		m.Roughness = 1;
		m.Metalness = 0;
		m.AmbientOcclusion = 1;
		
		m.AmbientOcclusion = saturate( m.AmbientOcclusion );
		m.Roughness = saturate( m.Roughness );
		m.Metalness = saturate( m.Metalness );
		m.Opacity = saturate( m.Opacity );

		// Result node takes normal as tangent space, convert it to world space now
		m.Normal = TransformNormal( m.Normal, i.vNormalWs, i.vTangentUWs, i.vTangentVWs );

		// for some toolvis shit
		m.WorldTangentU = i.vTangentUWs;
		m.WorldTangentV = i.vTangentVWs;
        m.TextureCoords = i.vTextureCoords.xy;
		
		return ShadingModelStandard::Shade( i, m );
	}
}
