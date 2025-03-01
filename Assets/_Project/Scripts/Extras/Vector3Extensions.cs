using UnityEngine;

public static class Vector3Extensions
{
	public static float SqrDistance(this Vector3 start, Vector3 end)
	{
		return (end - start).sqrMagnitude;
	}

	public static bool IsEnoughClose(this Vector3 start, Vector3 end, float distance)
	{
		return start.SqrDistance(end) <= distance * distance;
	}
	
	public static Vector3 WithX(this Vector3 vector, float x)
	{
		return new Vector3(x, vector.y, vector.z);
	}

	public static Vector3 WithY(this Vector3 vector, float y)
	{
		return new Vector3(vector.x, y, vector.z);
	}

	public static Vector3 WithZ(this Vector3 vector, float z)
	{
		return new Vector3(vector.x, vector.y, z);
	}
}
