Shader "X_Shader/C_Charactar/Fur/FurUnlit"
{
	
	Properties
	{
		_Color("Color", Color) = (1, 1, 1, 1)

		_MainTex("Texture", 2D) = "white" { }
		_FurTex("Fur Pattern", 2D) = "white" { }
		[MaterialToggle]_UseFur ("UseFur",float) = 1
		_FurLength("Fur Length", Range(0.0, 1)) = 0.5
		_FurDensity("Fur Density", Range(0, 2)) = 0.11
		_FurThinness("Fur Thinness", Range(0.01, 50)) = 1
		_FurShading("Fur Shading", Range(0.0, 1)) = 0.25
	}

		Category
		{

			Tags { "RenderType" = "Transparent" "IgnoreProjector" = "True" "Queue" = "Transparent" }
			Cull Off
			ZWrite On
			Blend SrcAlpha OneMinusSrcAlpha

			SubShader
			{
				Pass
				{
					CGPROGRAM
					#pragma vertex vert_base
					#pragma fragment frag_base
					#define FURSTEP 0.05
					#include "CharactarFurCore.cginc"
					ENDCG
				}

				Pass
				{
					CGPROGRAM

					#pragma vertex vert_base
					#pragma fragment frag_base
					#define FURSTEP 0.15
					#include "CharactarFurCore.cginc"

					ENDCG

				}


				Pass
				{
					CGPROGRAM

					#pragma vertex vert_base
					#pragma fragment frag_base
					#define FURSTEP 0.25
					#include "CharactarFurCore.cginc"

					ENDCG

				}

				Pass
				{
					CGPROGRAM

					#pragma vertex vert_base
					#pragma fragment frag_base
					#define FURSTEP 0.35
					#include "CharactarFurCore.cginc"

					ENDCG

				}

				Pass
				{
					CGPROGRAM

					#pragma vertex vert_base
					#pragma fragment frag_base
					#define FURSTEP 0.45
					#include "CharactarFurCore.cginc"

					ENDCG

				}



				Pass
				{
					CGPROGRAM

					#pragma vertex vert_base
					#pragma fragment frag_base
					#define FURSTEP 0.55
					#include "CharactarFurCore.cginc"

					ENDCG

				}


				Pass
				{
					CGPROGRAM

					#pragma vertex vert_base
					#pragma fragment frag_base
					#define FURSTEP 0.65
					#include "CharactarFurCore.cginc"

					ENDCG

				}


				Pass
				{
					CGPROGRAM

					#pragma vertex vert_base
					#pragma fragment frag_base
					#define FURSTEP 0.75
					#include "CharactarFurCore.cginc"

					ENDCG

				}


				Pass
				{
					CGPROGRAM

					#pragma vertex vert_base
					#pragma fragment frag_base
					#define FURSTEP 0.85
					#include "CharactarFurCore.cginc"

					ENDCG

				}

				Pass
				{
					CGPROGRAM

					#pragma vertex vert_base
					#pragma fragment frag_base
					#define FURSTEP 1.00
					#include "CharactarFurCore.cginc"

					ENDCG

				}
			}
		}
}