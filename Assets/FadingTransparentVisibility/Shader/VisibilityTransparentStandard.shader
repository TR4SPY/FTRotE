
//http://www.shaderific.com/glsl-functions/
//https://docs.unity3d.com/Manual/SL-SurfaceShaders.html

Shader "Visibility/VisibilityTransparentStandard" {

	Properties {

		//Generic Shader Properties
		_MainTex ("Texture", 2D) = "white" {}
		_Albedo ("Albedo", color) = (1.0,1.0,1.0,1.0)
		_MetallicMap("Metallic Map", 2D) = "white" {}
		_Metallic ("Metallic", Range (0, 1)) = 1
		_Smoothness ("Smoothness", Range (0, 1)) = 1
		[Normal]
		_NormalMap ("Normal map", 2D) = "bump" {}
		_NormalMultiplier ("Normal Multiplier", float) = 1
		_OcclusionMap ("Occlusion", 2D) = "white" {}
		_Occlusion ("Occlusion", Range (0, 1)) = 1
		[Toggle] 
		_EmissionEnabled("Emission Enabled", int) = 0
		_EmissionMap("Emission Map", 2D) = "white" {}
		[HDR] 
		_Emission ("Emission", color) = (0.0,0.0,0.0,1.0)

		//Spacing
		[Space(15)]

		//Custom Properties
		[Toggle] 
		_Override ("Override Viewer Settings", int) = 0
		_VisibleDistance ("Visibility Distance", float) = 8.0
		_CenterAlpha ("Center Alpha", Range (0, 1)) = 1
		_RangeAlpha ("Range Alpha", Range (0, 1)) = 0.8
		_LimitDistance ("Limit Distance", float) = 2
		_LimitAlpha ("Limit Alpha", Range (0, 1)) = 0.1
		_VisibleAngle ("Visibility Angle", Range (0, 360)) = 140
		_AngleAlpha ("Angle Alpha", Range (0, 1)) = 0.1
		_OutlineInternal ("Internal Outline", float) = 0.1
		_OutlineExternal ("External Outline", float) = 0.1
		_OutlineColour ("Outline Colour", color) = (0.0,0.0,0.0,1.0)
		_OutlineAlpha ("Outline Alpha", Range (0, 1)) = 1

		//Hidden Properties
		[HideInInspector]
		_VisibleAngleCosine ("Visibility Angle Cosine", Range (-1, 1)) = 0

	}

	SubShader {

		Tags {  "RenderType" = "Transparent" "Queue" = "Transparent"}
		Blend SrcAlpha OneMinusSrcAlpha
		
		CGPROGRAM

		#pragma target 3.0
		#pragma surface surf Standard keepalpha
		#include "UnityStandardUtils.cginc"

		//Generic Shader Properties
		uniform sampler2D _MainTex;
		uniform float4 _Albedo;
		uniform sampler2D _MetallicMap;
		uniform float _Metallic;
		uniform float _Smoothness;
		uniform sampler2D _NormalMap;
		uniform float _NormalMultiplier;
		uniform sampler2D _OcclusionMap;
		uniform float _Occlusion;
		uniform int _EmissionEnabled;
		uniform sampler2D _EmissionMap;		
		uniform half3 _Emission;

		//Custom Properties
		uniform float _CenterAlpha;
		uniform float _RangeAlpha;
		uniform float _LimitAlpha;
		uniform float _LimitDistance;
		uniform float _AngleAlpha;
		uniform float _VisibleDistance;
		uniform float _VisibleAngle;
		uniform float _OutlineInternal;
		uniform float _OutlineExternal;
		uniform fixed4 _OutlineColour;
		uniform float _OutlineAlpha;

		//Hidden Properties
		uniform float _VisibleAngleCosine;
		
		//Non properties
		uniform float3 _ViewPosition;
		uniform float3 _Forward;

		struct Input {
			float2 uv_MainTex;
			float2 uv_NormalMap;
			float3 worldPos;
			float3 worldNormal;
			INTERNAL_DATA
		};

		//Optimization variants
		#pragma shader_feature BEYOND_BOUNDS
		#pragma shader_feature BEYOND_ANGLE

		void surf (Input IN, inout SurfaceOutputStandard o) {

			//General Properties
			float4 color = tex2D (_MainTex, IN.uv_MainTex);
			color *= _Albedo;
			o.Albedo = color.rgb;

			fixed3 normal = UnpackScaleNormal (tex2D (_NormalMap, IN.uv_NormalMap), _NormalMultiplier);
			o.Normal = normal;

			fixed4 metallic = tex2D(_MetallicMap, IN.uv_MainTex);
			o.Metallic = metallic.r * _Metallic;
			o.Smoothness = metallic.a * _Smoothness;

			fixed4 occlusion = tex2D(_OcclusionMap, IN.uv_MainTex);
			o.Occlusion = occlusion * _Occlusion;

			fixed4 emission = tex2D(_EmissionMap, IN.uv_MainTex);
			if(_EmissionEnabled == 1)
				o.Emission = emission * _Emission;

			/*
				Check bounds optimization
				If closest point of the bounds of an object are beyond the full distance (visible distance + limit distance)
            	then all the pixels of the object are beyond that limit and dont need to calculate an interpolation
				This is calculated in the object script and set to BEYOND_BOUNDS to be used here
			*/
			#if !BEYOND_BOUNDS

				//Distance to view point
				float dist = distance(_ViewPosition.xyz, IN.worldPos);

				//Ratios
				float toRange = dist / _VisibleDistance;
				float toLimit = (dist - _VisibleDistance) / _LimitDistance;			

				//Inside Range
				if (dist < _VisibleDistance - _OutlineInternal){
					o.Alpha = lerp(_CenterAlpha, _RangeAlpha, toRange);
				}
				//Inside Limit
				else if(dist > _VisibleDistance + _OutlineExternal && dist < _VisibleDistance + _LimitDistance){
					o.Alpha = lerp(_RangeAlpha, _LimitAlpha, toLimit);
				}
				//Outline
				else if (dist > _VisibleDistance - _OutlineInternal && dist < _VisibleDistance + _OutlineExternal) {
					o.Alpha = _OutlineAlpha;
					o.Albedo = _OutlineColour.rgb;
				}
				//Beyond limit
				else{
					o.Alpha = _LimitAlpha;
				}
			
			//If bounds beyond limit just apply the limit alpha
			#else
				o.Alpha = _LimitAlpha;
			#endif

			/*
				Check angle optimization
				If the best possible ray towards the center of the bounds of the object doesn't intersect a sphere with bounds.extents radius
            	then all the pixels of the object are beyond the angle and don't need to check individually
				This is calculated in the object script and set to BEYOND_ANGLE to be used here
			*/
			#if !BEYOND_ANGLE

				//No need to calculate any values
				if( _VisibleAngle == 0){
					o.Alpha = _AngleAlpha;
					o.Albedo = color.rgb;
				}
				//Need to compare angle individually
				else if(_VisibleAngle != 360){

					//Direction from view to pixel
					float3 dir = IN.worldPos - _ViewPosition;

					//Cosine of angle between forward of view and direction
					float mag1 = length(dir);
					float mag2 = length(_Forward);
					float angleCos = dot(dir, _Forward)/(mag1*mag2);

					/*
						Compare cosines instead of calculating angle and comparing angles
						This saves an arc cosine to be needed here to find the angle
					*/
					if(angleCos < _VisibleAngleCosine){
						o.Alpha = _AngleAlpha;
						o.Albedo = color.rgb;
					}

				}

			//If angle beyond limit just apply the angle alpha and color because of eventual outline
			#else
				o.Alpha = _AngleAlpha;
				o.Albedo = color.rgb;
			#endif	

		}

		ENDCG
	} 
	Fallback "Diffuse"
}