#if !defined(EASING_INCLUDED)
#define EASING_INCLUDED

#include "UnityCG.cginc"
#define PI UNITY_PI


//NOTICE//
//请务必将变量x控制在[0,1]的区间中
//
inline float EaseLinear(float x)
{
	return x;
}
//Sine
inline float EaseInSine(float x)
{
	return 1 - cos((x * PI) / 2);
}
inline float EaseOutSine(float x)
{
	return sin((x * PI) / 2);
}
inline float EaseInOutSine(float x)
{
	return -(cos(PI * x) - 1) / 2;
}
//Quad
inline float EaseInQuad(float x)
{
	return x * x;
}
inline float EaseOutQuad(float x)
{
	return 1 - (1 - x) * (1 - x);
}
inline float EaseInOutQuad(float x)
{
	return x < 0.5 ? 2 * x * x : 1 - pow(-2 * x + 2, 2) / 2;
}
//Cubic
inline float EaseInCubic(float x)
{
	return x * x * x;
}
inline float EaseOutCubic(float x)
{
	return 1 - pow(1 - x, 3);
}
inline float EaseInOutCubic(float x)
{
	//return x < 0.5 ? 4 * x * x * x : 1 - pow(-2 * x + 2, 3) / 2;
	return lerp(4 * x * x * x,1 - pow(-2 * x + 2, 3) / 2, step(x,0.5));
}
//Quart
inline float EaseInQuart(float x)
{
	return x * x * x * x;
}
inline float EaseOutQuart(float x)
{
	return 1 - pow(1 - x, 4);
}
inline float EaseInOutQuart(float x)
{
	return x < 0.5 ? 8 * x * x * x * x : 1 - pow(-2 * x + 2, 4) / 2;
}
//Quint
inline float EaseInQuint(float x)
{
	return x * x * x * x * x;
}
inline float EaseOutQuint(float x)
{
	return 1 - pow(1 - x, 5);
}
inline float EaseInOutQuint(float x)
{
	return x < 0.5 ? 16 * x * x * x * x * x : 1 - pow(-2 * x + 2, 5) / 2;;
}
//Expo
inline float EaseInExpo(float x)
{
	return x == 0 ? 0 : pow(2, 10 * x - 10);
}
inline float EaseOutExpo(float x)
{
	return x == 1 ? 1 : 1 - pow(2, -10 * x);
}
inline float EaseInOutExpo(float x)
{
	return x == 0 
	? 0 
	: x == 1 
	? 1 
	: x < 0.5 
	? pow(2, 20 * x - 10) / 2 
	: (2 - pow(2, -20 * x + 10)) / 2;
}
//Circ
inline float EaseInCirc(float x)
{
	return 1 - sqrt(1 - pow(x, 2));
}
inline float EaseOutCirc(float x)
{
	return sqrt(1 - pow(x - 1, 2));
}
inline float EaseInOutCirc(float x)
{
	return x < 0.5
  ? (1 - sqrt(1 - pow(2 * x, 2))) / 2
  : (sqrt(1 - pow(-2 * x + 2, 2)) + 1) / 2;
}
//Elastic
inline float EaseInElastic(float x)
{
	float c4 = (2 * PI) / 3;

	return x == 0
	  ? 0
	  : x == 1
	  ? 1
	  : -pow(2, 10 * x - 10) * sin((x * 10 - 10.75) * c4);
}
inline float EaseOutElastic(float x)
{
	float c4 = (2 * PI) / 3;

	return x == 0
	  ? 0
	  : x == 1
	  ? 1
	  : pow(2, -10 * x) * sin((x * 10 - 0.75) * c4) + 1;
}
inline float EaseInOutElastic(float x)
{
	float c5 = (2 * PI) / 4.5;

	return x == 0
	  ? 0
	  : x == 1
	  ? 1
	  : x < 0.5
	  ? -(pow(2, 20 * x - 10) * sin((20 * x - 11.125) * c5)) / 2
	  : (pow(2, -20 * x + 10) * sin((20 * x - 11.125) * c5)) / 2 + 1;
}
//Back
inline float EaseInBack(float x)
{
	float c1 = 1.70158;
	float c3 = c1 + 1;

	return c3 * x * x * x - c1 * x * x;
}
inline float EaseOutBack(float x)
{
	float c1 = 1.70158;
	float c3 = c1 + 1;

	return 1 + c3 * pow(x - 1, 3) + c1 * pow(x - 1, 2);
}
inline float EaseInOutBack(float x)
{
	float c1 = 1.70158;
	float c2 = c1 * 1.525;

	return x < 0.5
	  ? (pow(2 * x, 2) * ((c2 + 1) * 2 * x - c2)) / 2
	  : (pow(2 * x - 2, 2) * ((c2 + 1) * (x * 2 - 2) + c2) + 2) / 2;
}
//Bounce
inline float EaseOutBounce(float x)
{
	float n1 = 7.5625;
	float d1 = 2.75;

	if (x < 1 / d1) {
		return n1 * x * x;
	} else if (x < 2 / d1) {
		return n1 * (x -= 1.5 / d1) * x + 0.75;
	} else if (x < 2.5 / d1) {
		return n1 * (x -= 2.25 / d1) * x + 0.9375;
	} else {
		return n1 * (x -= 2.625 / d1) * x + 0.984375;
	}
}
inline float EaseInBounce(float x)
{
	return 1 - EaseOutBounce(1 - x);
}
inline float EaseInOutBounce(float x)
{
	return x < 0.5
	  ? (1 - EaseOutBounce(1 - 2 * x)) / 2
	  : (1 + EaseOutBounce(2 * x - 1)) / 2;
}

#endif //EASING_INCLUDED